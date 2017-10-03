using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Kuriimu.Kontract;
using Kuriimu.Kontract.Interface;
using Kohii.Properties;
using System.Linq;
using System.IO;

namespace Kohii
{
    public partial class Kohii : Form
    {
        private List<ISoundAdapter> _soundAdapters;

        public Kohii(string[] args)
        {
            InitializeComponent();

            // Load Plugins
            _soundAdapters = PluginLoader<ISoundAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "sound*.dll").ToList();

            // Load passed in file
            if (args.Length > 0 && File.Exists(args[0]))
                OpenFile(args[0]);
        }

        private void OpenFile(string filename = "")
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
