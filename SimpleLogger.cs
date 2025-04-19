using System;
using System.IO;
using System.Text;
using System.Windows.Forms;


namespace NGramm
{
    public class SimpleLogger
    {
        private readonly TextBox _outputBox;
        private readonly StringBuilder _logBuffer;
        private string _fileName = "debug_log.txt";

        public SimpleLogger(TextBox outputBox, string fileName)
        {
            _outputBox = outputBox;
            _logBuffer = new StringBuilder();
            _outputBox.ReadOnly = true;
            _outputBox.ScrollBars = ScrollBars.Vertical;
            _outputBox.Multiline = true;
            _fileName = fileName;
        }

        public void Print(string message)
        {
            string timestamped = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            _logBuffer.AppendLine(timestamped);

            if (_outputBox.InvokeRequired)
            {
                _outputBox.Invoke(new Action(() =>
                {
                    _outputBox.AppendText(timestamped + Environment.NewLine);
                }));
            }
            else
            {
                _outputBox.AppendText(timestamped + Environment.NewLine);
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(_fileName, _logBuffer.ToString());
                MessageBox.Show("Лог збережено у файл:\n" + Path.GetFullPath(_fileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження логу: " + ex.Message);
            }
        }

        public void Clear()
        {
            _logBuffer.Clear();
            _outputBox.Clear();
        }
    }

}