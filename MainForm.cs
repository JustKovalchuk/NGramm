using NGramm.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NGramm
{
    public partial class MainForm : Form
    {
        // uncomment to see console output
        // [DllImport("kernel32.dll")]
        // private static extern bool AllocConsole();

        bool _isProgramText = false;
        NgrammProcessor processor;
        string filename;
        NGrammContainer unified;
        readonly ProgressReporter reporter;
        int IndexPorah = -1;
        readonly Stopwatch stopwatch = new Stopwatch();
        readonly Action<SendOrPostCallback, object> SyncContext;

        private readonly NGramListSettings nGramListSettings = new NGramListSettings();

        public MainForm()
        {
            InitializeComponent();
            numericUpDown1.Maximum = PerformanceSettings.MaxCores;
            numericUpDown1.Value = PerformanceSettings.MaxCores;
            numericUpDown2.Value = PerformanceSettings.MinNGrammCount;
            EndSignsList.Text = new string(NgrammProcessor.endsigns.ToArray());
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            tabControl1.SelectTab(tabPage2);
            tabControl1.Selecting += new TabControlCancelEventHandler(tabControl1_Selected);
            reporter = new ProgressReporter();
            reporter.ProgressChanged += Reporter_ProgressChanged;
            reporter.TimerStartRequest += Reporter_TimerStartRequest;
            reporter.TimerStopRequest += Reporter_TimerStopRequest;
            reporter.OperationNameChanged += Reporter_OperationNameChanged;
            SyncContext = SynchronizationContext.Current.Post;

            PrepareMeCabResources();
            PrepareJiebaResources();
            
            // uncomment to see console output
            // AllocConsole();
            // Console.WriteLine("Console is active");
        }

        private void EditRegisterCheckbox()
        {
            var selectedtab = (Tabs)tabControl1.SelectedIndex;
            
            if (selectedtab == Tabs.Words && _isProgramText)
            {
                IgnoreRegisterChecbox.Enabled = false;
                IgnoreRegisterChecbox.Hide();
            }
            else
            {
                IgnoreRegisterChecbox.Enabled = true;
                IgnoreRegisterChecbox.Show();
            }
        }

        private void EditRemoveCommentsCheckbox()
        {
            if (!(processor is CodeNgrammProcessor))
                return;
            
            if (((CodeNgrammProcessor)processor).CanRemoveComments)
            {
                checkBoxComments.Enabled = true;
                checkBoxComments.Show();
            }
            else
            {
                checkBoxComments.Enabled = false;
                checkBoxComments.Hide();
            }
        }

        private async Task OpenFile(bool isProgramText=false)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (isProgramText)
                    processor = new CodeNgrammProcessor(openFileDialog1.FileName, reporter, checkBoxComments.Checked, checkBoxStrings.Checked);
                else
                    processor = new NgrammProcessor(openFileDialog1.FileName, reporter);
                filename = Path.GetFileName(openFileDialog1.FileName);
                await processor.Preprocess();
                var textType = isProgramText ? "програмний" : "природний";
                toolStripStatusLabel1.Text = $"Текст ({textType}): " + Path.GetFileName(openFileDialog1.FileName);
                еуіеToolStripMenuItem.Text = $"L={File.ReadAllText(openFileDialog1.FileName).Length}";
                порахуватиToolStripMenuItem.Enabled = true;
                groupBox4.Enabled = true;
                groupBox1.Enabled = true;
                IndexPorah = tabControl1.SelectedIndex;
            }
            else
            {
                groupBox4.Enabled = false;
                groupBox1.Enabled = false;
                порахуватиToolStripMenuItem.Enabled = false;
            }
            _isProgramText = isProgramText;
            EditRegisterCheckbox();
            
            if (isProgramText)
                if (processor is CodeNgrammProcessor && !((CodeNgrammProcessor)processor).CanRemoveComments)
                    MessageBox.Show("Програма не підтримує коментарі даного типу файлу!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void PrepareMeCabResources()
        {
            string tempDictPath = Path.Combine(Path.GetTempPath(), "MeCab_dict");
            Directory.CreateDirectory(tempDictPath);

            List<string> fileList = new List<string>() { "char.bin", "dicrc", "matrix.bin", "sys.dic", "unk.dic" };

            foreach (var file in fileList)
            {
                if (!File.Exists(Path.Combine(tempDictPath, file)))
                    File.WriteAllBytes(Path.Combine(tempDictPath, file), ReadEmbeddedResourceBin(file, "dic.ipadic"));
            }

        }
        private void PrepareJiebaResources()
        {
            string tempDictPath = Path.Combine(Path.GetTempPath(), "jieba_dict");
            Directory.CreateDirectory(tempDictPath);

            List<string> fileList = new List<string>() { "dict.txt", "prob_trans.json", "prob_emit.json", "char_state_tab.json", "cn_synonym.txt", 
                "idf.txt", "pos_prob_emit.json", "pos_prob_start.json", "pos_prob_trans.json", "stopwords.txt" };


            foreach (var file in fileList)
            {
                if (!File.Exists(Path.Combine(tempDictPath, file)))
                    File.WriteAllText(Path.Combine(tempDictPath, file), ReadEmbeddedResource(file, "Resources"));
            }

            JiebaNet.Segmenter.ConfigManager.ConfigFileBaseDir = tempDictPath;
        }

        byte[] ReadEmbeddedResourceBin(string fileName, string prefix)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.Equals($"NGramm.{prefix}.{fileName}"));

            if (resourceName == null)
                throw new FileNotFoundException($"Resource {fileName} not found.");

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    return binaryReader.ReadBytes((int)stream.Length);
                }
            }
        }
        string ReadEmbeddedResource(string fileName, string prefix)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.Equals($"NGramm.{prefix}.{fileName}"));

            if (resourceName == null)
                throw new FileNotFoundException($"Resource {fileName} not found.");

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private void tabControl1_Selected(object sender, TabControlCancelEventArgs e)
        {

        }

        private void Reporter_OperationNameChanged(object sender, string e)
        {
            RunOnUiContext(() =>
            {
                OperationNameLabel.Text = e;
            });
        }

        private void Reporter_TimerStopRequest(object sender, EventArgs e)
        {
            RunOnUiContext(() =>
            {
                stopwatch.Stop();
                TimeLabel.Text = stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.ff");
            });
        }

        private void Reporter_TimerStartRequest(object sender, EventArgs e)
        {
            RunOnUiContext(() =>
            {
                stopwatch.Reset();
                stopwatch.Start();
            });
        }

        private void Reporter_ProgressChanged(object sender, int e)
        {

            RunOnUiContext(() =>
            {
                if (e > progressBar1.Maximum || e < progressBar1.Minimum) return;
                progressBar1.Value = e;
            });
        }

        private void RunOnUiContext(Action ac) =>
            SyncContext(_ => ac(), null);
        
        // кнопка Статистика/для1n-грам/показати_n_грами
        private void button2_Click(object sender, EventArgs e)
        {
            var n = int.Parse(SingleStatN.Text) - 1;
            if (IndexPorah == 0)
            {
                ListForm listForm = new ListForm(nGramListSettings);
                listForm.Text = "N-грами " + SingleStatN.Text + " порядку";
                listForm.ShowContent(Helpers.SortByVal(processor.GetLiteralNgrams().ElementAt(n).GetNgrams(PerformanceSettings.MinNGrammCount)));
                listForm.Show();
            }
            else if (IndexPorah == 1)
            {
                ListForm listForm = new ListForm(nGramListSettings);
                listForm.Text = "N-грами " + SingleStatN.Text + " порядку";
                listForm.ShowContent(Helpers.SortByVal(processor.GetSymbolNgrams().ElementAt(n).GetNgrams(PerformanceSettings.MinNGrammCount)));
                listForm.Show();
            }
            else if (IndexPorah == 2)
            {
                ListForm listForm = new ListForm(nGramListSettings);
                listForm.Text = "N-грами " + SingleStatN.Text + " порядку";
                listForm.ShowContent(Helpers.SortByVal(processor.GetWordsNgrams().ElementAt(n).GetNgrams(PerformanceSettings.MinNGrammCount)));
                listForm.Show();
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (IndexPorah == 0)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string folder = saveFileDialog1.FileName;
                    System.IO.Directory.CreateDirectory(folder);
                    StreamWriter wr = new StreamWriter(folder + "//all_ngrams.txt");
                    wr.WriteLine("N\tAbsolute count\tCount");
                    var ngrams = processor.GetLiteralNgrams();
                    int i;
                    for (i = 0; i < ngrams.Count; i++)
                    {
                        wr.WriteLine((i + 1).ToString() + "\t" + ngrams.ElementAt(i).absCount.ToString() + "\t" + ngrams.ElementAt(i).count.ToString());
                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//" + SingleStatN.Text + "-grams.txt"); ;
                    int nh = int.Parse(SingleStatN.Text);
                    wr.WriteLine("Literal Ngram Statistic for " + nh.ToString() + "-grams");
                    wr.WriteLine();
                    NGrammContainer item = processor.GetLiteralNgrams().ElementAt(nh - 1);
                    wr.WriteLine();
                    wr.WriteLine("{0}\t{2}\t{1}", "RANK", "F", "NGramm");
                    //i = 1;
                    Dictionary<string, int> ngrms = Helpers.SortByVal(item.GetNgrams());
                    string ngr;
                    int rank = 0;
                    Dictionary<int, String> statistics = new Dictionary<int, String>();
                    if (CommonRankBox.Checked)
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            if (statistics.Keys.Contains(ngrms[ng]))
                            {
                                //statistics[ngrms[ng]] += ", " + ng;
                            }
                            else
                            {
                                statistics.Add(ngrms[ng], ng);
                            }
                        }
                        foreach (int ng in statistics.Keys)
                        {
                            ngr = statistics[ng];
                            rank++;
                            if (ngr.Contains("\n"))
                            {
                                ngr = ngr.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ng, statistics[ng].ToString());
                        }
                    }
                    else
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            ngr = ng;
                            rank++;
                            if (ng.Contains("\n"))
                            {
                                ngr = ng.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ngrms[ng].ToString(), ngr);
                        }

                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//zipf2_plot_data.txt");
                    wr.WriteLine("F\tPDF");
                    Statistics stats = new Statistics(item, CommonRankBox.Checked);
                    foreach (int repd in stats.GetZipf2Stats().Keys.Reverse())
                    {
                        wr.WriteLine(repd + "\t" + stats.GetZipf2Stats()[repd].ToString());

                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//pareto_plot_data.txt");
                    wr.WriteLine("Pareto plot data n=" + item.n.ToString());
                    wr.WriteLine();
                    wr.WriteLine("F\tCDF");
                    foreach (double rep in stats.GetParetroStats().Keys)
                    {
                        wr.WriteLine(rep + "\t" + stats.GetParetroStats()[rep].ToString());
                    }

                    wr.Close();
                }
            }
            else if (IndexPorah == 1)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string folder = saveFileDialog1.FileName;
                    System.IO.Directory.CreateDirectory(folder);
                    StreamWriter wr = new StreamWriter(folder + "//all_ngrams.txt");
                    wr.WriteLine("N\tAbsolute count\tCount");
                    var ngrams = processor.GetSymbolNgrams();
                    int i;
                    for (i = 0; i < ngrams.Count; i++)
                    {
                        wr.WriteLine((i + 1).ToString() + "\t" + ngrams.ElementAt(i).absCount.ToString() + "\t" + ngrams.ElementAt(i).count.ToString());
                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//" + SingleStatN.Text + "-grams.txt"); ;
                    int nh = int.Parse(SingleStatN.Text);
                    wr.WriteLine("Symbol Ngram Statistic for " + nh.ToString() + "-grams");
                    wr.WriteLine();
                    int k = 1;
                    NGrammContainer item = processor.GetSymbolNgrams().ElementAt(nh - 1);
                    wr.WriteLine();
                    wr.WriteLine("{0}\t{2}\t{1}", "RANK", "F", "NGramm");
                    //i = 1;
                    Dictionary<string, int> ngrms = Helpers.SortByVal(item.GetNgrams());
                    string ngr;
                    int rank = 0;
                    Dictionary<int, String> statistics = new Dictionary<int, String>();
                    if (CommonRankBox.Checked)
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            if (statistics.Keys.Contains(ngrms[ng]))
                            {
                                // statistics[ngrms[ng]] += ", " + ng;
                            }
                            else
                            {
                                statistics.Add(ngrms[ng], ng);
                            }
                        }
                        foreach (int ng in statistics.Keys)
                        {
                            ngr = statistics[ng];
                            rank++;
                            if (ngr.Contains("\n"))
                            {
                                ngr = ngr.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ng, statistics[ng].ToString());
                        }
                    }
                    else
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            ngr = ng;
                            rank++;
                            if (ng.Contains("\n"))
                            {
                                ngr = ng.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ngrms[ng].ToString(), ngr);
                        }

                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//zipf2_plot_data.txt");
                    wr.WriteLine("F\tPDF");
                    Statistics stats = new Statistics(item, CommonRankBox.Checked);
                    foreach (int repd in stats.GetZipf2Stats().Keys.Reverse())
                    {
                        wr.WriteLine(repd + "\t" + stats.GetZipf2Stats()[repd].ToString());
                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//pareto_plot_data.txt");
                    wr.WriteLine("Pareto plot data n=" + item.n.ToString());
                    wr.WriteLine();
                    wr.WriteLine("F\tCDF");
                    foreach (double rep in stats.GetParetroStats().Keys)
                    {
                        wr.WriteLine(rep + "\t" + stats.GetParetroStats()[rep].ToString());
                    }

                    wr.Close();
                }
            }
            else if (IndexPorah == 2)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string folder = saveFileDialog1.FileName;
                    Directory.CreateDirectory(folder);
                    StreamWriter wr = new StreamWriter(folder + "//all_ngrams.txt");
                    wr.WriteLine("N\tAbsolute count\tCount");
                    var ngrams = processor.GetWordsNgrams();
                    int i;
                    for (i = 0; i < ngrams.Count; i++)
                    {
                        wr.WriteLine((i + 1).ToString() + "\t" + ngrams.ElementAt(i).absCount.ToString() + "\t" +
                                     ngrams.ElementAt(i).count.ToString());
                    }

                    wr.Close();



                    wr = new StreamWriter(folder + "//" + SingleStatN.Text + "-grams.txt");
                    ;
                    int nh = int.Parse(SingleStatN.Text);
                    wr.WriteLine("Word Ngram Statistic for " + nh.ToString() + "-grams");
                    wr.WriteLine();
                    int k = 1;
                    NGrammContainer item = processor.GetWordsNgrams().ElementAt(nh - 1);
                    wr.WriteLine();
                    wr.WriteLine("{0}\t{2}\t{1}", "RANK", "F", "NGramm");
                    //i = 1;
                    Dictionary<string, int> ngrms = Helpers.SortByVal(item.GetNgrams());
                    Dictionary<int, String> statistics = new Dictionary<int, String>();
                    string ngr;
                    int rank = 0;

                    if (CommonRankBox.Checked)
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            if (statistics.Keys.Contains(ngrms[ng]))
                            {
                                //statistics[ngrms[ng]] += ", " + ng;
                            }
                            else
                            {
                                statistics.Add(ngrms[ng], ng);
                            }
                        }

                        foreach (int ng in statistics.Keys)
                        {
                            ngr = statistics[ng];
                            rank++;
                            if (ngr.Contains("\n"))
                            {
                                ngr = ngr.Replace("\n", "[new line]");
                            }

                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ng, statistics[ng].ToString());
                        }
                    }
                    else
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            ngr = ng;
                            rank++;
                            if (ng.Contains("\n"))
                            {
                                ngr = ng.Replace("\n", "[new line]");
                            }

                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ngrms[ng].ToString(), ngr);
                        }

                    }

                    wr.Close();

                    wr = new StreamWriter(folder + "//zipf2_plot_data.txt");
                    wr.WriteLine("F\tPDF");
                    Statistics stats = new Statistics(item, CommonRankBox.Checked);
                    foreach (int repd in stats.GetZipf2Stats().Keys.Reverse())
                    {
                        wr.WriteLine(repd + "\t" + stats.GetZipf2Stats()[repd].ToString());

                    }

                    wr.Close();

                    wr = new StreamWriter(folder + "//pareto_plot_data.txt");
                    wr.WriteLine("Pareto plot data n=" + item.n.ToString());
                    wr.WriteLine();
                    wr.WriteLine("F\tCDF");
                    foreach (double rep in stats.GetParetroStats().Keys)
                    {
                        wr.WriteLine(rep + "\t" + stats.GetParetroStats()[rep].ToString());
                    }

                    wr.Close();
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            int from = int.Parse(MultStatsStartN.Text) - 1;
            int to = int.Parse(MultStatsEndN.Text) - 1;

            if (IndexPorah == 0)
            {

                var lits = processor.GetLiteralNgrams();
                List<NGrammContainer> conts = new List<NGrammContainer>();
                for (int i = from; i <= to; i++)
                {
                    conts.Add(lits.ElementAt(i));
                }
                unified = new NGrammContainer(conts);

            }
            else if (IndexPorah == 1)
            {
                var lits = processor.GetSymbolNgrams();
                List<NGrammContainer> conts = new List<NGrammContainer>();
                for (int i = from; i <= to; i++)
                {
                    conts.Add(lits.ElementAt(i));
                }
                unified = new NGrammContainer(conts);
            }
            else if (IndexPorah == 2)
            {
                var lits = processor.GetWordsNgrams();
                List<NGrammContainer> conts = new List<NGrammContainer>();
                for (int i = from; i <= to; i++)
                {
                    conts.Add(lits.ElementAt(i));
                }
                unified = new NGrammContainer(conts);
            }
            
            SaveMultStatsButton.Enabled = true;
            ShowMultStatButton.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (IndexPorah == 0)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string folder = saveFileDialog1.FileName;
                    Directory.CreateDirectory(folder);

                    StreamWriter wr = new StreamWriter(folder + "//" + MultStatsStartN.Text + ".." + MultStatsEndN.Text + "-grams.txt"); ;
                    int from = int.Parse(MultStatsStartN.Text);
                    int to = int.Parse(MultStatsEndN.Text);
                    wr.WriteLine("Literal Ngram Statistic for " + from.ToString() + ".." + to.ToString() + "-grams");
                    wr.WriteLine();
                    int k = 1;
                    NGrammContainer item = unified;
                    wr.WriteLine();
                    wr.WriteLine("{0}\t{2}\t{1}", "RANK", "F", "NGramm");
                    string ngr;
                    int rank = 0;
                    Dictionary<string, int> ngrms = Helpers.SortByVal(item.GetNgrams());
                    Dictionary<int, String> statistics = new Dictionary<int, String>();
                    if (CommonRankBox.Checked)
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            if (statistics.Keys.Contains(ngrms[ng]))
                            {
                                //statistics[ngrms[ng]] += ", " + ng;
                            }
                            else
                            {
                                statistics.Add(ngrms[ng], ng);
                            }
                        }
                        foreach (int ng in statistics.Keys)
                        {
                            ngr = statistics[ng];
                            rank++;
                            if (ngr.Contains("\n"))
                            {
                                ngr = ngr.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ng, statistics[ng].ToString());
                        }
                    }
                    else
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            ngr = ng;
                            rank++;
                            if (ng.Contains("\n"))
                            {
                                ngr = ng.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ngrms[ng].ToString(), ngr);
                        }

                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//zipf2_plot_data.txt");
                    wr.WriteLine("F\tPDF");
                    Statistics stats = new Statistics(item, CommonRankBox.Checked);
                    foreach (int repd in stats.GetZipf2Stats().Keys.Reverse())
                    {
                        wr.WriteLine(repd + "\t" + stats.GetZipf2Stats()[repd].ToString());

                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//pareto_plot_data.txt");
                    wr.WriteLine("F\tCDF");
                    foreach (double rep in stats.GetParetroStats().Keys)
                    {
                        wr.WriteLine(rep + "\t" + stats.GetParetroStats()[rep].ToString());
                    }

                    wr.Close();
                }
            }
            else if (IndexPorah == 1)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string folder = saveFileDialog1.FileName;
                    System.IO.Directory.CreateDirectory(folder);

                    StreamWriter wr = new StreamWriter(folder + "//" + MultStatsStartN.Text + ".." + MultStatsEndN.Text + "-grams.txt"); ;
                    int from = int.Parse(MultStatsStartN.Text);
                    int to = int.Parse(MultStatsEndN.Text);
                    wr.WriteLine("Symbol Ngram Statistic for " + from.ToString() + ".." + to.ToString() + "-grams");
                    wr.WriteLine();
                    int k = 1;
                    NGrammContainer item = unified;
                    wr.WriteLine();
                    wr.WriteLine("{0}\t{2}\t{1}", "RANK", "F", "NGramm");
                    //int i = 1;
                    string ngr;
                    int rank = 0;
                    Dictionary<string, int> ngrms = Helpers.SortByVal(item.GetNgrams());
                    Dictionary<int, String> statistics = new Dictionary<int, String>();
                    if (CommonRankBox.Checked)
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            if (statistics.Keys.Contains(ngrms[ng]))
                            {
                                // statistics[ngrms[ng]] += ", " + ng;
                            }
                            else
                            {
                                statistics.Add(ngrms[ng], ng);
                            }
                        }
                        foreach (int ng in statistics.Keys)
                        {
                            ngr = statistics[ng];
                            rank++;
                            if (ngr.Contains("\n"))
                            {
                                ngr = ngr.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ng, statistics[ng].ToString());
                        }
                    }
                    else
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            ngr = ng;
                            rank++;
                            if (ng.Contains("\n"))
                            {
                                ngr = ng.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ngrms[ng].ToString(), ngr);
                        }

                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//zipf2_plot_data.txt");
                    wr.WriteLine("F\tCDF");
                    Statistics stats = new Statistics(item, CommonRankBox.Checked);
                    foreach (int rep in stats.GetZipf2Stats().Keys.Reverse())
                    {
                        wr.WriteLine(rep + "\t" + stats.GetZipf2Stats()[rep].ToString());
                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//pareto_plot_data.txt");
                    wr.WriteLine("F\tCDF");
                    foreach (double rep in stats.GetParetroStats().Keys)
                    {
                        wr.WriteLine(rep + "\t" + stats.GetParetroStats()[rep].ToString());
                    }

                    wr.Close();
                }
            }
            else if (IndexPorah == 2)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string folder = saveFileDialog1.FileName;
                    System.IO.Directory.CreateDirectory(folder);

                    StreamWriter wr = new StreamWriter(folder + "//" + MultStatsStartN.Text + ".." + MultStatsEndN.Text + "-grams.txt"); ;
                    int from = int.Parse(MultStatsStartN.Text);
                    int to = int.Parse(MultStatsEndN.Text);
                    wr.WriteLine("Word Ngram Statistic for " + from.ToString() + ".." + to.ToString() + "-grams");
                    wr.WriteLine();
                    int k = 1;
                    NGrammContainer item = unified;
                    wr.WriteLine();
                    wr.WriteLine("{0}\t{2}\t{1}", "RANK", "F", "NGramm");
                    int rank = 0;
                    string ngr;
                    Dictionary<string, int> ngrms = Helpers.SortByVal(item.GetNgrams());
                    Dictionary<int, String> statistics = new Dictionary<int, String>();
                    if (CommonRankBox.Checked)
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            if (statistics.Keys.Contains(ngrms[ng]))
                            {
                                //statistics[ngrms[ng]] += ", " + ng;
                            }
                            else
                            {
                                statistics.Add(ngrms[ng], ng);
                            }
                        }
                        foreach (int ng in statistics.Keys)
                        {
                            ngr = statistics[ng];
                            rank++;
                            if (ngr.Contains("\n"))
                            {
                                ngr = ngr.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ng, statistics[ng].ToString());
                        }
                    }
                    else
                    {
                        foreach (string ng in ngrms.Keys)
                        {
                            ngr = ng;
                            rank++;
                            if (ng.Contains("\n"))
                            {
                                ngr = ng.Replace("\n", "[new line]");
                            }
                            wr.WriteLine("{0}\t{2}\t{1}", rank.ToString(), ngrms[ng].ToString(), ngr);
                        }

                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//zipf2_plot_data.txt");
                    wr.WriteLine("F\tPDF");
                    Statistics stats = new Statistics(item, CommonRankBox.Checked);
                    foreach (int rep in stats.GetZipf2Stats().Keys.Reverse())
                    {
                        wr.WriteLine(rep + "\t" + stats.GetZipf2Stats()[rep].ToString());
                    }
                    wr.Close();

                    wr = new StreamWriter(folder + "//pareto_plot_data.txt");
                    wr.WriteLine("F\tCDF");
                    foreach (double rep in stats.GetParetroStats().Keys)
                    {
                        wr.WriteLine(rep + "\t" + stats.GetParetroStats()[rep].ToString());
                    }

                    wr.Close();
                }
            }
        }

        private void EndSignsChanged(object sender, EventArgs e)
        {
            NgrammProcessor.endsigns = new HashSet<char>(EndSignsList.Text.ToCharArray());
        }

        private void button23_Click(object sender, EventArgs e) //Gips
        {
            IndexPorah = tabControl1.SelectedIndex;
            new GipsForm(IndexPorah, getSize(), int.Parse(HeapsN.Text), processor).ShowDialog();
        }

        private async void порахуватиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IReadOnlyCollection<NGrammContainer> ngrams = new List<NGrammContainer>();
            NgrammProcessor.ignore_case = IgnoreRegisterChecbox.Checked;
            CodeNgrammProcessor.removeCodeComments = checkBoxComments.Checked;
            CodeNgrammProcessor.removeCodeStrings = checkBoxStrings.Checked;
            
            var sizeChangeTask = Task.Run(() =>
            {
                showSize();
            });

            var nPoryadok = int.Parse(N.Text);
            
            if (tabControl1.SelectedIndex == 0)
            {
                NgrammProcessor.process_spaces = LiteralCountSpaces.Checked;
                await processor.ProcessLiteralNGramms(nPoryadok);
                ngrams = processor.GetLiteralNgrams();

            }
            else if (tabControl1.SelectedIndex == 1)
            {
                NgrammProcessor.process_spaces = SymbolsCountSpaces.Checked;
                NgrammProcessor.consequtive_spaces = consequtiveSpaces.Checked;
                NgrammProcessor.show_NBS = ShowNPS.Checked;
                await processor.ProcessSymbolNGramms(nPoryadok);
                ngrams = processor.GetSymbolNgrams();
            }
            else if (tabControl1.SelectedIndex == 2)
            {
                NgrammProcessor.process_spaces = SymbolsCountSpaces.Checked;
                CodeNgrammProcessor.removeCodeComments = checkBoxComments.Checked;
                CodeNgrammProcessor.removeCodeStrings = checkBoxStrings.Checked;
                await processor.ProcessWordNGramms(nPoryadok);
                ngrams = processor.GetWordsNgrams();
            }

            listView1.Items.Clear();
            for (int i = 0; i < ngrams.Count; i++)
            {
                ListViewItem nli = new ListViewItem((i + 1).ToString());
                nli.SubItems.Add(ngrams.ElementAt(i).absCount.ToString());
                listView1.Items.Add(nli);
            }

            groupBox10.Enabled = true;
            groupBox2.Enabled = true;
            IndexPorah = tabControl1.SelectedIndex;
            MultStatsEndN.Text = N.Text;
            SaveMultStatsButton.Enabled = false;
            ShowMultStatButton.Enabled = false;
            await sizeChangeTask;
            MessageBox.Show("Завершено!");
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(MultStatsEndN.Text) > Convert.ToInt32(N.Text))
            {
                MultStatsEndN.Text = N.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var N = int.Parse(SingleStatN.Text) - 1;
            if (IndexPorah == 0)
            {
                Statistics stats = new Statistics(processor.GetLiteralNgrams().ElementAt(N), CommonRankBox.Checked);
                StatisticsWindow statWindow = new StatisticsWindow();
                statWindow.SetStats(stats, int.Parse(SingleStatN.Text) - 1);
                statWindow.ShowDialog();
            }
            else if (IndexPorah == 1)
            {
                Statistics stats = new Statistics(processor.GetSymbolNgrams().ElementAt(N), CommonRankBox.Checked);
                StatisticsWindow statWindow = new StatisticsWindow();
                statWindow.SetStats(stats, int.Parse(SingleStatN.Text) - 1);
                statWindow.ShowDialog();
            }
            else if (IndexPorah == 2)
            {
                Statistics stats = new Statistics(processor.GetWordsNgrams().ElementAt(N), CommonRankBox.Checked);
                StatisticsWindow statWindow = new StatisticsWindow();
                statWindow.SetStats(stats, int.Parse(SingleStatN.Text) - 1);
                statWindow.ShowDialog();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(SingleStatN.Text) > Convert.ToInt32(N.Text))
            {
                SingleStatN.Text = N.Text;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(HeapsN.Text) > Convert.ToInt32(N.Text))
            {
                HeapsN.Text = N.Text;
            }
        }

        int getSize()
        {
            if (processor != null)
            {
                int size = 0;
                switch (tabControl1.SelectedIndex)
                {
                    case 0:
                        {
                            size = processor.GetLiteralCount(LiteralCountSpaces.Checked);
                            break;
                        }
                    case 1:
                        {
                            size = processor.GetSymbolsCount(SymbolsCountSpaces.Checked);
                            break;
                        }
                    case 2:
                        {
                            size = processor.GetWordsCount();
                            break;
                        }
                }

                return size;
            }
            return -1;
        }

        void showSize()
        {
            if (processor != null)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    int size = getSize();
                    еуіеToolStripMenuItem.Text = $"L={size}";
                }));

            }
        }

        private void SpecialSymbolsCount_CheckedChanged(object sender, EventArgs e)
        {
            NgrammProcessor.ignore_punctuation = !SpecialSymbolsCount.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            PerformanceSettings.Cores = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            PerformanceSettings.MinNGrammCount = (int)numericUpDown2.Value;
        }

        private void ShowMultStatButton_Click(object sender, EventArgs e)
        {
            var start = int.Parse(MultStatsStartN.Text);
            var end = int.Parse(MultStatsEndN.Text);

            ListForm listForm = new ListForm(nGramListSettings)
            {
                Text = $"N-грами {start} - {end} порядків"
            };
            listForm.ShowContent(Helpers.SortByVal(unified.GetNgrams(PerformanceSettings.MinNGrammCount)));
            listForm.Show();
        }

        private void ShowNPS_CheckedChanged(object sender, EventArgs e)
        {
            nGramListSettings.ShowNPS = ShowNPS.Checked;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            nGramListSettings.SelectedTab = (Tabs)tabControl1.SelectedIndex;

            EditRegisterCheckbox();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }


        private async void природнийТекстToolStripMenuItem_Click(object sender, EventArgs e)
        {
            codeWordsPanel.Hide();
            codeWordsPanel.Enabled = false;

            wordsPanel.Show();
            wordsPanel.Enabled = true;

            await OpenFile(isProgramText: false);
        }

        private async void програмнийКодToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wordsPanel.Hide();
            wordsPanel.Enabled = false;

            codeWordsPanel.Show();
            codeWordsPanel.Enabled = true;

            await OpenFile(isProgramText: true);
            EditRemoveCommentsCheckbox();
        }
    }
}
