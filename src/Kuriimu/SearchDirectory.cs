using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Kontract;
using Kuriimu.Properties;

namespace Kuriimu
{
    public partial class SearchDirectory : Form
    {
        public List<ITextAdapter> TextAdapters { private get; set; }
        public SearchDirectoryResult Return { get; set; }

        public SearchDirectory()
        {
            InitializeComponent();
            Icon = Resources.search;
        }

        private void frmSearchDirectory_Load(object sender, EventArgs e)
        {
            if (TextAdapters == null) Close();
            txtSearchDirectory.Text = Settings.Default.SearchDirectoryDirectory;
            txtSearchText.Text = Settings.Default.SearchDirectoryWhat;
            chkMatchCase.Checked = Settings.Default.SearchDirectoryMatchCase;
            chkSearchSubfolders.Checked = Settings.Default.SearchDirectorySubfolders;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog
            {
                Description = "Select the directory to search through.",
                SelectedPath = Settings.Default.SearchDirectoryDirectory
            };

            if (fbd.ShowDialog() == DialogResult.OK)
                txtSearchDirectory.Text = fbd.SelectedPath;
            else
                return;

            Settings.Default.SearchDirectoryDirectory = fbd.SelectedPath;
            Settings.Default.Save();
        }

        private void btnFindText_Click(object sender, EventArgs e)
        {
            lstResults.Items.Clear();

            if (txtSearchDirectory.Text.Trim() != string.Empty && Directory.Exists(txtSearchDirectory.Text.Trim()))
            {
                if (txtSearchText.Text.Trim() != string.Empty)
                {
                    var types = TextAdapters.Select(x => x.Extension.ToLower()).Select(y => y.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(z => z).Distinct().ToList();

                    var files = new List<string>();
                    foreach (var type in types)
                        if (type != "*.kup")
                            files.AddRange(Directory.GetFiles(txtSearchDirectory.Text.Trim(), type, chkSearchSubfolders.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

                    var results = new List<SearchDirectoryResult>();

                    foreach (var file in files)
                    {
                        var fi = new FileInfo(file);
                        var currentAdapter = Editor.SelectTextAdapter(TextAdapters, file, true);

                        try
                        {
                            if (currentAdapter != null)
                            {
                                currentAdapter.Load(file, true);
                                if (txtSearchText.Text.Trim() != string.Empty && currentAdapter.Entries != null)
                                {
                                    foreach (var entry in currentAdapter.Entries)
                                    {
                                        if (chkMatchCase.Checked)
                                        {
                                            if (entry.EditedText.Contains(txtSearchText.Text) || entry.OriginalText.Contains(txtSearchText.Text) || entry.Name.Contains(txtSearchText.Text))
                                                results.Add(new SearchDirectoryResult { Filename = file, Entry = entry, Index = currentAdapter.Entries.ToList().IndexOf(entry) });
                                        }
                                        else
                                        {
                                            if (entry.EditedText.ToLower().Contains(txtSearchText.Text.ToLower()) || entry.OriginalText.ToLower().Contains(txtSearchText.Text.ToLower()) || entry.Name.ToLower().Contains(txtSearchText.Text.ToLower()))
                                                results.Add(new SearchDirectoryResult { Filename = file, Entry = entry, Index = currentAdapter.Entries.ToList().IndexOf(entry) });
                                        }

                                        foreach (var subEntry in entry.SubEntries)
                                        {
                                            if (chkMatchCase.Checked)
                                            {
                                                if (subEntry.EditedText.Contains(txtSearchText.Text) || subEntry.OriginalText.Contains(txtSearchText.Text) || subEntry.Name.Contains(txtSearchText.Text))
                                                    results.Add(new SearchDirectoryResult { Filename = file, Entry = subEntry, Index = currentAdapter.Entries.ToList().IndexOf(subEntry) });
                                            }
                                            else
                                            {
                                                if (subEntry.EditedText.ToLower().Contains(txtSearchText.Text.ToLower()) || subEntry.OriginalText.ToLower().Contains(txtSearchText.Text.ToLower()) || subEntry.Name.ToLower().Contains(txtSearchText.Text.ToLower()))
                                                    results.Add(new SearchDirectoryResult { Filename = file, Entry = subEntry, Index = currentAdapter.Entries.ToList().IndexOf(subEntry) });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    lstResults.Items.AddRange(results.ToArray());
                }
            }

            if (lstResults.Items.Count == 0)
            {
                MessageBox.Show($"Could not find \"{txtSearchText.Text}\".", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void txtSearchDirectory_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.SearchDirectoryDirectory = txtSearchDirectory.Text;
            Settings.Default.Save();
        }

        private void txtSearchText_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.SearchDirectoryWhat = txtSearchText.Text;
            Settings.Default.Save();
        }

        private void chkMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.SearchDirectoryMatchCase = chkMatchCase.Checked;
            Settings.Default.Save();
        }

        private void chkSearchSubfolders_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.SearchDirectorySubfolders = chkSearchSubfolders.Checked;
            Settings.Default.Save();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lstResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstResults.Items.Count > 0 && lstResults.SelectedIndex >= 0)
            {
                Return = (SearchDirectoryResult)lstResults.SelectedItem;
                Close();
            }
        }
    }

    public class SearchDirectoryResult
    {
        public string Filename { get; set; }
        public TextEntry Entry { get; set; }
        public int Index { get; set; }

        public override string ToString()
        {
            return new FileInfo(Filename).Name + " - " + Entry;
        }
    }
}