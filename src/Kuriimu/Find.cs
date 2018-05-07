using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Kontract.Interface;
using Kontract;
using Kuriimu.Properties;

namespace Kuriimu
{
    public partial class Find : Form
    {
        public IEnumerable<TextEntry> Entries { get; set; }
        public TextEntry Selected { get; private set; }
        public TextEntry Current { get; set; }
        public bool Replace { get; set; }
        public bool Replaced { get; private set; }

        public Find()
        {
            InitializeComponent();
        }

        private void frmFind_Load(object sender, EventArgs e)
        {
            Icon = !Replace ? Resources.find : Resources.replace;
            Text = !Replace ? "Find" : "Replace";
            tabFindReplace.SelectedIndex = !Replace ? 0 : 1;
            AcceptButton = !Replace ? btnFindText : btnReplaceText;
            CancelButton = !Replace ? btnCancel : btnCancelReplace;

            // Find
            txtFindText.Text = Settings.Default.FindWhat;
            chkMatchCase.Checked = Settings.Default.FindMatchCase;

            // Replace
            txtFindTextReplace.Text = Settings.Default.FindWhat;
            txtReplaceText.Text = Settings.Default.ReplaceWith;
            btnReplaceText.Text = Settings.Default.ReplaceAll ? "Replace All" : "Replace";
            chkMatchCaseReplace.Checked = Settings.Default.FindMatchCase;
            chkReplaceAll.Checked = Settings.Default.ReplaceAll;

            if (!Replace)
                DoFind();

            if (lstResults.Items.Count > Settings.Default.FindSelectedIndex)
                lstResults.SelectedIndex = Settings.Default.FindSelectedIndex;
        }

        private void tabFindReplace_SelectedIndexChanged(object sender, EventArgs e)
        {
            Replace = tabFindReplace.SelectedIndex != 0;
            frmFind_Load(this, EventArgs.Empty);
            if (!Replace)
            {
                txtFindText.Focus();
                txtFindText.SelectAll();
            }
            else
            {
                txtFindTextReplace.Focus();
                txtFindTextReplace.SelectAll();
            }
        }

