using System;
using System.Drawing;
using System.Windows.Forms;

namespace Kuriimu
{
	public partial class frmMain : Form
	{
		public frmMain(string[] args)
		{
			InitializeComponent();

			ClientSize = new Size(32 + (104 * 3) + (6 * 2) + 32, 32 + 104 + 32);
			Console.Write(Common.GetAppMessage());
		}

		private void Main_Load(object sender, EventArgs e)
		{
			Icon = Properties.Resources.kuriimu;
		}

		private void btnEditor_Click(object sender, EventArgs e)
		{
			frmEditor editor = new frmEditor();
			editor.Show();
		}

		private void btnDumping_Click(object sender, EventArgs e)
		{
			frmExtensionSelect dSelect = new frmExtensionSelect();

			if (dSelect.ShowDialog() == DialogResult.OK)
				dSelect.Extension.Show();
		}

		private void btnDirector_Click(object sender, EventArgs e)
		{
			frmDirector director = new frmDirector();
			director.Show();
		}
	}
}