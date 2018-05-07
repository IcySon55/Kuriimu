using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kontract.Interface;
using Kontract;
using runext.Properties;

namespace runext
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0 && File.Exists(Path.Combine(Settings.Default.PluginDirectory, args[0])))
            {
                List<IExtension> extension = PluginLoader<IExtension>.LoadPlugins(Settings.Default.PluginDirectory, args[0]).ToList();
                Application.Run(extension[0].CreateInstance(args.Skip(1).ToArray()));
            }
            else
            {
                ExtensionSelect extensionSelect = new ExtensionSelect();

                if (extensionSelect.ShowDialog() == DialogResult.OK)
                    Application.Run(extensionSelect.Extension.CreateInstance());
            }
        }
    }
}