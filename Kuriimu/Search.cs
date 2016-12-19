using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Kuriimu.Properties;
using KuriimuContract;

namespace Kuriimu
{
	public partial class frmSearch : Form
	{
		public List<IEntry> Entries { get; set; }
		public IEntry Selected { get; set; }

		public frmSearch()
		{
			InitializeComponent();
			Icon = Resources.find;
		}

		private void frmSearch_Load(object sender, EventArgs e)
		{
			Icon = Resources.find;

			Selected = null;
			txtFindText.Text = Settings.Default.FindWhat;
			chkMatchCase.Checked = Settings.Default.FindMatchCase;

			Find();

			if (lstResults.Items.Count > Settings.Default.FindSelectedIndex)
				lstResults.SelectedIndex = Settings.Default.FindSelectedIndex;
		}

		private void btnFindText_Click(object sender, EventArgs e)
		{
			Find();

			if (lstResults.Items.Count == 0)
				MessageBox.Show("Could not find \"" + txtFindText.Text + "\".", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void Find()
		{
			List<IEntry> matches = new List<IEntry>();
			lstResults.Items.Clear();

			if (txtFindText.Text.Trim() != string.Empty && Entries != null)
			{
				foreach (IEntry entry in Entries)
				{
					if (chkMatchCase.Checked)
					{
						if (entry.EditedTextString.Contains(txtFindText.Text) || entry.OriginalTextString.Contains(txtFindText.Text))
							matches.Add(entry);
					}
					else
					{
						if (entry.EditedTextString.ToLower().Contains(txtFindText.Text) || entry.OriginalTextString.ToLower().Contains(txtFindText.Text))
							matches.Add(entry);
					}
				}
			}

			foreach (IEntry entry in matches)
				lstResults.Items.Add(new ListItem(entry.ToString(), entry));

			tslResultCount.Text = matches.Count > 0 ? "Found " + matches.Count + " matches" : string.Empty;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void txtFindText_TextChanged(object sender, EventArgs e)
		{
			Settings.Default.FindWhat = txtFindText.Text;
			Settings.Default.Save();
		}

		private void chkMatchCase_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.FindMatchCase = chkMatchCase.Checked;
			Settings.Default.Save();
		}

		private void lstResults_DoubleClick(object sender, EventArgs e)
		{
			if (lstResults.Items.Count > 0 && lstResults.SelectedIndex >= 0)
			{
				Selected = (IEntry)((ListItem)lstResults.SelectedItem).Value;
				Close();
			}
		}

		private void lstResults_SelectedIndexChanged(object sender, EventArgs e)
		{
			Settings.Default.FindSelectedIndex = lstResults.SelectedIndex;
			Settings.Default.Save();
		}
	}
}