using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using static System.Windows.Forms.ListViewItem;
using NGramm.Models;
using System.Globalization;

namespace NGramm
{
    public partial class ListForm : Form
    {
        private NGramListSettings _settings;
        public ListForm(NGramListSettings settings)
        {
            InitializeComponent();

            listView1.ListViewItemSorter = new ListViewItemComparer(0, SortOrder.Ascending);
            listView1.Columns[0].Text = listView1.Columns[0].Text + " (↓)";
            _settings = settings;
        }
        public void ShowContent(Dictionary<string, int> ngrams)
        {
            int k = 1;
            var number = new List<string>();
            var rang = new List<string>();

            var canShowNPS = CanShowNPS(ngrams);

            if (canShowNPS)
            {
                listView1.Columns.Add("Юнікод");
                listView1.Columns.Add("Категорія символу");
            }

            foreach (string item in ngrams.Keys)
            {
                ListViewItem nli = new ListViewItem(k.ToString());
                number.Add(item);
                nli.SubItems.Add(item);
                nli.SubItems.Add(ngrams[item].ToString());

                if (canShowNPS)
                {
                    nli.SubItems.Add($"\\u{(int)item[0]:X4}");
                    nli.SubItems.Add(CharUnicodeInfo.GetUnicodeCategory(item[0]).ToString());
                }

                listView1.Items.Add(nli);
                k++;
            }

            /* foreach (string item in ngrams.Keys)
             {
                 number.Add(item);
             }


             for (int i=1; i <= number.Count; i++)
             {
                 ListViewItem nli = new ListViewItem(k.ToString());

                 foreach (string item in ngrams.Keys)
                 {
                     if (i == 1)
                     {
                         nli.SubItems.Add(item);
                         nli.SubItems.Add(ngrams[item].ToString());
                         listView1.Items.Add(nli);
                     } 
                     if(number[i] == number[i - 1])
                     {
                         nli.SubItems.Add(item);
                         nli.SubItems.Add(ngrams[item].ToString());
                         listView1.Items.Add("");
                     }
                     else
                     {
                         nli.SubItems.Add(item);  
                         nli.SubItems.Add(ngrams[item].ToString());
                         listView1.Items.Add(nli); 
                     }

                     k++;
                 }*/

        }

        private void ListForm_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var listView = (ListView)sender;
            ListViewItemComparer listViewItemSorter = (ListViewItemComparer)listView.ListViewItemSorter;

            foreach (ColumnHeader col in listView.Columns)
            {
                col.Text = col.Text.Replace(" (↑)", "");
                col.Text = col.Text.Replace(" (↓)", "");
            }

            if (listViewItemSorter != null)
            {
                listViewItemSorter.ColumnId = e.Column;

                if (listViewItemSorter.Order == SortOrder.Ascending)
                {
                    listViewItemSorter.Order = SortOrder.Descending;
                    listView.Columns[e.Column].Text = listView.Columns[e.Column].Text + " (↑)";

                }
                else
                {
                    listViewItemSorter.Order = SortOrder.Ascending;
                    listView.Columns[e.Column].Text = listView.Columns[e.Column].Text + " (↓)";
                }
            }
            else
            {
                listViewItemSorter = new ListViewItemComparer(e.Column, SortOrder.Ascending);

                listView.Columns[e.Column].Text = listView.Columns[e.Column].Text + " (↓)";
                listView.ListViewItemSorter = listViewItemSorter;
            }

            listView.BeginUpdate();
            listView.Sort();
            listView.EndUpdate();
        }


        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                List<string> list = new List<string>();
                foreach(ListViewItem item in listView1.SelectedItems)
                {
                    List<string> subItems = new List<string>();
                    foreach(ListViewSubItem subItem in item.SubItems)
                    {
                        subItems.Add(subItem.Text);
                    }
                    list.Add(string.Join("\t", subItems));
                }

                Clipboard.SetText(String.Join("\n", list));
            }

        }

        private bool CanShowNPS(Dictionary<string, int> ngrams) 
            => _settings.SelectedTab == Tabs.Symbols && _settings.ShowNPS && !ngrams.Keys.Any(x => x.Length > 1);
    }
}
