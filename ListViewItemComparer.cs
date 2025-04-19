using System.Collections;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

namespace NGramm
{
    public class ListViewItemComparer : IComparer
    {
        public int ColumnId;
        public SortOrder Order;

        public ListViewItemComparer(int column, SortOrder order)
        {
            ColumnId = column;
            Order = order;
        }

        public int Compare(object x, object y)
        {
            if (!(x is ListViewItem))
                return (0);
            if (!(y is ListViewItem))
                return (0);

            ListViewItem l1 = (ListViewItem)x;
            ListViewItem l2 = (ListViewItem)y;

            if (l1.ListView.Columns[ColumnId].Tag == null)
            {
                l1.ListView.Columns[ColumnId].Tag = "Text";
            }

            if (l1.ListView.Columns[ColumnId].Tag.ToString() == "Numeric")
            {
                if (!double.TryParse(l1.SubItems[ColumnId].Text, out double fl1))
                {
                    fl1 = 0;
                }
                if (!double.TryParse(l2.SubItems[ColumnId].Text, out double fl2))
                {
                    fl2 = 0;
                }

                if (Order == SortOrder.Ascending)
                {
                    return fl1.CompareTo(fl2);
                }
                else
                {
                    return fl2.CompareTo(fl1);
                }
            }
            else
            {
                string str1 = l1.SubItems[ColumnId].Text;
                string str2 = l2.SubItems[ColumnId].Text;

                if (Order == SortOrder.Ascending)
                {
                    return str1.CompareTo(str2);
                }
                else
                {
                    return str2.CompareTo(str1);
                }
            }
        }

    }
}
