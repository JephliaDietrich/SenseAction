using SenseAction.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SenseAction.Rendering
{
    public sealed class HandFrameAnimator : IDisposable
    {
        private const int BASE_INTERVAL_MS = 250;
        private const string FRAMES_ROOT = @"Resources\Frames";
        private const string BASE_FRAME_PATH = @"Resources\arm_anterior.png";

        private readonly PictureBox _canvas;
        private readonly Timer _timer;
        private readonly Bitmap _baseFrame;
        private readonly Dictionary<ReactionLevel, HandFrameSet> _frameSets;

        private Bitmap _currentFrame;
        private int _step;
        private HandFrameSet _activeSet;
        private float _speed = 1f;

        public event Action OnSequenceFinished;

        public Bitmap CurrentFrame => _currentFrame;

        public bool IsBaseFrame => _currentFrame == _baseFrame;
        public HandFrameAnimator(PictureBox canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));

            _baseFrame = LoadBitmapOrNull(BASE_FRAME_PATH);
            _currentFrame = _baseFrame;

            _frameSets = LoadAllFrameSets();

            _timer = new Timer { Interval = BASE_INTERVAL_MS };
            _timer.Tick += OnTick;
        }

        public void PlaySequence(ReactionLevel level)
        {
            if (!_frameSets.TryGetValue(level, out HandFrameSet set))
            {
                OnSequenceFinished?.Invoke();
                return;
            }

            _activeSet = set;
            _step = 0;
            _timer.Interval = Math.Max(30, (int)(BASE_INTERVAL_MS / _speed));
            _timer.Start();
        }

        public void Reset()
        {
            _timer.Stop();
            _activeSet = null;
            _step = 0;
            _currentFrame = _baseFrame;
            _canvas.Invalidate();
        }

        public void SetSpeed(float speed)
        {
            _speed = Math.Max(0.1f, speed);
            if (_timer.Enabled)
                _timer.Interval = Math.Max(30, (int)(BASE_INTERVAL_MS / _speed));
        }

        private void OnTick(object sender, EventArgs e)
        {
            _step++;

            switch (_step)
            {
                case 1:
                    _currentFrame = _activeSet.TransitionFrame ?? _baseFrame;
                    _canvas.Invalidate();
                    break;

                case 2:
                    _currentFrame = _activeSet.FinalFrame ?? _baseFrame;
                    _canvas.Invalidate();
                    break;

                default:
                    _timer.Stop();
                    OnSequenceFinished?.Invoke();
                    break;
            }
        }

        private Dictionary<ReactionLevel, HandFrameSet> LoadAllFrameSets()
        {
            var dict = new Dictionary<ReactionLevel, HandFrameSet>();

            var folderMap = new Dictionary<ReactionLevel, string>
            {
                { ReactionLevel.Light,  "Weak"   },
                { ReactionLevel.Medium, "Medium" },
                { ReactionLevel.Strong, "Strong" }
            };

            foreach (var kvp in folderMap)
            {
                string folder = Path.Combine(FRAMES_ROOT, kvp.Value);
                Bitmap f2 = LoadBitmapOrNull(Path.Combine(folder, "frame2.png"));
                Bitmap f3 = LoadBitmapOrNull(Path.Combine(folder, "frame3.png"));
                dict[kvp.Key] = new HandFrameSet(_baseFrame, f2, f3);
            }

            return dict;
        }

        private static Bitmap LoadBitmapOrNull(string path)
        {
            try
            {
                return File.Exists(path) ? new Bitmap(path) : null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();

            foreach (var set in _frameSets.Values)
                set.Dispose();

            _baseFrame?.Dispose();
        }
    }
}