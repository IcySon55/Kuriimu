using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KuriimuContract;
using Kuriimu.Properties;

namespace Kuriimu
{
	public partial class DumperSelect : Form
	{
		Dictionary<string, IDumper> dumpers = null;

		#region Properties

		public IDumper Dumper { get; set; }

		#endregion

		public DumperSelect()
		{
			InitializeComponent();
			this.Dumper = null;
		}

		private void DumperSelect_Load(object sender, EventArgs e)
		{
			this.Text = Settings.Default.ApplicationName;
			this.Icon = Resources.kuriimu;

			// Load Plugins
			Console.WriteLine("Loading dumpers...");

			dumpers = new Dictionary<string, IDumper>();
			foreach (IDumper dumper in PluginLoader<IDumper>.LoadPlugins(Settings.Default.PluginDirectory))
				dumpers.Add(dumper.Name, dumper);

			// Populate List
			int index = 0;
			imgList.TransparentColor = Color.Transparent;
			foreach (string key in dumpers.Keys)
			{
				IDumper dumper = dumpers[key];
				imgList.Images.Add(dumper.Icon);
				imgList.Images.SetKeyName(index, dumper.Name);
				TreeNode node = new TreeNode(dumpers[key].Name, index, index);
				node.Tag = dumper;
				treDumpers.Nodes.Add(node);
				index++;
			}

			if (treDumpers.Nodes.Count > 0)
				treDumpers.SelectedNode = treDumpers.Nodes[0];
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			Dumper = (IDumper)treDumpers.SelectedNode.Tag;
			DialogResult = DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}