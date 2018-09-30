using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ext_fenceposts.Properties;
using Kontract.Interface;
using Kontract.IO;
using Kontract;

namespace ext_fenceposts
{
    public partial class frmExtension : Form
    {
        private FileInfo _file = null;
        private IGameHandler _gameHandler = null;
        private bool _fileOpen = false;
        private bool _hasChanges = false;

        private KUP _kup = null;
        private KupUser _kupUser = null;
        private Dictionary<long, long> _pointers = null;
        private List<IGameHandler> _gameHandlers = null;

        private BackgroundWorker _workerDumper = new BackgroundWorker();
        private BackgroundWorker _workerInjector = new BackgroundWorker();

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
            Icon = Resources.fenceposts;

            // Load Plugins
            _gameHandlers = Tools.LoadGameHandlers(Settings.Default.PluginDirectory, tsbGameSelect, Resources.game_none, tsbGameSelect_SelectedIndexChanged);
            _gameHandler = _gameHandlers.FirstOrDefault();

            // Configure workers
            _workerDumper.DoWork += new DoWorkEventHandler(workerDumper_DoWork);
            _workerDumper.ProgressChanged += new ProgressChangedEventHandler(workerDumper_ProgressChanged);
            _workerDumper.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerDumper_RunWorkerCompleted);
            _workerDumper.WorkerReportsProgress = true;
            _workerDumper.WorkerSupportsCancellation = true;

