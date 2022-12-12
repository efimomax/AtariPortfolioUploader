using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PortfolioUploader
{
    public static class Extensions
    {
        public static bool IsFile(this ListViewItem listViewItem)
        {
            return listViewItem.SubItems.Count == 2 && listViewItem.ImageKey.ToLowerInvariant() == "file.png";
        }
    }
}
