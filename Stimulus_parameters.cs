using SenseAction.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SenseAction
{
    public enum ReactionLevel
    {
        Light,

        Medium,

        Strong
    }

    public partial class Stimulus_parameters : Form
    {
        private TrackBar _intensitySlider;
        private NumericUpDown _durationSpinner;
        private ComboBox _typeSelector;
        private Label _lblIntensity;
        private Button _btnOk;
        private Button _btnCancel;

        public Stimulus_parameters()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Параметри стимулу";
            this.Size = new Size(300, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _lblIntensity = new Label
            {
                Text = "Інтенсивність: 5",
                Location = new Point(10, 15),
                AutoSize = true
            };
            _intensitySlider = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                Location = new Point(10, 35),
                Width = 250
            };
            _intensitySlider.ValueChanged += (s, e) =>
            {
                _lblIntensity.Text = $"Інтенсивність: {_intensitySlider.Value}";
            };

            Label lblDuration = new Label
            {
                Text = "Тривалість (секунди):",
                Location = new Point(10, 85),
                AutoSize = true
            };
            _durationSpinner = new NumericUpDown
            {
                Minimum = 0.1m,
                Maximum = 10m,
                DecimalPlaces = 1,
                Increment = 0.5m,
                Value = 1.0m,
                Location = new Point(10, 105),
                Width = 250
            };

            Label lblType = new Label
            {
                Text = "Тип стимулу:",
                Location = new Point(10, 145),
                AutoSize = true
            };
            _typeSelector = new ComboBox
            {
                Location = new Point(10, 165),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _typeSelector.Items.Add(StimulusType.Mechanical.ToString());
            _typeSelector.SelectedIndex = 0;

            _btnOk = new Button
            {
                Text = "Підтвердити",
                Location = new Point(35, 215),
                Width = 100,
                DialogResult = DialogResult.OK
            };
            _btnCancel = new Button
            {
                Text = "Скасувати",
                Location = new Point(145, 215),
                Width = 100,
                DialogResult = DialogResult.Cancel
            };

            this.AcceptButton = _btnOk;
            this.CancelButton = _btnCancel;

            this.Controls.Add(_lblIntensity);
            this.Controls.Add(_intensitySlider);
            this.Controls.Add(lblDuration);
            this.Controls.Add(_durationSpinner);
            this.Controls.Add(lblType);
            this.Controls.Add(_typeSelector);
            this.Controls.Add(_btnOk);
            this.Controls.Add(_btnCancel);
        }

        public float GetIntensity() => _intensitySlider.Value;
        public float GetDuration() => (float)_durationSpinner.Value;

        public StimulusType GetStimulusType()
        {
            return StimulusType.Mechanical;
        }

        public ReactionLevel GetReactionLevel()
        {
            float totalPower = GetIntensity() * GetDuration();
            if (totalPower <= 15f) return ReactionLevel.Light;
            if (totalPower <= 40f) return ReactionLevel.Medium;
            return ReactionLevel.Strong;
        }

        private void Stimulus_parameters_Load(object sender, EventArgs e) { }
    }
}