using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Kuriimu.Contract;
using Kuriimu.Properties;

namespace Kuriimu
{
    public partial class frmSearch : Form
    {
        public IEnumerable<IEntry> Entries { get; set; }
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

            lstResults.BeginUpdate();

            lstResults.Items.Clear();

            if (txtFindText.Text.Trim() != string.Empty && Entries != null)
            {
                foreach (IEntry entry in Entries)
                {
                    if (chkMatchCase.Checked)
                    {
                        if (entry.EditedText.Contains(txtFindText.Text) || entry.OriginalText.Contains(txtFindText.Text))
                            lstResults.Items.Add(new ListItem(entry.ToString(), entry));
                    }
                    else
                    {
                        if (entry.EditedText.ToLower().Contains(txtFindText.Text.ToLower()) || entry.OriginalText.ToLower().Contains(txtFindText.Text.ToLower()))
                            lstResults.Items.Add(new ListItem(entry.ToString(), entry));
                    }

                    foreach (IEntry subEntry in entry.SubEntries)
                    {
                        if (chkMatchCase.Checked)
                        {
                            if (subEntry.EditedText.Contains(txtFindText.Text) || subEntry.OriginalText.Contains(txtFindText.Text))
                                lstResults.Items.Add(new ListItem(entry.ToString() + "/" + subEntry.ToString(), subEntry));
                        }
                        else
                        {
                            if (subEntry.EditedText.ToLower().Contains(txtFindText.Text.ToLower()) || subEntry.OriginalText.ToLower().Contains(txtFindText.Text.ToLower()))
                                lstResults.Items.Add(new ListItem(entry.ToString() + "/" + subEntry.ToString(), subEntry));
                        }
                    }
                }
            }

            lstResults.EndUpdate();

            tslResultCount.Text = matches.Count > 0 ? "Found " + matches.Count + " matches" : string.Empty;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
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
                DialogResult = DialogResult.OK;
            }
        }

        private void lstResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.FindSelectedIndex = lstResults.SelectedIndex;
            Settings.Default.Save();
        }
    }
}