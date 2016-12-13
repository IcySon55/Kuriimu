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
	public partial class frmExtensionSelect : Form
	{
		private Dictionary<string, IExtension> extensions = null;

		#region Properties

		public IExtension Extension { get; set; }

		#endregion

		public frmExtensionSelect()
		{
			InitializeComponent();
			this.Extension = null;
		}

		private void ExtensionSelect_Load(object sender, EventArgs e)
		{
			this.Text = Settings.Default.ApplicationName;
			this.Icon = Resources.kuriimu;

			// Load Dumpers
			Console.WriteLine("Loading extensions...");

			extensions = new Dictionary<string, IExtension>();
			foreach (IExtension extension in PluginLoader<IExtension>.LoadPlugins(Settings.Default.PluginDirectory, "ext*.dll"))
				extensions.Add(extension.Name, extension);

			// Populate List
			int index = 0;
			imgList.TransparentColor = Color.Transparent;
			foreach (string key in extensions.Keys)
			{
				IExtension extension = extensions[key];
				imgList.Images.Add(extension.Icon);
				imgList.Images.SetKeyName(index, extension.Name);
				TreeNode node = new TreeNode(extensions[key].Name, index, index);
				node.Tag = extension;
				treExtensions.Nodes.Add(node);
				index++;
			}

			if (treExtensions.Nodes.Count > 0)
				treExtensions.SelectedNode = treExtensions.Nodes[0];
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			Extension = (IExtension)treExtensions.SelectedNode.Tag;
			DialogResult = DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}