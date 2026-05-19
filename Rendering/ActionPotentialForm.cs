using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SenseAction.Rendering
{
    public class ActionPotentialForm : Form
    {
        private Chart _chart;

        public ActionPotentialForm()
        {
            InitializeChart();
        }

        private void InitializeChart()
        {
            this.Text = "Потенціал дії (Мембранний потенціал)";
            this.Size = new Size(600, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true; 

            _chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };

            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisY.Minimum = -90; 
            chartArea.AxisY.Maximum = 40;
            chartArea.AxisY.Title = "Мембранний потенціал (мВ)";
            chartArea.AxisX.Title = "Час (с)";

            chartArea.AxisX.LabelStyle.Format = "0.0";
            // Налаштування сітки
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            StripLine thresholdLine = new StripLine
            {
                IntervalOffset = -55,
                StripWidth = 0.5,
                BackColor = Color.Orange,
                Text = "Поріг",
                TextAlignment = StringAlignment.Near
            };
            chartArea.AxisY.StripLines.Add(thresholdLine);

            _chart.ChartAreas.Add(chartArea);

            Series series = new Series("Voltage")
            {
                ChartType = SeriesChartType.Spline,
                Color = Color.Crimson,
                BorderWidth = 3
            };
            _chart.Series.Add(series);

            this.Controls.Add(_chart);
        }

        public void UpdateGraph(float time, double voltage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateGraph(time, voltage)));
                return;
            }

            _chart.Series[0].Points.AddXY(time, voltage);

            if (_chart.Series[0].Points.Count > 100)
            {
                _chart.Series[0].Points.RemoveAt(0);
                _chart.ChartAreas[0].AxisX.Minimum = _chart.Series[0].Points[0].XValue;
                _chart.ChartAreas[0].AxisX.Maximum = time + 0.1;
            }
        }

        public void ClearGraph()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ClearGraph));
                return;
            }
            _chart.Series[0].Points.Clear();
            _chart.ChartAreas[0].AxisX.Minimum = double.NaN;
            _chart.ChartAreas[0].AxisX.Maximum = double.NaN;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ActionPotentialForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "ActionPotentialForm";
            this.ResumeLayout(false);

        }

    }
}