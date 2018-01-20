using System;
using System.Windows.Forms;

namespace DBClassGen.Classes {
    public class ListViewSorter : System.Collections.IComparer {
        public int Compare(object x, object y) {
            int result = 0;

            if (!(x is ListViewItem)) {
                return result;
            }
            if (!(y is ListViewItem)) {
                return result;
            }

            if (((ListViewItem)x).SubItems.Count < ByColumn +1)
                return 0;
            if (((ListViewItem)y).SubItems.Count < ByColumn +1)
                return 0;

            // Determine whether the type being compared is a date type.
            try {
                DateTime firstDate = DateTime.Parse(((ListViewItem)x).SubItems[ByColumn].Text);
                DateTime secondDate = DateTime.Parse(((ListViewItem)y).SubItems[ByColumn].Text);
                result = DateTime.Compare(firstDate, secondDate);
            }
            catch {

                // is it an iteger?
                try{
                    int first=int.Parse(((ListViewItem) x).SubItems[ByColumn].Text);
                    int second=int.Parse(((ListViewItem) y).SubItems[ByColumn].Text);
                    if (first < second)
                        result = 0;
                    else
                        result=-1;
                }
                catch(Exception){
                    // Compare the two items as a string. 
                    result = String.Compare(((ListViewItem)x).SubItems[ByColumn].Text, ((ListViewItem)y).SubItems[ByColumn].Text);
                    
                }

               
            }

            // Determine whether the sort order is descending. 
            if (((ListViewItem)x).ListView.Sorting == SortOrder.Descending) {
                // Invert the value returned by compare. 
                result *= -1;
            }

            LastSort = ByColumn;
            return result;
        }

        public int ByColumn {
            get { return Column; }
            set { Column = value; }
        }
        int Column = 0;

        public int LastSort {
            get { return LastColumn; }
            set { LastColumn = value; }
        }
        int LastColumn = 0;
    }

}
