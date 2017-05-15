using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Kuriimu.Properties;
using Kuriimu.Contract;
using Kuriimu.IO;
using System.Collections.Generic;
using System.Linq;
using Kuriimu.Contract.IO;

namespace Kuriimu
{
    public partial class SequenceSearch : Form
    {
        public class SearchResult
        {
            public string Filename { get; set; }
            public int Offset { get; set; }

            public override string ToString()
            {
                return $"{new FileInfo(Filename).Name} - {Offset} (0x{Offset:X2})";
            }
        }

        public SequenceSearch()
        {
            InitializeComponent();
            Icon = Resources.search;
        }

        private void frmSearchDirectory_Load(object sender, EventArgs e)
        {
            txtSearchDirectory.Text = Settings.Default.SequenceSearchDirectory;
            txtSearchText.Text = Settings.Default.SequenceSearchWhat;
            chkSearchSubfolders.Checked = Settings.Default.SequenceSearchSubfolders;

            try
            {
                var enc = Encoding.GetEncoding(Settings.Default.SequenceSearchEncoding);
                Tools.LoadSupportedEncodings(cmbEncoding, enc);
            }
            catch (ArgumentException)
            {
                Tools.LoadSupportedEncodings(cmbEncoding, Encoding.Unicode);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog
            {
                Description = "Select the directory to search through.",
                SelectedPath = Settings.Default.SequenceSearchDirectory
            };

            if (fbd.ShowDialog() == DialogResult.OK)
                txtSearchDirectory.Text = fbd.SelectedPath;
            else
                return;

            Settings.Default.SequenceSearchDirectory = fbd.SelectedPath;
            Settings.Default.Save();
        }

        private void btnFindText_Click(object sender, EventArgs e)
        {
            lstResults.Items.Clear();

            if (txtSearchDirectory.Text.Trim() != string.Empty && Directory.Exists(txtSearchDirectory.Text.Trim()))
            {
                if (txtSearchText.Text.Trim() != string.Empty)
                {
                    var W = (cmbEncoding.SelectedValue as Encoding)?.GetBytes(txtSearchText.Text);
                    var files = Directory.GetFiles(txtSearchDirectory.Text.Trim(), "*.*", (chkSearchSubfolders.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                    var results = new List<SearchResult>();
                    var searcher = new KmpSearcher(W);

                    foreach (var file in files.Where(o => new FileInfo(o).Length < (8 * 1024 * 1024)))
                    {
                        using (var br = new BinaryReaderX(File.OpenRead(file)))
                        {
                            var offset = searcher.Search(br);

                            if (offset >= 0)
                                results.Add(new SearchResult { Filename = file, Offset = offset });
                        }
                    }

                    lstResults.Items.AddRange(results.ToArray());
                }
            }

            if (lstResults.Items.Count == 0)
            {
                MessageBox.Show($"Could not find \"{txtSearchText.Text}\".", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void txtSearchText_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.SequenceSearchWhat = txtSearchText.Text;
            Settings.Default.Save();
        }

        private void cmbEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.SequenceSearchEncoding = ((Encoding)cmbEncoding.SelectedValue).WebName;
            Settings.Default.Save();
        }

        private void chkSearchSubfolders_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.SequenceSearchSubfolders = chkSearchSubfolders.Checked;
            Settings.Default.Save();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}