            _workerInjector.DoWork += new DoWorkEventHandler(workerInjector_DoWork);
            _workerInjector.ProgressChanged += new ProgressChangedEventHandler(workerInjector_ProgressChanged);
            _workerInjector.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerInjector_RunWorkerCompleted);
            _workerInjector.WorkerReportsProgress = true;
            _workerInjector.WorkerSupportsCancellation = true;

            UpdateForm();
        }

        private void Fenceposts_FormClosing(object sender, FormClosingEventArgs e)
        {
            _workerDumper.CancelAsync();
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

        private void tsbGameSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripItem tsi = (ToolStripItem)sender;

            _gameHandler = tsi.Tag as IGameHandler;

            tsbGameSelect.Text = tsi.Text;
            tsbGameSelect.Image = tsi.Image;
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
            _kup = new KUP();
            _kupUser = new KupUser();
            LoadForm();
            _hasChanges = false;
            UpdateForm();
        }

        private void OpenFile(string filename = "")
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Kuriimu Archive (*.kup)|*.kup";
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
                    _kup = KUP.Load(filename);
                    _kupUser = KupUser.Load(filename + ".user");
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
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Kuriimu Archive (*.kup)|*.kup";
            DialogResult dr = DialogResult.OK;

            if (_file == null || saveAs)
            {
                dr = sfd.ShowDialog();
            }

            if ((_file == null || saveAs) && dr == DialogResult.OK)
            {
                _file = new FileInfo(sfd.FileName);
            }

            if (dr == DialogResult.OK)
            {
                _kup.Save(_file.FullName);
                _kupUser.Save(_file.FullName + ".user");
                _hasChanges = false;
                UpdateForm();
            }

            return dr;
        }

        private void LoadForm()
        {
            lstPointerTables.Items.Clear();
            foreach (Bound bound in _kup.PointerTables)
                lstPointerTables.Items.Add(bound);
            if (lstPointerTables.Items.Count > 0)
                lstPointerTables.SelectedIndex = 0;

            lstStringBounds.Items.Clear();
            foreach (Bound bound in _kup.StringBounds)
                lstStringBounds.Items.Add(bound);
            if (lstStringBounds.Items.Count > 0)
                lstStringBounds.SelectedIndex = 0;

            txtFilename.Text = _kupUser.Filename;
            chkCleanDump.Checked = Settings.Default.CleanDump;
        }

        // Utilities
        private void UpdateForm()
        {
            Text = Settings.Default.PluginName + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

            saveToolStripMenuItem.Enabled = _fileOpen;
            saveAsToolStripMenuItem.Enabled = _fileOpen;
            tsbSave.Enabled = _fileOpen;
            tsbSaveAs.Enabled = _fileOpen;

            splConfigure.Enabled = _fileOpen;
            splBounds.Enabled = _fileOpen;

            tsbGameSelect.Enabled = _fileOpen;

            bool pointerTableSelected = lstPointerTables.SelectedIndex >= 0;
            tsbPointerTableAdd.Enabled = _fileOpen;
            tsbPointerTableProperties.Enabled = pointerTableSelected;
            tsbPointerTableDelete.Enabled = pointerTableSelected;
            lstPointerTables.Enabled = _fileOpen;

            bool dumpingBoundSelected = lstStringBounds.SelectedIndex >= 0;
            tsbStringBoundAdd.Enabled = _fileOpen;
            tsbStringBoundProperties.Enabled = dumpingBoundSelected;
            tsbStringBoundDelete.Enabled = dumpingBoundSelected;
            lstStringBounds.Enabled = _fileOpen;

            txtFilename.Enabled = _fileOpen;
            btnBrowse.Enabled = _fileOpen;
            btnOptions.Enabled = _fileOpen;
            chkCleanDump.Enabled = _fileOpen;
            btnDump.Enabled = (lstPointerTables.Items.Count > 0 && lstStringBounds.Items.Count > 0);
            btnInject.Enabled = lstStringBounds.Items.Count > 0;
            lstStatus.Enabled = _fileOpen;
        }

        private string FileName()
        {
            string result = "Untitled";

            if (_file != null && _fileOpen)
                result = _file.Name;

            return result;
        }

        // Functions
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
                txtFilename.Text = ofd.FileName;
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            Options options = new Options(_kup);
            options.ShowDialog();

            if (options.HasChanges)
            {
                _hasChanges = true;
                UpdateForm();
            }
        }

        private void btnDump_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtFilename.Text))
            {
                tlsMain.Enabled = false;
                tlsPointerTables.Enabled = false;
                tlsStringBounds.Enabled = false;
                lstPointerTables.Enabled = false;
                lstStringBounds.Enabled = false;
                txtFilename.Enabled = false;
                btnBrowse.Enabled = false;
                btnDump.Enabled = false;
                btnInject.Enabled = false;

                prgTop.Value = 0;
                prgBottom.Value = 0;
                lstStatus.Items.Clear();
                if (chkCleanDump.Checked)
                    _kup.Entries.Clear();
                _workerDumper.RunWorkerAsync();
            }
        }

        private void btnInject_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtFilename.Text))
            {
                tlsMain.Enabled = false;
                tlsPointerTables.Enabled = false;
                tlsStringBounds.Enabled = false;
                lstPointerTables.Enabled = false;
                lstStringBounds.Enabled = false;
                txtFilename.Enabled = false;
                btnBrowse.Enabled = false;
                btnDump.Enabled = false;
                btnInject.Enabled = false;

                prgTop.Value = 0;
                prgBottom.Value = 0;
                lstStatus.Items.Clear();
                _workerInjector.RunWorkerAsync();
            }
        }

        // Dumper
        private void workerDumper_DoWork(object sender, DoWorkEventArgs e)
        {
            FileStream fs = new FileStream(txtFilename.Text, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReaderX br = new BinaryReaderX(fs, ByteOrder.LittleEndian);

            // Pointer Tables
            _pointers = new Dictionary<long, long>();

            _workerDumper.ReportProgress(0, "STATUS|Dumping pointers...");
            foreach (Bound pointerBound in _kup.PointerTables)
            {
                long end = Math.Max(4, pointerBound.EndLong);

                if (end == 4)
                    end += pointerBound.StartLong;

                br.BaseStream.Seek(pointerBound.StartLong, SeekOrigin.Begin);
                while (br.BaseStream.Position < end)
                {
                    uint offset = br.ReadUInt32() - _kup.RamOffsetUInt;
                    if (offset < br.BaseStream.Length)
                        _pointers.Add(br.BaseStream.Position - sizeof(uint), offset); // AAAHAAAAARRRRRRGGRGGGGHHHHHHH

                    _workerDumper.ReportProgress((int)(((double)br.BaseStream.Position - pointerBound.StartLong) / ((double)pointerBound.EndLong - pointerBound.StartLong) * prgBottom.Maximum), "BOTTOM");
                }
            }
            _workerDumper.ReportProgress(0, "STATUS|Found " + _pointers.Count + " pointers...");

            _workerDumper.ReportProgress(0, "STATUS|Dumping strings...");
            for (int i = 0; i < _kup.StringBounds.Count; i++)
            {
                Bound stringBound = _kup.StringBounds[i];

                _workerDumper.ReportProgress((int)(((double)i + 1 / (double)_kup.StringBounds.Count) * prgBottom.Maximum), "BOTTOM");

                if (stringBound.Dumpable)
                {
                    var result = new List<byte>();
                    var offset = stringBound.StartLong;
                    long jumpBack = 0;

                    br.BaseStream.Seek(stringBound.StartLong, SeekOrigin.Begin);
                    while (br.BaseStream.Position < stringBound.EndLong)
                    {
                        var unichar = br.ReadBytes(_kup.Encoding.IsSingleByte ? 1 : 2);
                        result.AddRange(unichar);

                        if (stringBound.Injectable)
                        {
                            if (jumpBack == 0 && (_pointers.Values.Contains(br.BaseStream.Position) || br.BaseStream.Position == stringBound.EndLong))
                                jumpBack = br.BaseStream.Position;

                            if ((_pointers.Values.Contains(br.BaseStream.Position) || br.BaseStream.Position == stringBound.EndLong) && (result[result.Count - 1] == 0x00 && (_kup.Encoding.IsSingleByte || result[result.Count - 2] == 0x00)))
                            {
                                if (result[result.Count - 1] == 0x00 && (_kup.Encoding.IsSingleByte || result[result.Count - 2] == 0x00))
                                {
                                    result.RemoveAt(result.Count - 1);
                                    if (!_kup.Encoding.IsSingleByte)
                                        result.RemoveAt(result.Count - 1);
                                }

                                var entry = new Entry { OffsetLong = offset };

                                // Merging
                                var matched = false;
                                foreach (var ent in _kup.Entries)
                                    if (ent.Offset == entry.Offset)
                                    {
                                        entry = ent;
                                        matched = true;
                                        break;
                                    }

                                // Pointers
                                foreach (var key in _pointers.Keys)
                                    if (_pointers[key] == offset)
                                        entry.AddPointer(key);

                                // Text
                                if (Equals(_kup.Encoding, Encoding.ASCII))
                                {
                                    var parsed = new StringBuilder();
                                    foreach (var b in result)
                                        parsed.Append((char)b);
                                    entry.OriginalText = _gameHandler.GetKuriimuString(parsed.ToString()).Replace("\0", "<null>").Replace("\n", "\r\n"); ;
                                }
                                else
                                    entry.OriginalText = _gameHandler.GetKuriimuString(_kup.Encoding.GetString(result.ToArray())).Replace("\0", "<null>").Replace("\n", "\r\n");

                                // Not Matched
                                if (!matched)
                                {
                                    entry.Relocatable = stringBound.Injectable;
                                    entry.Name = Regex.Match(entry.OriginalText, @"\w+", RegexOptions.IgnoreCase).Value;
                                    entry.EditedText = entry.OriginalText;
                                    _kup.Entries.Add(entry);
                                }

                                if (entry.OriginalText.Contains("<null>"))
                                    _workerDumper.ReportProgress(0, "STATUS|Found a potentially broken string at " + entry.Offset + ": " + entry.Name + "|" + entry.Offset);

                                if (jumpBack > 0)
                                {
                                    br.BaseStream.Seek(jumpBack, SeekOrigin.Begin);
                                    jumpBack = 0;
                                }

                                offset = br.BaseStream.Position;
                                result.Clear();
                            }
                        }
                        else
                        {
                            if (br.BaseStream.Position == stringBound.EndLong)
                            {
                                if (result.Count >= (_kup.Encoding.IsSingleByte ? 1 : 2))
                                {
                                    if (result[result.Count - 1] == 0x00 && (_kup.Encoding.IsSingleByte || result[result.Count - 2] == 0x00))
                                    {
                                        result.RemoveAt(result.Count - 1);
                                        if (!_kup.Encoding.IsSingleByte)
                                            result.RemoveAt(result.Count - 1);
                                    }

                                    var entry = new Entry { OffsetLong = stringBound.StartLong };

                                    // Merging
                                    var matched = false;
                                    foreach (var ent in _kup.Entries)
                                        if (ent.Offset == entry.Offset)
                                        {
                                            entry = ent;
                                            matched = true;
                                            break;
                                        }

                                    // Text
                                    if (Equals(_kup.Encoding, Encoding.ASCII))
                                    {
                                        var parsed = new StringBuilder();
                                        foreach (var b in result)
                                            parsed.Append((char)b);
                                        entry.OriginalText = _gameHandler.GetKuriimuString(parsed.ToString()).Replace("\0", "<null>").Replace("\n", "\r\n"); ;
                                    }
                                    else
                                        entry.OriginalText = _gameHandler.GetKuriimuString(_kup.Encoding.GetString(result.ToArray())).Replace("\0", "<null>").Replace("\n", "\r\n");

                                    // Not Matched
                                    if (!matched)
                                    {
                                        entry.Relocatable = stringBound.Injectable;
                                        entry.Name = Regex.Match(entry.OriginalText, @"\w+", RegexOptions.IgnoreCase).Value;
                                        entry.EditedText = entry.OriginalText;
                                        entry.MaxLength = entry.EditedText.Length;
                                        _kup.Entries.Add(entry);
                                    }
                                }

                                result.Clear();
                            }
                        }

                        _workerDumper.ReportProgress((int)(((double)br.BaseStream.Position - stringBound.StartLong) / ((double)stringBound.EndLong - stringBound.StartLong) * prgTop.Maximum), "TOP");
                    }
                }
            }
            _workerDumper.ReportProgress(0, "STATUS|Dumped " + _kup.Entries.Count + " strings...");

            br.Close();
        }

        private void workerDumper_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string[] items = e.UserState.ToString().Split('|');

            switch (items[0])
            {
                case "TOP":
                    prgTop.Value = Math.Max(Math.Min(e.ProgressPercentage, prgTop.Maximum), 0);
                    break;
                case "BOTTOM":
                    prgBottom.Value = Math.Max(Math.Min(e.ProgressPercentage, prgBottom.Maximum), 0);
                    break;
                case "STATUS":
                    if (items.Length == 3)
                        lstStatus.Items.Add(new ListItem(items[1], items[2]));
                    else
                        lstStatus.Items.Add(items[1]);
                    if (lstStatus.Items.Count > 0)
                        lstStatus.SelectedIndex = lstStatus.Items.Count - 1;
                    break;
            }
        }

        private void workerDumper_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            prgTop.Value = prgTop.Maximum;
            prgBottom.Value = prgBottom.Maximum;
            tlsMain.Enabled = true;
            tlsPointerTables.Enabled = true;
            tlsStringBounds.Enabled = true;
            _hasChanges = true;
            UpdateForm();
        }

        // Injector

        private void workerInjector_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Entry> injectedEntries = new List<Entry>();

            // Sort Entries
            if (_kup.OptimizeStrings)
                _kup.Entries = _kup.Entries.Select(o => o).OrderByDescending(o => _kup.Encoding.GetByteCount(o.EditedText)).ThenBy(o => o.EditedText).ToList();

            // Bound Setup
            foreach (Bound bound in _kup.StringBounds)
                bound.NextAvailableOffset = bound.StartLong;

            // Copy Injection File
            File.Copy(txtFilename.Text, txtFilename.Text + ".injected", true);
            FileInfo codeFile = new FileInfo(txtFilename.Text + ".injected");

            // Begin Injection
            FileStream fs = new FileStream(codeFile.FullName, FileMode.Open, FileAccess.Write, FileShare.Read);
            BinaryWriterX bw = new BinaryWriterX(fs, ByteOrder.LittleEndian);

            _workerInjector.ReportProgress(0, "STATUS|Injecting strings...");

            bool outOfSpace = false;
            int count = 0, optimizedCount = 0;
            foreach (Entry entry in _kup.Entries)
            {
                byte[] editedText = _kup.Encoding.GetBytes(entry.EditedText);
                count++;

                if (entry.Relocatable)
                {
                    // Optimization pass
                    bool optimized = false;

                    if (_kup.OptimizeStrings)
                    {
                        foreach (Entry injectedEntry in injectedEntries)
                        {
                            if (injectedEntry.EditedText.EndsWith(entry.EditedText))
                            {
                                byte[] injectedText = _kup.Encoding.GetBytes(injectedEntry.EditedText);
                                // Update the pointer
                                foreach (Pointer pointer in entry.Pointers)
                                {
                                    bw.BaseStream.Seek(pointer.AddressLong, SeekOrigin.Begin);
                                    bw.Write((uint)(injectedEntry.InjectedOffsetLong + (injectedText.Length - editedText.Length) + _kup.RamOffsetUInt));
                                }
                                optimized = true;
                                optimizedCount++;
                                break;
                            }
                        }
                    }

                    if (!optimized)
                    {
                        // Select bound
                        Bound bound = null;
                        foreach (Bound stringBound in _kup.StringBounds)
                        {
                            if (!stringBound.Full && stringBound.Injectable && editedText.Length < stringBound.SpaceRemaining)
                            {
                                bound = stringBound;
                                break;
                            }
                        }

                        if (bound != null)
                        {
                            // Update the pointer
                            foreach (Pointer pointer in entry.Pointers)
                            {
                                bw.BaseStream.Seek(pointer.AddressLong, SeekOrigin.Begin);
                                bw.Write((uint)(bound.NextAvailableOffset + _kup.RamOffsetUInt));
                            }

                            // Write the string
                            bw.BaseStream.Seek(bound.NextAvailableOffset, SeekOrigin.Begin);
                            bw.Write(editedText);
                            entry.InjectedOffsetLong = bound.NextAvailableOffset;
                            if (_kup.Encoding.IsSingleByte)
                                bw.Write(new byte[] { 0x0 });
                            else
                                bw.Write(new byte[] { 0x0, 0x0 });
                            bound.NextAvailableOffset = bw.BaseStream.Position;

                            injectedEntries.Add(entry);
                        }
                        else
                        {
                            // Ran out of injection space
                            outOfSpace = true;
                            break;
                        }
                    }
                }
                else
                {
                    // In place string update
                    bw.BaseStream.Seek(entry.OffsetLong, SeekOrigin.Begin);
                    var start = bw.BaseStream.Position;
                    bw.Write(editedText, 0, Math.Min(editedText.Length, entry.MaxLength));
                    if (_kup.Encoding.IsSingleByte)
                    {
                        if (entry.MaxLength - (bw.BaseStream.Position - start) > 0)
                            bw.Write(new byte[] { 0x0 });
                    }
                    else
                    {
                        if (entry.MaxLength - (bw.BaseStream.Position - start) > 1)
                            bw.Write(new byte[] { 0x0, 0x0 });
                    }
                }

                _workerInjector.ReportProgress((int)(((double)count) / ((double)_kup.Entries.Count) * prgBottom.Maximum), "BOTTOM");
            }

            if (outOfSpace)
            {
                _workerInjector.ReportProgress(0, "STATUS|The injector has run out of space to inject strings.");
                _workerInjector.ReportProgress(0, "STATUS|Injected " + injectedEntries.Count + " strings...");
                if (_kup.OptimizeStrings)
                    _workerInjector.ReportProgress(0, "STATUS|Optimized " + optimizedCount + " strings...");
                _workerInjector.ReportProgress(0, "STATUS|" + (_kup.Entries.Count - count) + " strings were not injected.");
            }
            else
            {
                _workerInjector.ReportProgress(0, "STATUS|Injected " + injectedEntries.Count + " strings...");
                if (_kup.OptimizeStrings)
                    _workerInjector.ReportProgress(0, "STATUS|Optimized " + optimizedCount + " strings...");
            }

            bw.Close();
        }

        private void workerInjector_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string[] items = e.UserState.ToString().Split('|');

            switch (items[0])
            {
                case "BOTTOM":
                    prgBottom.Value = Math.Max(Math.Min(e.ProgressPercentage, prgBottom.Maximum), 0);
                    break;
                case "STATUS":
                    lstStatus.Items.Add(items[1]);
                    if (lstStatus.Items.Count > 0)
                        lstStatus.SelectedIndex = lstStatus.Items.Count - 1;
                    break;
            }
        }

        private void workerInjector_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            prgBottom.Value = prgBottom.Maximum;
            tlsMain.Enabled = true;
            tlsPointerTables.Enabled = true;
            tlsStringBounds.Enabled = true;
            UpdateForm();
        }

        // Toolbar
        private void tsbPointerTableAdd_Click(object sender, EventArgs e)
        {
            Bound bound = new Bound();
            BoundForm frm = new BoundForm(bound);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                _kup.PointerTables.Add(bound);
                _hasChanges = true;
                LoadForm();
            }
        }

        private void tsbPointerTableProperties_Click(object sender, EventArgs e)
        {
            BoundForm frm = new BoundForm((Bound)lstPointerTables.SelectedItem);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                _hasChanges = true;
                LoadForm();
            }
        }

        private void tsbPointerTableDelete_Click(object sender, EventArgs e)
        {
            Bound bound = (Bound)lstPointerTables.SelectedItem;
            if (MessageBox.Show("Are you sure you want to remove " + bound.Start + " to " + bound.End + "?", "Delete Bound?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _kup.PointerTables.Remove(bound);
                _hasChanges = true;
                LoadForm();
            }
        }

        private void tsbStringBoundAdd_Click(object sender, EventArgs e)
        {
            Bound bound = new Bound();
            bound.Dumpable = true;
            BoundForm frm = new BoundForm(bound);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                _kup.StringBounds.Add(bound);
                _hasChanges = true;
                LoadForm();
            }
        }

        private void tsbStringBoundProperties_Click(object sender, EventArgs e)
        {
            BoundForm frm = new BoundForm((Bound)lstStringBounds.SelectedItem);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                _hasChanges = true;
                LoadForm();
            }
        }

        private void tsbStringBoundDelete_Click(object sender, EventArgs e)
        {
            Bound bound = (Bound)lstStringBounds.SelectedItem;
            if (MessageBox.Show("Are you sure you want to remove " + bound.Start + " to " + bound.End + "?", "Delete Bound?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _kup.StringBounds.Remove(bound);
                _hasChanges = true;
                LoadForm();
            }
        }

        // Events
        private void chkCleanDump_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.CleanDump = chkCleanDump.Checked;
            Settings.Default.Save();
        }

        // List
        private void lstPointerTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void lstStringBounds_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void lstPointerTables_DoubleClick(object sender, EventArgs e)
        {
            tsbPointerTableProperties_Click(sender, e);
        }

        private void lstStringBounds_DoubleClick(object sender, EventArgs e)
        {
            tsbStringBoundProperties_Click(sender, e);
        }

        private void cmsStatus_Opening(object sender, CancelEventArgs e)
        {
            if (lstStatus.SelectedItem == null || lstStatus.SelectedItem.GetType() != typeof(ListItem))
                e.Cancel = true;
        }

        private void copyOffsetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstStatus.SelectedItem != null && lstStatus.SelectedItem.GetType() == typeof(ListItem))
                Clipboard.SetText((string)((ListItem)lstStatus.SelectedItem).Value);
        }

        private void lstStatus_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.C)
                if (lstStatus.SelectedItem != null && lstStatus.SelectedItem.GetType() == typeof(ListItem))
                    Clipboard.SetText((string)((ListItem)lstStatus.SelectedItem).Value);
        }

        private void lstStatus_MouseDown(object sender, MouseEventArgs e)
        {
            lstStatus.SelectedIndex = lstStatus.IndexFromPoint(e.X, e.Y);
        }

        // Change Detection
        private void txtFilename_TextChanged(object sender, EventArgs e)
        {
            _kupUser.Filename = txtFilename.Text;
            _hasChanges = true;
            UpdateForm();
        }
    }
}