        private void btnFindText_Click(object sender, EventArgs e)
        {
            if (txtFindText.Focused)
                DoFind();
            else
                lstResults_DoubleClick(lstResults, EventArgs.Empty);

            if (lstResults.Items.Count == 0)
                MessageBox.Show("Could not find \"" + txtFindText.Text + "\".", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoFind()
        {
            lstResults.BeginUpdate();

            lstResults.Items.Clear();
            if (txtFindText.Text.Trim() != string.Empty && Entries != null)
            {
                foreach (var entry in Entries)
                {
                    if (chkMatchCase.Checked)
                    {
                        if (entry.EditedText.Contains(txtFindText.Text) || entry.OriginalText.Contains(txtFindText.Text) || entry.Name.Contains(txtFindText.Text))
                            lstResults.Items.Add(new ListItem(entry.ToString(), entry));
                    }
                    else
                    {
                        if (entry.EditedText.ToLower().Contains(txtFindText.Text.ToLower()) || entry.OriginalText.ToLower().Contains(txtFindText.Text.ToLower()) || entry.Name.ToLower().Contains(txtFindText.Text.ToLower()))
                            lstResults.Items.Add(new ListItem(entry.ToString(), entry));
                    }

                    foreach (var subEntry in entry.SubEntries)
                    {
                        if (chkMatchCase.Checked)
                        {
                            if (subEntry.EditedText.Contains(txtFindText.Text) || subEntry.OriginalText.Contains(txtFindText.Text) || subEntry.Name.Contains(txtFindText.Text))
                                lstResults.Items.Add(new ListItem(entry + "/" + subEntry, subEntry));
                        }
                        else
                        {
                            if (subEntry.EditedText.ToLower().Contains(txtFindText.Text.ToLower()) || subEntry.OriginalText.ToLower().Contains(txtFindText.Text.ToLower()) || subEntry.Name.ToLower().Contains(txtFindText.Text.ToLower()))
                                lstResults.Items.Add(new ListItem(entry + "/" + subEntry, subEntry));
                        }
                    }
                }
            }
            lstResults.EndUpdate();

            tslResultCount.Text = lstResults.Items.Count > 0 ? "Found " + lstResults.Items.Count + " matches." : string.Empty;
        }

        private void btnReplaceText_Click(object sender, EventArgs e)
        {
            DoReplace();

            if (lstResultsReplace.Items.Count == 0)
                MessageBox.Show("Could not find \"" + txtFindTextReplace.Text + "\".", "Replace", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DoReplace()
        {
            lstResultsReplace.BeginUpdate();

            lstResultsReplace.Items.Clear();
            if (txtFindTextReplace.Text.Trim() != string.Empty)
            {
                if (Settings.Default.ReplaceAll && Entries != null)
                {
                    foreach (var entry in Entries)
                    {
                        if (chkMatchCaseReplace.Checked)
                        {
                            if (entry.EditedText.Contains(txtFindTextReplace.Text))
                            {
                                entry.EditedText = Regex.Replace(entry.EditedText, txtFindTextReplace.Text, txtReplaceText.Text);
                                lstResultsReplace.Items.Add(new ListItem(entry.ToString(), entry));
                            }
                        }
                        else
                        {
                            if (entry.EditedText.ToLower().Contains(txtFindText.Text.ToLower()))
                            {
                                entry.EditedText = Regex.Replace(entry.EditedText, txtFindTextReplace.Text, txtReplaceText.Text, RegexOptions.IgnoreCase);
                                lstResultsReplace.Items.Add(new ListItem(entry.ToString(), entry));
                            }
                        }

                        foreach (var subEntry in entry.SubEntries)
                        {
                            if (chkMatchCaseReplace.Checked)
                            {
                                if (subEntry.EditedText.Contains(txtFindTextReplace.Text))
                                {
                                    subEntry.EditedText = Regex.Replace(subEntry.EditedText, txtFindTextReplace.Text, txtReplaceText.Text);
                                    lstResultsReplace.Items.Add(new ListItem(entry + "/" + subEntry, subEntry));
                                }
                            }
                            else
                            {
                                if (subEntry.EditedText.ToLower().Contains(txtFindText.Text.ToLower()))
                                {
                                    subEntry.EditedText = Regex.Replace(subEntry.EditedText, txtFindTextReplace.Text, txtReplaceText.Text, RegexOptions.IgnoreCase);
                                    lstResultsReplace.Items.Add(new ListItem(entry + "/" + subEntry, subEntry));
                                }
                            }
                        }
                    }
                }
                else if (!Settings.Default.ReplaceAll && Current != null)
                {
                    if (chkMatchCaseReplace.Checked)
                    {
                        if (Current.EditedText.Contains(txtFindTextReplace.Text))
                        {
                            Current.EditedText = Regex.Replace(Current.EditedText, txtFindTextReplace.Text, txtReplaceText.Text);
                            lstResultsReplace.Items.Add(new ListItem(Current.ToString(), Current));
                        }
                    }
                    else
                    {
                        if (Current.EditedText.ToLower().Contains(txtFindText.Text.ToLower()))
                        {
                            Current.EditedText = Regex.Replace(Current.EditedText, txtFindTextReplace.Text, txtReplaceText.Text, RegexOptions.IgnoreCase);
                            lstResultsReplace.Items.Add(new ListItem(Current.ToString(), Current));
                        }
                    }
                }
            }
            lstResultsReplace.EndUpdate();

            tslResultCount.Text = lstResultsReplace.Items.Count > 0 ? "Replaced strings in " + lstResultsReplace.Items.Count + (lstResultsReplace.Items.Count == 1 ? " entry." : " entries.") : string.Empty;

            if (!Replaced)
                Replaced = lstResultsReplace.Items.Count > 0;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtFindText_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.FindWhat = ((TextBox)sender).Text;
            Settings.Default.Save();
        }

        private void txtReplaceText_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.ReplaceWith = ((TextBox)sender).Text;
            Settings.Default.Save();
        }

        private void chkMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.FindMatchCase = ((CheckBox)sender).Checked;
            Settings.Default.Save();
        }

        private void chkReplaceAll_CheckedChanged(object sender, EventArgs e)
        {
            btnReplaceText.Text = chkReplaceAll.Checked ? "Replace All" : "Replace";
            Settings.Default.ReplaceAll = chkReplaceAll.Checked;
            Settings.Default.Save();
        }

        private void lstResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstResults.Items.Count <= 0 || lstResults.SelectedIndex < 0) return;
            Selected = (TextEntry)((ListItem)lstResults.SelectedItem).Value;
            DialogResult = DialogResult.OK;
        }

        private void lstResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.FindSelectedIndex = lstResults.SelectedIndex;
            Settings.Default.Save();
        }

        private void lstResultsReplace_DoubleClick(object sender, EventArgs e)
        {
            if (lstResultsReplace.Items.Count <= 0 || lstResultsReplace.SelectedIndex < 0) return;
            Selected = (TextEntry)((ListItem)lstResultsReplace.SelectedItem).Value;
            DialogResult = DialogResult.OK;
        }
    }
}