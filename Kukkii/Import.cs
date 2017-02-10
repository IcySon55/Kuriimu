using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kukkii.Properties;

namespace Kukkii
{
	public partial class frmImport : Form
	{
		private string _filename = string.Empty;

		public frmImport(string filename)
		{
			InitializeComponent();

			_filename = filename;
		}

		private void Import_Load(object sender, EventArgs e)
		{
			Icon = Resources.kukkii;

			if (File.Exists(_filename))
			{
				var bmp = Image.FromFile(_filename);
				imbPreview.Image = bmp;
			}
		}

		private void btnImport_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
		}
	}
}