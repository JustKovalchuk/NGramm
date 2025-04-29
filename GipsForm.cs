using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NGramm
{
    public partial class GipsForm : Form
    {
        private int tabIndex;
        private readonly int pageSize;
        private int Ngramm;
        private readonly NgrammProcessor processor;
        private Dictionary<int, double> hips = new Dictionary<int, double>();
        private Dictionary<int, double> hipssq = new Dictionary<int, double>();
        bool lnx;
        bool lny;

        public GipsForm(int tabIndex, int pageSize, int N, NgrammProcessor processor)
        {
            InitializeComponent();
            chart1.Series.Clear();
            this.tabIndex = tabIndex;
            this.pageSize = pageSize;
            Ngramm = N;
            this.processor = processor;
            chart1.Series.Add("Heaps");

            textBox5.Text = pageSize.ToString();
            textBox6.Text = pageSize.ToString();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            label6.Text = "Підрахунок...";
            int step = int.Parse(textBox1.Text);
            int window_grow = int.Parse(textBox3.Text);
            int initialWindowSize = int.Parse(textBox2.Text);
            int start_pos = int.Parse(textBox4.Text);
            int end_pos = int.Parse(textBox6.Text);
            List<int> lst = new List<int>();
            hips.Clear();
            hipssq.Clear();
            bool fixed_window = checkBox1.Checked;
            bool fixedWindownotav = checkBox3.Checked;
            bool fixed_pos = checkBox2.Checked;

            if (fixed_window || fixedWindownotav)
            {
                end_pos = initialWindowSize;
            }

            await Task.Run(() =>
            {
                var words = Array.Empty<string>();
                int maxWindow = 0;
                switch (tabIndex)
                {
                    case 0:
                        maxWindow = processor.unsignedTextorg.Substring(start_pos, end_pos - start_pos).Length;
                        break;
                    case 1:
                        maxWindow = processor.rawTextorg.Substring(start_pos, end_pos - start_pos).Length;
                        break;
                    case 2:
                        words = processor.WordsNotStatic();
                        maxWindow = words.Length;
                        break;
                }

             


                var sizes = (maxWindow - initialWindowSize) / window_grow;
                bool rep;
                int incPerStep = 1, stepsPerProg = 1;
                if (sizes > 90)
                {
                    rep = true;
                    stepsPerProg = sizes / 90;
                }
                else
                {
                    rep = false;
                    incPerStep = 90 / sizes;
                }

                processor.progressReporter.StartNewOperation("Обрахунок гіпс");
                processor.progressReporter.MoveProgress(5);
                var ngramsInWindows = Partitioner.Create(initialWindowSize, maxWindow + 1, window_grow)
                    .AsParallel()
                    .WithDegreeOfParallelism(PerformanceSettings.Cores)
                    .Select(part =>
                    {
                        var ff = fixed_pos ? part.Item1 : end_pos;
                        var res = (0, Array.Empty<int>());
                        switch (tabIndex)
                        {
                            case 0:
                                res = (part.Item1, processor.ProcessLiteralNGrammsInWindow(Ngramm, part.Item1, step, start_pos, ff)
                                .Select(p => p.count).ToArray());
                                break;
                            case 1:
                                res = (part.Item1, processor.ProcessSymbolNGrammsInWindow(Ngramm, part.Item1, step, start_pos, ff)
                                .Select(p => p.count).ToArray());
                                break;
                            case 2:
                                ff = fixed_pos ? part.Item1 : maxWindow;
                                res = (part.Item1, processor.ProcessWordNGrammsInWindow(words, Ngramm, part.Item1, step, start_pos, ff)
                                .Select(p => p.count).ToArray());
                                break;
                        }

                        if (rep && (part.Item1 - initialWindowSize) / window_grow % stepsPerProg == 0)
                        {
                            processor.progressReporter.MoveProgress();
                        }
                        else
                        {
                            processor.progressReporter.MoveProgress(incPerStep);
                        }
                        return res;
                    }).ToArray();

                ngramsInWindows = ngramsInWindows.OrderBy(v => v.Item1).ToArray();
                processor.progressReporter.Finish();

                if (fixed_window)
                {
                    //this executes only once
                    int h = start_pos + initialWindowSize;
                    var arr = ngramsInWindows[0].Item2;

                    for (int i = 0; i < arr.Length; i++)
                    {
                        hips.Add(h, arr[i]);
                        hipssq.Add(h, arr[i] * arr[i]);
                        h += step;
                    }
                }
                else
                {
                    foreach (var wn in ngramsInWindows)
                    {
                        long sum = 0;
                        long sq_sum = 0;
                        foreach (int item in wn.Item2)
                        {
                            sum += item;
                            sq_sum += (item * item);
                        }

                        double lstCount = wn.Item2.Length;
                        hips.Add(wn.Item1, sum / lstCount);
                        hipssq.Add(wn.Item1, sq_sum / lstCount);
                    }

                }
            });
            
            //TODO: add support when initialWindowSize is less than finalWindowSize.
            //If you don't add this support then don't change conditions of this circle

            lnx = true;
            lny = true;
            chart1.Series.Clear();
            string tmp_series = "hips";
            chart1.Series.Add(tmp_series);
            chart1.Series[tmp_series].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            listView1.Items.Clear();
            foreach (int item in hips.Keys)
            {
                chart1.Series[tmp_series].Points.AddXY(Math.Log(item), Math.Log(hips[item]));
                ListViewItem nli = new ListViewItem(item.ToString());
                nli.SubItems.Add(hips[item].ToString());
                nli.SubItems.Add(Math.Sqrt(hipssq[item] - hips[item] * hips[item]).ToString());
                listView1.Items.Add(nli);
            }
            drawChart();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string folder = saveFileDialog1.FileName;
                StreamWriter wr = new StreamWriter(folder);
                wr.WriteLine("L\tV\tdV");

                foreach (int item in hips.Keys)
                {
                    wr.WriteLine(item.ToString() + "\t" + hips[item].ToString() + "\t" + Math.Sqrt(hipssq[item] - hips[item] * hips[item]).ToString());
                }
                wr.Close();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void GipsForm_Load(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(textBox5.Text) < Convert.ToInt32(textBox6.Text) + Convert.ToInt32(textBox4.Text))
            {
                textBox6.Text = (Convert.ToInt32(textBox5.Text) - Convert.ToInt32(textBox4.Text)).ToString();
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            this.lny = checkBox5.Checked;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            drawChart();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            this.lnx = checkBox4.Checked;
        }

        private void drawChart()
        {
            chart1.Series.Clear();
            string tmp_series = "Heaps";
            chart1.Series.Add(tmp_series);
            chart1.Series[tmp_series].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            foreach (int item in hips.Keys)
            {
                chart1.Series[tmp_series].Points.AddXY(lnx ? Math.Log10(item) : item, lny ? Math.Log10(hips[item]) : hips[item]);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}
