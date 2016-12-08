using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KuriimuContract;

namespace Kuriimu
{
	public partial class Main : Form
	{
		public Main(string[] args)
		{
			InitializeComponent();

			this.ClientSize = new Size(32 + (104 * 3) + (6 * 2) + 32, 32 + 104 + 32);
			Console.Write(Common.GetAppMessage());
		}

		private void Main_Load(object sender, EventArgs e)
		{
			this.Icon = Properties.Resources.kuriimu;
		}

		private void btnEditor_Click(object sender, EventArgs e)
		{
			Editor editor = new Editor();
			editor.Show();
		}

		private void btnDumping_Click(object sender, EventArgs e)
		{
			ExtensionSelect dSelect = new ExtensionSelect();
			dSelect.StartPosition = FormStartPosition.CenterParent;

			if (dSelect.ShowDialog() == DialogResult.OK)
				dSelect.Extension.Show();
		}

		private void btnDirector_Click(object sender, EventArgs e)
		{
			Director director = new Director();
			director.Show();
		}
	}
}