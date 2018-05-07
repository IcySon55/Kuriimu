using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ext_ridge_racer.Properties;

namespace ext_ridge_racer
{
    public partial class frmExtension : Form
    {
        private FileInfo _file = null;
        private bool _fileOpen = false;
        private bool _hasChanges = false;

        private BGMDB _bgmdb = null;

        public frmExtension(string[] args = null)
        {
            InitializeComponent();

            // Load passed in file
            if (args != null && args.Length > 0 && File.Exists(args[0]))
                OpenFile(args[0]);
        }

        private void Fenceposts_Load(object sender, EventArgs e)
        {
            Text = Settings.Default.PluginName;
            Icon = Resources.ridge_racer;

            UpdateForm();
        }

        private void Fenceposts_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_hasChanges)
            {
                DialogResult dr = MessageBox.Show("Would you like to save your changes before exiting?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                {
                    if (SaveFile() != DialogResult.OK)
                        e.Cancel = true;
                }
                else if (dr == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfirmNewFile();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfirmOpenFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tsbNew_Click(object sender, EventArgs e)
        {
            ConfirmNewFile();
        }

        private void tsbOpen_Click(object sender, EventArgs e)
        {
            ConfirmOpenFile();
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void tsbSaveAs_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }

        private void ConfirmNewFile()
        {
            DialogResult dr = DialogResult.No;

            if (_fileOpen && _hasChanges)
                dr = MessageBox.Show("You have unsaved changes in " + FileName() + ". Save changes before creating a new file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            switch (dr)
            {
                case DialogResult.Yes:
                    dr = SaveFile();
                    if (dr == DialogResult.OK) NewFile();
                    break;
                case DialogResult.No:
                    NewFile();
                    break;
            }
        }

        private void ConfirmOpenFile(string filename = "")
        {
            DialogResult dr = DialogResult.No;

            if (_fileOpen && _hasChanges)
                dr = MessageBox.Show("You have unsaved changes in " + FileName() + ". Save changes before opening another file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            switch (dr)
            {
                case DialogResult.Yes:
                    dr = SaveFile();
                    if (dr == DialogResult.OK) OpenFile(filename);
                    break;
                case DialogResult.No:
                    OpenFile(filename);
                    break;
            }
        }

        private void NewFile()
        {
            _file = null;
            _fileOpen = true;
            _bgmdb = new BGMDB();
            LoadForm();
            _hasChanges = false;
            UpdateForm();
        }

        private void OpenFile(string filename = "")
        {
            var ofd = new OpenFileDialog { Filter = "Ridge Racer BGMDB Binary (*.bin)|*.bin" };
            DialogResult dr = DialogResult.OK;

            if (filename == string.Empty)
            {
                dr = ofd.ShowDialog();
                filename = ofd.FileName;
            }

            if (dr == DialogResult.OK)
            {
                try
                {
                    _file = new FileInfo(filename);
                    _fileOpen = true;
                    _bgmdb = BGMDB.Load(File.OpenRead(_file.FullName));
                    LoadForm();
                    _hasChanges = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK);
                    _fileOpen = false;
                    _hasChanges = false;
                }

                UpdateForm();
            }
        }

        private DialogResult SaveFile(bool saveAs = false)
        {
            if (!ProcessForm())
                return DialogResult.Abort;

            var sfd = new SaveFileDialog { Filter = "Ridge Racer BGMDB Binary (*.bin)|*.bin" };
            DialogResult dr = DialogResult.OK;

            if (_file == null || saveAs)
                dr = sfd.ShowDialog();

            if ((_file == null || saveAs) && dr == DialogResult.OK)
                _file = new FileInfo(sfd.FileName);

            if (dr == DialogResult.OK)
            {
                _bgmdb.Save(File.Create(_file.FullName));
                _hasChanges = false;
                UpdateForm();
            }

            return dr;
        }

        private void LoadForm()
        {
            if (_fileOpen)
            {
                txtID.Text = _bgmdb.Data.nID.ToString();
                txtBgmName.Text = _bgmdb.BgmName;
                txtArtistName.Text = _bgmdb.ArtistName;
                txtOrder.Text = _bgmdb.Data.nOrder.ToString();
                txtRcid.Text = _bgmdb.Rcid;
            }
        }

        private bool ProcessForm()
        {
            var result = true;
            var errors = new List<string>();

            // Slot
            if (!int.TryParse(txtID.Text, out var nID) || nID < 1)
            {
                errors.Add("The Slot is not a valid number greater than 0.");
                result = false;
            }
            _bgmdb.Data.nID = Math.Max(nID, 1);

            _bgmdb.BgmName = txtBgmName.Text;
            _bgmdb.ArtistName = txtArtistName.Text;

            // Order
            if (!int.TryParse(txtOrder.Text, out var nOrder) || nOrder < 0)
            {
                errors.Add("The Order is not a valid non-negative number.");
                result = false;
            }
            _bgmdb.Data.nOrder = nOrder;

            _bgmdb.Rcid = txtRcid.Text;

            if (errors.Count > 0)
                MessageBox.Show(string.Join("\r\n", errors), "Form Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return result;
        }

        // Utilities
        private void UpdateForm()
        {
            Text = Settings.Default.PluginName + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

            saveToolStripMenuItem.Enabled = _fileOpen;
            saveAsToolStripMenuItem.Enabled = _fileOpen;
            tsbSave.Enabled = _fileOpen;
            tsbSaveAs.Enabled = _fileOpen;
        }

        private string FileName()
        {
            string result = "Untitled";

            if (_file != null && _fileOpen)
                result = _file.Name;
            else if (!_fileOpen)
                result = string.Empty;

            return result;
        }

        // Functions


        // Toolbar


        // Change Detection
        private void txt_TextChanged(object sender, EventArgs e)
        {
            _hasChanges = true;
            UpdateForm();
        }

        private void tsbKarameru_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, "karameru.exe"));
            start.WorkingDirectory = Application.StartupPath;

            Process p = new Process();
            p.StartInfo = start;
            p.Start();
        }
    }
}