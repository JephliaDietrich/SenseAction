using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenseAction.Rendering
{
    public class SignalAnimator
    {
        private readonly PictureBox _canvas;
        private readonly Timer _timer;
        private Point[] _path;
        private int _currentStep;
        private bool _travelling;
        private Action _onComplete;
        private float _speed = 1f;

        private const int SIGNAL_RADIUS = 8;
        private const int BASE_INTERVAL = 30;
        private Color _signalColor = Color.Yellow;

        public bool IsPlaying => _travelling;

        public SignalAnimator(PictureBox canvas)
        {
            _canvas = canvas;
            _timer = new Timer();
            _timer.Interval = BASE_INTERVAL;
            _timer.Tick += OnTick;
        }

        public void Play(Point[] signalPath, Color pathColor, Action onComplete = null)
        {
            _path = signalPath;
            _signalColor = pathColor;
            _currentStep = 0;
            _travelling = true;
            _onComplete = onComplete;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _travelling = false;
            _currentStep = 0;
            _canvas.Invalidate();
        }

        public void Pause() => _timer.Stop();

        public void Resume()
        {
            if (_travelling) _timer.Start();
        }

        public void SetSpeed(float speed)
        {
            _speed = Math.Max(0.1f, speed);
            _timer.Interval = Math.Max(10, (int)(BASE_INTERVAL / _speed));
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_path == null || _currentStep >= _path.Length - 1)
            {
                _timer.Stop();
                _travelling = false;
                _onComplete?.Invoke();
                return;
            }
            _currentStep++;
            _canvas.Invalidate();
        }

        public void Draw(Graphics g)
        {
            if (!_travelling || _path == null) return;

            Point pos = _path[_currentStep];

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(80, _signalColor.R, _signalColor.G, _signalColor.B)))
                g.FillEllipse(glow,
                    pos.X - SIGNAL_RADIUS * 2,
                    pos.Y - SIGNAL_RADIUS * 2,
                    SIGNAL_RADIUS * 4,
                    SIGNAL_RADIUS * 4);

            using (SolidBrush core = new SolidBrush(Color.FromArgb(220, _signalColor.R, _signalColor.G, _signalColor.B)))
                g.FillEllipse(core,
                    pos.X - SIGNAL_RADIUS,
                    pos.Y - SIGNAL_RADIUS,
                    SIGNAL_RADIUS * 2,
                    SIGNAL_RADIUS * 2);
        }
    }
}