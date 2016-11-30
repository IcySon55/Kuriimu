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

			Console.Write(Common.GetAppMessage());
		}

		private void Main_Load(object sender, EventArgs e)
		{
			this.Icon = Properties.Resources.kuriimu;
		}

		private void btnEditor_Click(object sender, EventArgs e)
		{
			Editor editor = new Editor(this);
			this.Hide();
			editor.Show();
		}

		private void btnDumping_Click(object sender, EventArgs e)
		{
			DumperSelect dSelect = new DumperSelect();
			dSelect.StartPosition = FormStartPosition.CenterParent;

			if (dSelect.ShowDialog() == DialogResult.OK)
			{
				dSelect.Dumper.ShowDialog();
			}
		}

		private void btnDirector_Click(object sender, EventArgs e)
		{
			Director director = new Director();
			director.ShowDialog();
		}
	}
}