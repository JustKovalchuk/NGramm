using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NGramm
{
    public partial class StatisticsWindow : Form
    {
        private Statistics stats;
        private int n;
        private bool logx;
        private bool logy;
        public StatisticsWindow()
        {
            InitializeComponent();
            logx = true;
            logy = true;
        }

        public void SetStats(Statistics stats, int n)
        {
            this.stats = stats;
            this.n = n;
            string tmp_series;
            Dictionary<int, double> tmp_zipf;
            tmp_series = (n + 1).ToString() + "-grams";
            chart1.Series.Add(tmp_series);
            chart1.Series[tmp_series].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            tmp_zipf = stats.GetZipf1StatsL();

            foreach (int item in tmp_zipf.Keys)
            {
                chart1.Series[tmp_series].Points.AddXY(logx?Math.Log10(item):item, logy?Math.Log10(tmp_zipf[item]): tmp_zipf[item]);

            }

            Dictionary<double, double> tmp_2zipf;
            tmp_series = (n + 1).ToString() + "-grams";
            chart2.Series.Add(tmp_series);
            chart2.Series[tmp_series].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            tmp_2zipf = stats.GetZipf2Stats();
            foreach (double item in tmp_2zipf.Keys)
            {
                chart2.Series[tmp_series].Points.AddXY(logx ? Math.Log10(item) : item, logy ? Math.Log10(tmp_2zipf[item]) : tmp_2zipf[item]);
            }

            Dictionary<double, double> tmp_pareto;
            tmp_series = (n + 1).ToString() + "-grams";
            chart3.Series.Add(tmp_series);
            chart3.Series[tmp_series].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            tmp_pareto = stats.GetParetroStats();
            foreach (double item in tmp_pareto.Keys)
            {
                chart3.Series[tmp_series].Points.AddXY(logx ? Math.Log10(item) : item, logy ? Math.Log10(tmp_pareto[item]) : tmp_pareto[item]);
            }

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            logx = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            logy = checkBox2.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
            chart2.Series.Clear();
            chart3.Series.Clear();
            this.SetStats(this.stats, this.n);
        }
    }
}
