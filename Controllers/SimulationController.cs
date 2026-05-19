using SenseAction.Core;
using SenseAction.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SenseAction.Controllers
{
    public class SimulationController
    {
        private readonly HandRenderer _renderer;
        private readonly SignalAnimator _animator;
        private readonly HandFrameAnimator _frameAnimator;
        private readonly PictureBox _canvas;

        private bool _isRunning;
        private bool _isPaused;
        private float _speed = 1f;

        public event Action OnAnimationFinished;
        public bool IsRunning => _isRunning;

        public bool IsFinished { get; private set; }

        private readonly Dictionary<string, Point[]> _nervePaths =
            new Dictionary<string, Point[]>();

        public SimulationController( HandRenderer renderer, SignalAnimator animator, HandFrameAnimator frameAnimator, PictureBox canvas)
        {
            _renderer = renderer;
            _animator = animator;
            _frameAnimator = frameAnimator;
            _canvas = canvas;

            _frameAnimator.OnSequenceFinished += HandleFrameSequenceFinished;
        }

        public void RebuildNervePaths()
        {
            float sx = NerveZoneBuilder.ScaleX;
            float sy = NerveZoneBuilder.ScaleY;
            Point cnsPoint = NerveZoneBuilder.GetCNSPoint();

            Point[] radialMain =
                AppendCNS(SwcReader.LoadPath(@"Resources\Nerves\Radial_MainPath.swc", sx, sy), cnsPoint);
            Point[] radialLat =
                SwcReader.LoadPath(@"Resources\Nerves\Radial_LateralBranch.swc", sx, sy);
            Point[] medianMain =
                AppendCNS(SwcReader.LoadPath(@"Resources\Nerves\Median_MiddleBranch.swc", sx, sy), cnsPoint);
            Point[] ulnarMain =
                AppendCNS(SwcReader.LoadPath(@"Resources\Nerves\Ulnar_MainPath.swc", sx, sy), cnsPoint);
            Point[] ulnarLittle =
                SwcReader.LoadPath(@"Resources\Nerves\Ulnar_LittleBranch.swc", sx, sy);

            Point[] fullRadial = radialLat.Concat(radialMain).ToArray();
            Point[] fullUlnar = ulnarLittle.Concat(ulnarMain).ToArray();

            _nervePaths.Clear();
            _nervePaths["Median nerve — palmar branch"] = medianMain;
            _nervePaths["Ulnar nerve — palmar branch"] = fullUlnar;
            _nervePaths["Radial nerve — superficial branch (C6-8)"] = fullRadial;
            _nervePaths["Lateral antebrachial cutaneous nerve (C5-6)"] = fullRadial;
            _nervePaths["Radial nerve — dorsal antebrachial cutaneous (C5-6)"] = radialMain;
            _nervePaths["Medial antebrachial cutaneous nerve (C8-T1)"] = ulnarMain;
            _nervePaths["Medial brachial cutaneous nerve (T1-2)"] = ulnarMain;
            _nervePaths["Intercostobrachial nerve (T2)"] = ulnarMain;
            _nervePaths["Axillary nerve — upper lateral cutaneous (C5-6)"] = radialMain;
            _nervePaths["Supraclavicular nerves (C3-4)"] = medianMain;
        }

        private static Point[] AppendCNS(Point[] path, Point cnsPoint)
        {
            if (path == null || path.Length == 0)
                return new[] { cnsPoint };

            var result = new Point[path.Length + 1];
            result[0] = cnsPoint;
            Array.Copy(path, 0, result, 1, path.Length);
            return result;
        }

        private void AddInterpolated(List<Point> list, Point start, Point end, int stride)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < stride) return;

            int steps = (int)(dist / stride);
            for (int i = 1; i < steps; i++)
            {
                list.Add(new Point(
                    start.X + (int)(dx * i / steps),
                    start.Y + (int)(dy * i / steps)
                ));
            }
        }

        private List<Point> SmoothPath(List<Point> points, int iterations)
        {
            if (points.Count < 3) return points;

            List<Point> current = new List<Point>(points);
            List<Point> next = new List<Point>(points.Count);

            for (int iter = 0; iter < iterations; iter++)
            {
                next.Clear();
                next.Add(current[0]);

                for (int i = 1; i < current.Count - 1; i++)
                {
                    int x = (current[i - 1].X + current[i].X * 2 + current[i + 1].X) / 4;
                    int y = (current[i - 1].Y + current[i].Y * 2 + current[i + 1].Y) / 4;
                    next.Add(new Point(x, y));
                }

                next.Add(current[current.Count - 1]);

                var temp = current;
                current = next;
                next = temp;
            }
            return current;
        }
        public bool StartSimulation(float intensity, StimulusType type, ReactionLevel reactionLevel, Point clickPoint)
        {
            if (_isRunning || IsFinished) return false;
            if (type != StimulusType.Mechanical) return false;

            string nerve = _renderer.GetClickedZone(clickPoint);
            if (nerve == null || !_nervePaths.ContainsKey(nerve)) return false;

            Point[] swcPath = _nervePaths[nerve];

            int closestIndex = 0;
            double minDist = double.MaxValue;
            for (int i = 0; i < swcPath.Length; i++)
            {
                double dx = swcPath[i].X - clickPoint.X;
                double dy = swcPath[i].Y - clickPoint.Y;

                double penaltyDist = (dx * dx * 10.0) + (dy * dy);

                if (penaltyDist < minDist)
                {
                    minDist = penaltyDist;
                    closestIndex = i;
                }
            }

            int segmentStart = closestIndex;
            while (segmentStart > 0)
            {
                double dist = Math.Pow(swcPath[segmentStart].X - swcPath[segmentStart - 1].X, 2) +
                              Math.Pow(swcPath[segmentStart].Y - swcPath[segmentStart - 1].Y, 2);
                if (dist > 4000) break;
                segmentStart--;
            }

            int stride = Math.Max(1, (int)Math.Round(15 * _speed));
            Point cnsPoint = NerveZoneBuilder.GetCNSPoint();

            var rawPath = new List<Point> { clickPoint };

            AddInterpolated(rawPath, clickPoint, swcPath[closestIndex], stride);

            for (int i = closestIndex; i >= segmentStart; i -= stride)
            {
                if (rawPath[rawPath.Count - 1] != swcPath[i])
                    rawPath.Add(swcPath[i]);
            }

            if (rawPath[rawPath.Count - 1] != swcPath[segmentStart])
                rawPath.Add(swcPath[segmentStart]);

            AddInterpolated(rawPath, rawPath[rawPath.Count - 1], cnsPoint, stride);

            if (rawPath[rawPath.Count - 1] != cnsPoint)
                rawPath.Add(cnsPoint);

            var pathToCNSList = SmoothPath(rawPath, 20);
            var pathToMuscleList = new List<Point>(pathToCNSList);
            pathToMuscleList.Reverse();

            _isRunning = true;
            _isPaused = false;

            _animator.Play(pathToCNSList.ToArray(), Color.Yellow, () =>
            {
                _animator.Play(pathToMuscleList.ToArray(), Color.Red, () =>
                {
                    _frameAnimator.PlaySequence(reactionLevel);
                });
            });

            return true;
        }
        private void HandleFrameSequenceFinished()
        {
            _isRunning = false;
            IsFinished = true;
            OnAnimationFinished?.Invoke();
        }

        public void Resume()
        {
            if (!_isPaused) return;
            _isPaused = false;
            _animator.Resume();
        }

        public void Pause()
        {
            if (!_isRunning) return;
            _isPaused = true;
            _animator.Pause();
        }

        public void Stop()
        {
            _animator.Stop();
            _frameAnimator.Reset();
            _isRunning = false;
            _isPaused = false;
            IsFinished = false;
            _canvas.Invalidate();
        }

        public void Reset() => Stop();

        public void SetSpeed(float speed)
        {
            _speed = speed;
            _animator.SetSpeed(speed);
            _frameAnimator.SetSpeed(speed);
        }
    }
}
