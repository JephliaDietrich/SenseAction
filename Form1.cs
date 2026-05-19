using SenseAction.Controllers;
using SenseAction.Core;
using SenseAction.Rendering;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SenseAction
{
    public partial class Form1 : Form
    {
        private HandRenderer _handRenderer;
        private SignalAnimator _signalAnimator;
        private HandFrameAnimator _handFrameAnimator;
        private SimulationController _controller;

        private Timer _uiTimer;
        private float _elapsedTime = 0f;

        private float _currentIntensity = 5f;
        private float _currentDuration = 1.0f;
        private StimulusType _currentStimulusType = StimulusType.Mechanical;
        private ReactionLevel _currentReaction = ReactionLevel.Medium;

        private ActionPotentialForm _graphForm;
        private FitzHughNagumoModel _neuronModel;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            _neuronModel = new FitzHughNagumoModel();
            _graphForm = new ActionPotentialForm();
            _graphForm.Owner = this;

            _handFrameAnimator = new HandFrameAnimator(pictureBox1);
            _handRenderer = new HandRenderer(_handFrameAnimator);
            _signalAnimator = new SignalAnimator(pictureBox1);
            _controller = new SimulationController(
                _handRenderer,
                _signalAnimator,
                _handFrameAnimator,
                pictureBox1);

            _controller.OnAnimationFinished += () =>
            {
                if (!this.IsHandleCreated) return;
                this.Invoke(new Action(() => _uiTimer.Stop()));
            };

            _uiTimer = new Timer { Interval = 100 };
            _uiTimer.Tick += UiTimer_Tick;

            toolStripButton1.Click += (s, e) =>
            {
                _controller.Reset();         
                _uiTimer.Stop();
                _elapsedTime = 0f;
                toolStripTextBox1.Text = "0.00";

                _neuronModel.Reset();
                _graphForm.ClearGraph();

                pictureBox1.Invalidate();
            };

            toolStripButton2.Click += (s, e) =>
            {
                _controller.Resume();
                _uiTimer.Start();
            };
            toolStripButton3.Click += (s, e) =>
            {
                _controller.Pause();
                _uiTimer.Stop();
            };

            trackBar1.ValueChanged += (s, e) =>
            {
                float speed = (trackBar1.Value + 1) * 0.5f;
                label2.Text = speed.ToString("0.00");
                _controller.SetSpeed(speed);
            };

            pictureBox1.Paint += OnPaint;
            pictureBox1.MouseClick += OnMouseClick;
            pictureBox1.Resize += OnPictureBoxResize;
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            _elapsedTime += 0.1f;
            toolStripTextBox1.Text = _elapsedTime.ToString("0.00");

            bool isStimulusActive = _controller.IsRunning && _elapsedTime <= _currentDuration;

            double currentI = isStimulusActive ? (_currentIntensity * 0.08) : 0.0;

            _neuronModel.Step(currentI);

            if (_graphForm != null && !_graphForm.IsDisposed && _graphForm.Visible)
            {
                _graphForm.UpdateGraph(_elapsedTime, _neuronModel.MembranePotential_mV);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            RefreshCanvasScale();
        }

        private void OnPictureBoxResize(object sender, EventArgs e)
        {
            RefreshCanvasScale();
        }

        private void RefreshCanvasScale()
        {
            int w = pictureBox1.Width;
            int h = pictureBox1.Height;
            if (w <= 0 || h <= 0) return;

            NerveZoneBuilder.SetCanvasSize(w, h);
            _handRenderer.RebuildZones();
            _controller.RebuildNervePaths();
            pictureBox1.Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(pictureBox1.BackColor);
            _handRenderer.Draw(e.Graphics, pictureBox1.Width, pictureBox1.Height);
            _signalAnimator.Draw(e.Graphics);
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (_controller.IsRunning || _controller.IsFinished) return;

            bool simulationStarted = _controller.StartSimulation(
                _currentIntensity,
                _currentStimulusType,
                _currentReaction,
                e.Location);

            if (simulationStarted)
            {
                _elapsedTime = 0f;
                toolStripTextBox1.Text = "0.00";

                _neuronModel.Reset();

                if (_graphForm != null && !_graphForm.IsDisposed && _graphForm.Visible)
                {
                    _graphForm.ClearGraph();
                    _graphForm.UpdateGraph(0f, _neuronModel.MembranePotential_mV);
                }

                _uiTimer.Start();
            }
        }


        private void stilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var stimulusWindow = new Stimulus_parameters())
            {
                if (stimulusWindow.ShowDialog(this) == DialogResult.OK)
                {
                    _currentIntensity = stimulusWindow.GetIntensity();
                    _currentDuration = stimulusWindow.GetDuration();
                    _currentStimulusType = stimulusWindow.GetStimulusType();
                    _currentReaction = stimulusWindow.GetReactionLevel();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Text = "0.50";
            toolStripTextBox1.Text = "0.00";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _handRenderer?.Dispose();
            _handFrameAnimator?.Dispose();
            base.OnFormClosed(e);
        }

        private void showGraphicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_graphForm == null || _graphForm.IsDisposed)
            {
                _graphForm = new ActionPotentialForm();
                _graphForm.Owner = this; 
            }

            if (_graphForm.WindowState == FormWindowState.Minimized)
            {
                _graphForm.WindowState = FormWindowState.Normal;
            }

            _graphForm.Show();
            _graphForm.Activate();
        }
    }
}