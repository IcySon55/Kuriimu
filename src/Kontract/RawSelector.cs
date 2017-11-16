using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kontract.Interface;

namespace Kontract
{
    public partial class RawSelector : Form
    {
        public RawSelector(string[] raws)
        {
            InitializeComponent();

            raws.Select(r => rawList.Items.Add(r));
        }

        public static Lazy<IArchiveManager, IFilePluginMetadata> Show(List<Lazy<IArchiveManager, IFilePluginMetadata>> raws)
        {
            var box = new RawSelector(raws.Select(r => (r.Metadata.Name + "; " + r.Metadata.Description)).ToArray());
            box.ShowDialog();
            return raws[box.rawList.SelectedIndex];
        }
    }
}
