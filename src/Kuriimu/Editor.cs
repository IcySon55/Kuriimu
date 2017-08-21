using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Kuriimu.Kontract;
using Kuriimu.Properties;
using Kuriimu.UI;

namespace Kuriimu
{
    public partial class Editor : Form
    {
        private ITextAdapter _textAdapter;
        private IGameHandler _gameHandler;
        private IList<Bitmap> _gameHandlerPages = new List<Bitmap>();
        private bool _fileOpen;
        private bool _hasChanges;
        private KuriimuSettings _settings;

        // Lot
        private Lot _lot;

        private List<ITextAdapter> _textAdapters;
        private List<IGameHandler> _gameHandlers;
        private List<IExtension> _extensions;

        private List<TextEntry> _entries;

        private int _page;

        public Editor(string[] args)
        {
            InitializeComponent();

            // Load Plugins
            _textAdapters = PluginLoader<ITextAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "text*.dll").ToList();
            _gameHandlers = Tools.LoadGameHandlers(Settings.Default.PluginDirectory, tsbGameSelect, Resources.game_none, tsbGameSelect_SelectedIndexChanged);
            _extensions = PluginLoader<IExtension>.LoadPlugins(Settings.Default.PluginDirectory, "ext*.dll").ToList();

            // Load Persist
            var settingsPath = Path.Combine(Application.StartupPath, "Kuriimu.settings");
            if (File.Exists(settingsPath))
                try
                {
                    _settings = KuriimuSettings.Load(settingsPath);
                }
                catch (Exception e)
                {
                    _settings = new KuriimuSettings();
                    _settings.Save(settingsPath);
                }
            else
            {
                _settings = new KuriimuSettings();
                _settings.Save(settingsPath);
            }

            // Load passed in file
            if (args.Length > 0 && File.Exists(args[0]))
                OpenFile(args[0]);
        }

        private void frmEditor_Load(object sender, EventArgs e)
        {
            Icon = Resources.kuriimu;
            Tools.DoubleBuffer(treEntries, true);
            LoadForm();
            UpdateForm();
        }

        private void frmEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_hasChanges)
            {
                DialogResult dr = MessageBox.Show("Would you like to save your changes before exiting?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (dr == DialogResult.Yes)
                    SaveFile();
                else if (dr == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        // Menu/Toolbar
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfirmOpenFile();
        }

        private void tsbOpen_Click(object sender, EventArgs e)
        {
            ConfirmOpenFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }

        private void tsbSaveAs_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Search search = new Search();
            search.Entries = _entries;
            search.ShowDialog();

            if (search.Selected != null)
            {
                treEntries.SelectNodeByTextEntry(search.Selected);

                if (txtEdit.Text.Contains(Settings.Default.FindWhat))
                {
                    txtEdit.SelectionStart = txtEdit.Text.IndexOf(Settings.Default.FindWhat);
                    txtEdit.SelectionLength = Settings.Default.FindWhat.Length;
                    txtEdit.Focus();
                }
            }
        }
        private void tsbFind_Click(object sender, EventArgs e)
        {
            findToolStripMenuItem_Click(sender, e);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_textAdapter.ShowProperties(Resources.kuriimu))
            {
                _hasChanges = true;
                UpdateForm();
            }
        }
        private void tsbFileProperties_Click(object sender, EventArgs e)
        {
            propertiesToolStripMenuItem_Click(sender, e);
        }

        private void scbFontFamily_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SetFont())
            {
                Settings.Default.FontFamily = scbFontFamily.Text;
                Settings.Default.Save();
            }
        }

        private void scbFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SetFont())
            {
                Settings.Default.FontSize = scbFontSize.Text;
                Settings.Default.Save();
            }
        }

        private void scbFontFamily_TextChanged(object sender, EventArgs e)
        {
            scbFontFamily_SelectedIndexChanged(sender, e);
        }

        private void scbFontSize_TextChanged(object sender, EventArgs e)
        {
            scbFontSize_SelectedIndexChanged(sender, e);
        }

        private void tsbKukki_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, "kukkii.exe"));
            start.WorkingDirectory = Application.StartupPath;

            Process p = new Process();
            p.StartInfo = start;
            p.Start();
        }

        private void tsbKarameru_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, "karameru.exe"));
            start.WorkingDirectory = Application.StartupPath;

            Process p = new Process();
            p.StartInfo = start;
            p.Start();
        }

        private void addEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treEntries.Focused)
            {
                TextEntry entry = _textAdapter.NewEntry();

                Name name = new Name(entry.Name, _textAdapter.EntriesHaveUniqueNames, _textAdapter.NameList, _textAdapter.NameFilter, _textAdapter.NameMaxLength, true);

                if (name.ShowDialog() == DialogResult.OK && name.NameChanged)
                {
                    entry.Name = name.NewName;
                    if (_textAdapter.AddEntry(entry))
                    {
                        _hasChanges = true;
                        LoadEntries();
                        treEntries.SelectNodeByTextEntry(entry);
                        UpdateForm();
                    }
                }
            }
        }
        private void tsbEntryAdd_Click(object sender, EventArgs e)
        {
            addEntryToolStripMenuItem_Click(sender, e);
        }

        private void renameEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treEntries.Focused)
            {
                TextEntry entry = (TextEntry)treEntries.SelectedNode.Tag;

                Name name = new Name(entry.Name, _textAdapter.EntriesHaveUniqueNames, _textAdapter.NameList, _textAdapter.NameFilter, _textAdapter.NameMaxLength);

                if (name.ShowDialog() == DialogResult.OK && name.NameChanged)
                {
                    if (_textAdapter.RenameEntry(entry, name.NewName))
                    {
                        _hasChanges = true;
                        treEntries.FindNodeByTextEntry(entry).Text = name.NewName;
                        UpdateForm();
                    }
                }
            }
        }
        private void tsbEntryRename_Click(object sender, EventArgs e)
        {
            renameEntryToolStripMenuItem_Click(sender, e);
        }

        private void deleteEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treEntries.Focused)
            {
                TextEntry entry = (TextEntry)treEntries.SelectedNode.Tag;

                if (MessageBox.Show("Are you sure you want to delete " + entry.Name + "?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (_textAdapter.DeleteEntry(entry))
                    {
                        _hasChanges = true;
                        TreeNode nextNode = treEntries.SelectedNode.NextNode;
                        UpdateEntries();
                        treEntries.Nodes.Remove(treEntries.FindNodeByTextEntry(entry));
                        treEntries.SelectedNode = nextNode;
                    }
                }
            }
        }
        private void tsbEntryDelete_Click(object sender, EventArgs e)
        {
            deleteEntryToolStripMenuItem_Click(sender, e);
        }

        private void entryPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEntry entry = (TextEntry)treEntries.SelectedNode.Tag;
            if (_textAdapter.ShowEntryProperties(entry, Resources.kuriimu))
            {
                _hasChanges = true;
                UpdateForm();
            }
        }
        private void tsbEntryProperties_Click(object sender, EventArgs e)
        {
            entryPropertiesToolStripMenuItem_Click(sender, e);
        }

        private void sequenceSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ss = new SequenceSearch();
            ss.Show();
        }

        private void tsbShowTextPreview_Click(object sender, EventArgs e)
        {
            Settings.Default.ShowTextPreview = !Settings.Default.ShowTextPreview;
            Settings.Default.Save();
            tsbShowTextPreview.Checked = Settings.Default.ShowTextPreview;
            LoadEntries();
        }

        private void sortEntriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _textAdapter.SortEntries = !_textAdapter.SortEntries;
            LoadEntries();
            UpdateForm();
        }
        private void tsbSortEntries_Click(object sender, EventArgs e)
        {
            sortEntriesToolStripMenuItem_Click(sender, e);
        }

        // Extensions
        private void extensionsToolStripMenuItems_Click(object sender, EventArgs e)
        {
            ((IExtension)((ToolStripMenuItem)sender).Tag).CreateInstance().Show();
        }

        // Help
        private void gBATempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://gbatemp.net/threads/release-kuriimu-a-general-purpose-game-translation-toolkit-for-authors-of-fan-translations.452375/");
        }

        private void gitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Icyson55/Kuriimu");
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        // UI Toolbars
        private void tsbGameSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripItem tsi = (ToolStripItem)sender;
            _gameHandler = (IGameHandler)tsi.Tag;
            tsbGameSelect.Text = tsi.Text;
            tsbGameSelect.Image = tsi.Image;

            UpdateTextView();
            UpdatePreview();
            UpdateForm();

            Settings.Default.SelectedGameHandler = tsi.Text;
            Settings.Default.Save();
        }

        private void tsbPreviewEnabled_Click(object sender, EventArgs e)
        {
            Settings.Default.PreviewEnabled = !Settings.Default.PreviewEnabled;
            Settings.Default.Save();
            UpdatePreview();
            UpdateForm();
        }

        private void tsbPreviewSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save Preview as PNG...";
            sfd.InitialDirectory = Settings.Default.LastDirectory;
            sfd.FileName = "preview.png";
            sfd.Filter = "Portable Network Graphics (*.png)|*.png";
            sfd.AddExtension = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = (Bitmap)pbxPreview.Image;
                bmp.Save(sfd.FileName, ImageFormat.Png);

                Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
                Settings.Default.Save();
            }
        }

        private void tsbPreviewCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(pbxPreview.Image);
        }

        private void tsbPreviousPage_Click(object sender, EventArgs e)
        {
            SetPage(-1);
            UpdatePreview();
            UpdateForm();
        }

        private void tsbNextPage_Click(object sender, EventArgs e)
        {
            SetPage(1);
            UpdatePreview();
            UpdateForm();
        }

        private void tsbHandlerSettings_Click(object sender, EventArgs e)
        {
            if (_gameHandler.ShowSettings(Resources.kuriimu))
            {
                UpdatePreview();
                UpdateForm();
            }
        }

        // File Handling
        private void frmEditor_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void frmEditor_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (files.Length > 0 && File.Exists(files[0]))
                ConfirmOpenFile(files[0]);
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

        private void OpenFile(string filename = "")
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = Settings.Default.LastDirectory,
                Filter = Tools.LoadFilters(_textAdapters)
            };

            var dr = DialogResult.OK;

            if (filename == string.Empty)
                dr = ofd.ShowDialog();

            if (dr != DialogResult.OK) return;

            if (filename == string.Empty)
                filename = ofd.FileName;

            var tempAdapter = SelectTextAdapter(filename);

            try
            {
                if (tempAdapter != null)
                {
                    tempAdapter.Load(filename);

                    _textAdapter = tempAdapter;
                    _lot = File.Exists(_textAdapter.FileInfo.FullName + ".lot") ? Lot.Load(_textAdapter.FileInfo.FullName + ".lot") : null;
                    _fileOpen = true;
                    _hasChanges = false;

                    // Select Game Handler
                    foreach (ToolStripItem tsi in tsbGameSelect.DropDownItems)
                        if (tsi.Text == Settings.Default.SelectedGameHandler)
                        {
                            _gameHandler = (IGameHandler)tsi.Tag;
                            tsbGameSelect.Text = tsi.Text;
                            tsbGameSelect.Image = tsi.Image;

                            break;
                        }
                    if (_gameHandler == null)
                        _gameHandler = (IGameHandler)tsbGameSelect.DropDownItems[0].Tag;

                    LoadEntries();
                    UpdateLabels();
                    //UpdateTextView();
                    //UpdatePreview();
                    UpdateForm();
                }

                Settings.Default.LastDirectory = new FileInfo(filename).DirectoryName;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), tempAdapter != null ? $"{tempAdapter.Name} - {tempAdapter.Description} Manager" : "Supported Format Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DialogResult SaveFile(bool saveAs = false)
        {
            var sfd = new SaveFileDialog();
            var dr = DialogResult.OK;

            sfd.Title = $"Save as {_textAdapter.Description}";
            sfd.FileName = _textAdapter.FileInfo.Name;
            sfd.Filter = _textAdapter.Description + " (" + _textAdapter.Extension + ")|" + _textAdapter.Extension;

            if (_textAdapter.FileInfo == null || saveAs)
            {
                sfd.InitialDirectory = Settings.Default.LastDirectory;
                dr = sfd.ShowDialog();
            }

            if ((_textAdapter.FileInfo == null || saveAs) && dr == DialogResult.OK)
            {
                _textAdapter.FileInfo = new FileInfo(sfd.FileName);
                Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
                Settings.Default.Save();
            }

            if (dr != DialogResult.OK) return dr;

            try
            {
                _textAdapter.Save(saveAs ? _textAdapter.FileInfo?.FullName : string.Empty);
                _lot?.Save(_textAdapter.FileInfo.FullName + ".lot");
                _hasChanges = false;
                //LoadEntries();
                UpdateForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), _textAdapter != null ? $"{_textAdapter.Name} - {_textAdapter.Description} Adapter" : "Supported Format Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dr;
        }

        private ITextAdapter SelectTextAdapter(string filename, bool batchMode = false)
        {
            ITextAdapter result = null;

            // first look for adapters whose extension matches that of our filename
            List<ITextAdapter> matchingAdapters = _textAdapters.Where(adapter => adapter.Extension.TrimEnd(';').Split(';').Any(s => filename.ToLower().EndsWith(s.TrimStart('*')))).ToList();

            result = matchingAdapters.FirstOrDefault(adapter => adapter.Identify(filename));

            // if none of them match, then try all other adapters
            if (result == null)
                result = _textAdapters.Except(matchingAdapters).FirstOrDefault(adapter => adapter.Identify(filename));

            if (result == null && !batchMode)
                MessageBox.Show("None of the installed plugins are able to open the file.", "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return result;
        }

        // Loading
        private void LoadForm()
        {
            // Fonts
            string[] fontSizes = { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "30", "36", "48", "72" };
            FontFamily[] fontFamilies = new InstalledFontCollection().Families;

            scbFontFamily.SelectedIndexChanged -= scbFontFamily_SelectedIndexChanged;
            scbFontFamily.TextChanged -= scbFontFamily_TextChanged;

            scbFontFamily.Items.Clear();
            scbFontFamily.Items.AddRange(fontFamilies.Select(o => o.Name).ToArray());
            scbFontFamily.Text = Settings.Default.FontFamily;

            scbFontFamily.SelectedIndexChanged += scbFontFamily_SelectedIndexChanged;
            scbFontFamily.TextChanged += scbFontFamily_TextChanged;

            scbFontSize.SelectedIndexChanged -= scbFontSize_SelectedIndexChanged;
            scbFontSize.TextChanged -= scbFontSize_TextChanged;

            scbFontSize.Items.Clear();
            scbFontSize.Items.AddRange(fontSizes);
            scbFontSize.Text = Settings.Default.FontSize;

            scbFontSize.SelectedIndexChanged += scbFontSize_SelectedIndexChanged;
            scbFontSize.TextChanged += scbFontSize_TextChanged;

            SetFont();

            // Tools
            CompressionTools.LoadCompressionTools(compressionToolStripMenuItem);
            EncryptionTools.LoadEncryptionTools(encryptionToolStripMenuItem);
            HashTools.LoadHashTools(hashToolStripMenuItem);

            // Extensions
            if (_extensions.Count > 0)
            {
                extensionsToolStripMenuItem.DropDownItems.Clear();

                foreach (IExtension extension in _extensions)
                {
                    ToolStripMenuItem tsiExtension = new ToolStripMenuItem(extension.Name, extension.Icon, extensionsToolStripMenuItems_Click);
                    tsiExtension.Tag = extension;
                    extensionsToolStripMenuItem.DropDownItems.Add(tsiExtension);
                }
            }
        }

        private bool SetFont()
        {
            bool result = true;

            try
            {
                float size;
                float.TryParse(scbFontSize.Text, out size);
                if (float.IsNaN(size) || float.IsInfinity(size) || size <= 0) size = 10;
                txtEdit.Font = new Font(scbFontFamily.Text, size);
                txtOriginal.Font = new Font(scbFontFamily.Text, size);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        private void SetPage(int direction)
        {
            _page += direction;

            if (_page < 0)
                _page = 0;
            else if (_gameHandlerPages != null && _page > _gameHandlerPages.Count - 1)
                _page = _gameHandlerPages.Count - 1;

            if (_gameHandlerPages != null && _gameHandlerPages.Count > 0)
                tslPage.Text = (_page + 1) + "/" + _gameHandlerPages.Count;
            else
                tslPage.Text = "0/0";
        }

        private void LoadEntries()
        {
            UpdateEntries();

            treEntries.BeginUpdate();

            TextEntry selectedEntry = null;
            if (treEntries.SelectedNode != null)
                selectedEntry = (TextEntry)treEntries.SelectedNode.Tag;

            treEntries.Nodes.Clear();
            if (_entries != null)
            {
                foreach (var entry in _entries)
                {
                    var imageIndex = 0;

                    if (_lot != null)
                        imageIndex = _settings.Labels.FindIndex(l => l.ID == _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name).LabelID) + 1;

                    var node = new TreeNode(entry + (Settings.Default.ShowTextPreview && entry.HasText ? " - " + entry.EditedText : string.Empty), imageIndex, imageIndex) { Tag = entry, Name = entry.ToString() };
                    if (!entry.HasText)
                        node.ForeColor = Color.Gray;
                    treEntries.Nodes.Add(node);

                    if (_textAdapter.EntriesHaveSubEntries)
                        foreach (var sub in entry.SubEntries)
                        {
                            if (_lot != null)
                                imageIndex = _settings.Labels.FindIndex(l => l.ID == _lot.LotEntries.FirstOrDefault(o => o.Entry == sub.ParentEntry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == sub.Name).LabelID) + 1;

                            var subNode = new TreeNode(sub + (Settings.Default.ShowTextPreview && sub.HasText ? " - " + sub.EditedText : string.Empty), imageIndex, imageIndex) { Tag = sub, Name = sub.ToString() };
                            node.Nodes.Add(subNode);
                        }

                    node.Expand();
                }
            }

            // TODO: Patch this code so that it works with subentries
            if ((selectedEntry == null || !_entries.Contains(selectedEntry)) && treEntries.Nodes.Count > 0)
                treEntries.SelectedNode = treEntries.Nodes[0];
            else
                treEntries.SelectNodeByTextEntry(selectedEntry);

            treEntries.EndUpdate();

            treEntries.Focus();
        }

        private void UpdateEntries()
        {
            _entries = _textAdapter.Entries.ToList();
        }

        // Utilities
        private void UpdateTextView()
        {
            TextEntry entry = (TextEntry)treEntries.SelectedNode?.Tag;

            if (entry?.EditedText == null)
            {
                txtEdit.Text = string.Empty;
                txtOriginal.Text = string.Empty;
            }
            else
            {
                txtEdit.Text = _gameHandler.GetKuriimuString(entry.EditedText).Replace("\0", "<null>").Replace(_textAdapter.LineEndings, "\r\n");
                if (entry.OriginalText != null)
                    txtOriginal.Text = _gameHandler.GetKuriimuString(entry.OriginalText).Replace("\0", "<null>").Replace(_textAdapter.LineEndings, "\r\n");
            }

            if (entry != null && entry.MaxLength != 0)
                txtEdit.MaxLength = entry.MaxLength == 0 ? int.MaxValue : entry.MaxLength;
        }

        private void UpdatePreview()
        {
            var entry = (TextEntry)treEntries.SelectedNode?.Tag;
            _gameHandlerPages = _gameHandler.GeneratePreviews(entry);
            SetPage(0);

            if (entry != null && _gameHandler.HandlerCanGeneratePreviews && Settings.Default.PreviewEnabled && _gameHandlerPages != null && _gameHandlerPages.Count > 0)
                pbxPreview.Image = _gameHandlerPages[_page];
            else
                pbxPreview.Image = null;
        }

        // Lot
        private void UpdateLot()
        {
            if (_lot == null) return;

            var entry = (TextEntry)treEntries.SelectedNode?.Tag;
            if (!entry.HasText) return;

            txtLotNotes.Text = !entry.IsSubEntry ? _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name).Notes : _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.ParentEntry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == entry.Name).Notes;
            UpdateScreenshotList();
        }

        private void UpdateLabels()
        {
            // Clear Labels
            for (var i = tsbManageLabels.DropDownItems.Count - 1; i > 0; i--)
                tsbManageLabels.DropDownItems.RemoveAt(i);
            for (var i = mnuLabels.Items.Count - 1; i > 0; i--)
                mnuLabels.Items.RemoveAt(i);

            if (_lot == null) return;
            if (_settings.Labels.Count < 0) return;

            imlEntries.Images.Clear();
            imlEntries.Images.Add("0", GenerateColorSquare(SystemColors.Window, 14, 14));

            if (_settings.Labels.Count > 0)
            {
                tsbManageLabels.DropDownItems.Add(new ToolStripSeparator());
                mnuLabels.Items.Add(new ToolStripSeparator());
            }

            foreach (var label in _settings.Labels)
            {
                // Images
                imlEntries.Images.Add((_settings.Labels.IndexOf(label) + 1).ToString(), GenerateColorSquare(ColorTranslator.FromHtml(label.Color), 14, 14));

                // Manage
                var tsiLabel = new ToolStripMenuItem(label.Name, GenerateColorSquare(ColorTranslator.FromHtml(label.Color)));
                tsbManageLabels.DropDownItems.Add(tsiLabel);

                var tsiLabelEdit = new ToolStripMenuItem("Edit " + label.Name, Resources.menu_edit, tsiEditLabel_Click) { Tag = label };
                tsiLabel.DropDownItems.Add(tsiLabelEdit);

                var tsiLabelDelete = new ToolStripMenuItem("Delete " + label.Name, Resources.menu_delete, tsiDeleteLabel_Click) { Tag = label };
                tsiLabel.DropDownItems.Add(tsiLabelDelete);

                // Set
                var tsiSetLabel = new ToolStripMenuItem(label.Name, GenerateColorSquare(ColorTranslator.FromHtml(label.Color)), tsiSetLabel_Click) { Tag = label };
                mnuLabels.Items.Add(tsiSetLabel);
            }
        }

        private void mnuLabels_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var entry = (TextEntry)treEntries.SelectedNode?.Tag;
            if (_lot == null || !entry.HasText)
            {
                e.Cancel = true;
                return;
            }
            var lotEntry = !entry.IsSubEntry ? _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name) : _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.ParentEntry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == entry.Name);

            removeLabelToolStripMenuItem.Enabled = lotEntry.LabelID != null && lotEntry.LabelID != string.Empty;
        }

        private void newLabelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_settings == null) return;

            var lblForm = new LabelForm(string.Empty, ColorTranslator.ToHtml(Color.Black), _settings.Labels.Select(o => o.Name).ToList(), true);

            if (lblForm.ShowDialog() == DialogResult.OK && (lblForm.NameChanged || lblForm.ColorChanged))
            {
                var label = new Kontract.Label(lblForm.NewName, lblForm.NewColor);
                _settings.Labels.Add(label);
                _settings.Save(Path.Combine(Application.StartupPath, "Kuriimu.settings"));
                UpdateLabels();
            }
        }

        private void removeLabelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var entry = (TextEntry)treEntries.SelectedNode?.Tag;
            var lotEntry = !entry.IsSubEntry ? _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name) : _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.ParentEntry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == entry.Name);

            var previousLabelID = lotEntry.LabelID;
            lotEntry.LabelID = string.Empty;

            if (lotEntry.LabelID == previousLabelID) return;
            treEntries.SelectedNode.ImageIndex = 0;
            treEntries.SelectedNode.SelectedImageIndex = 0;
            _hasChanges = true;
            UpdateForm();
        }

        private void tsiSetLabel_Click(object sender, EventArgs e)
        {
            var entry = (TextEntry)treEntries.SelectedNode?.Tag;
            var label = (Kontract.Label)((ToolStripItem)sender).Tag;
            var lotEntry = !entry.IsSubEntry ? _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name) : _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.ParentEntry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == entry.Name);

            lotEntry.LabelID = label.ID;
            var newIndex = _settings.Labels.IndexOf(label) + 1;

            if (newIndex == treEntries.SelectedNode.ImageIndex) return;
            treEntries.SelectedNode.ImageIndex = newIndex;
            treEntries.SelectedNode.SelectedImageIndex = newIndex;
            _hasChanges = true;
            UpdateForm();
        }

        private void tsiEditLabel_Click(object sender, EventArgs e)
        {
            if (_settings == null) return;
            var label = (Kontract.Label)((ToolStripItem)sender).Tag;

            var lblForm = new LabelForm(label.Name, label.Color, _settings.Labels.Select(o => o.Name).ToList());

            if (lblForm.ShowDialog() == DialogResult.OK && (lblForm.NameChanged || lblForm.ColorChanged))
            {
                label.Name = lblForm.NewName;
                label.Color = lblForm.NewColor;
                _settings.Save(Path.Combine(Application.StartupPath, "Kuriimu.settings"));
                UpdateLabels();

                UpdateForm();
            }
        }

        private void tsiDeleteLabel_Click(object sender, EventArgs e)
        {
            if (_settings == null) return;
            var label = (Kontract.Label)((ToolStripItem)sender).Tag;

            if (MessageBox.Show($"Are you sure you want to delete the {label.Name} label?", "Confirm Label Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            foreach (var entry in _entries)
            {
                var lotEntry = _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name);
                if (lotEntry.LabelID == label.ID)
                {
                    lotEntry.LabelID = string.Empty;

                    var nodes = treEntries.Nodes.Find(entry.Name, false);
                    foreach (var sNode in nodes)
                    {
                        sNode.ImageIndex = 0;
                        sNode.SelectedImageIndex = 0;
                    }
                    _hasChanges = true;
                }

                foreach (var sub in entry.SubEntries)
                {
                    var lotSubEntry = _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == sub.Name);
                    if (lotSubEntry.LabelID == label.ID)
                    {
                        lotSubEntry.LabelID = string.Empty;

                        var pNodes = treEntries.Nodes.Find(entry.Name, false);
                        foreach (var pNode in pNodes)
                        {
                            var cNodes = pNode.Nodes.Find(sub.Name, false);
                            foreach (var cNode in cNodes)
                            {
                                cNode.ImageIndex = 0;
                                cNode.SelectedImageIndex = 0;
                            }
                        }
                        _hasChanges = true;
                    }
                }
            }

            _settings.Labels.Remove(label);
            _settings.Save(Path.Combine(Application.StartupPath, "Kuriimu.settings"));
            UpdateLabels();

            UpdateForm();
        }

        private Bitmap GenerateColorSquare(Color color, int width = 16, int height = 16)
        {
            var square = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            var gfx = Graphics.FromImage(square);
            gfx.FillRectangle(new SolidBrush(color), new Rectangle(new Point(0, 0), square.Size));
            return square;
        }

        private void UpdateScreenshotList()
        {
            if (_lot == null) return;

            var entry = (TextEntry)treEntries.SelectedNode?.Tag;
            if (!entry.HasText) return;

            var lotEntry = !entry.IsSubEntry ? _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name) : _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.ParentEntry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == entry.Name);

            treLotScreenshots.BeginUpdate();
            treLotScreenshots.Nodes.Clear();
            imlScreenshots.Images.Clear();
            imlScreenshots.TransparentColor = Color.Transparent;
            treLotScreenshots.ItemHeight = imlScreenshots.ImageSize.Height + 6;

            for (var i = 0; i < lotEntry.Screenshots.Count; i++)
            {
                imlScreenshots.Images.Add(i.ToString(), GenerateThumbnail(lotEntry.Screenshots[i].Bitmap));
                treLotScreenshots.Nodes.Add(new TreeNode
                {
                    Text = lotEntry.Screenshots[i].Name,
                    Tag = i,
                    ImageKey = i.ToString(),
                    SelectedImageKey = i.ToString()
                });
            }

            treLotScreenshots.EndUpdate();
        }

        private Bitmap GenerateThumbnail(Bitmap input)
        {
            var thumbWidth = imlScreenshots.ImageSize.Width;
            var thumbHeight = imlScreenshots.ImageSize.Height;
            var thumb = new Bitmap(thumbWidth, thumbHeight, PixelFormat.Format24bppRgb);
            var gfx = Graphics.FromImage(thumb);

            gfx.CompositingQuality = CompositingQuality.HighSpeed;
            gfx.PixelOffsetMode = PixelOffsetMode.Default;
            gfx.SmoothingMode = SmoothingMode.HighSpeed;
            gfx.InterpolationMode = InterpolationMode.Default;

            var wRatio = (float)input.Width / thumbWidth;
            var hRatio = (float)input.Height / thumbHeight;
            var ratio = wRatio >= hRatio ? wRatio : hRatio;

            if (input.Width <= thumbWidth && input.Height <= thumbHeight)
                ratio = 1.0f;

            var size = new Size((int)Math.Min(input.Width / ratio, thumbWidth), (int)Math.Min(input.Height / ratio, thumbHeight));
            var pos = new Point(thumbWidth / 2 - size.Width / 2, thumbHeight / 2 - size.Height / 2);

            gfx.InterpolationMode = ratio != 1.0f ? InterpolationMode.HighQualityBicubic : InterpolationMode.Default;
            gfx.DrawImage(input, pos.X, pos.Y, size.Width, size.Height);

            return thumb;
        }

        private void UpdateForm()
        {
            Text = $"{Settings.Default.ApplicationName} v{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion}" + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty) + (_textAdapter != null ? " - " + _textAdapter.Description + " Adapter (" + _textAdapter.Name + ")" : string.Empty);

            var entry = (TextEntry)treEntries.SelectedNode?.Tag;

            tslEntries.Text = _fileOpen ? (_textAdapter.Entries?.Count() + " Entries").Trim() : "Entries";

            if (_textAdapter != null)
            {
                var itemSelected = _fileOpen && treEntries.SelectedNode != null;
                var canAdd = _fileOpen && _textAdapter.CanAddEntries;
                var canRename = itemSelected && _textAdapter.CanRenameEntries && entry.ParentEntry == null;
                var canDelete = itemSelected && _textAdapter.CanDeleteEntries && entry.ParentEntry == null;

                splMain.Enabled = _fileOpen;
                splContent.Enabled = _fileOpen;
                splText.Enabled = _fileOpen;
                splPreview.Enabled = _fileOpen;

                // Menu
                saveToolStripMenuItem.Enabled = _fileOpen && _textAdapter.CanSave;
                tsbSave.Enabled = _fileOpen && _textAdapter.CanSave;
                saveAsToolStripMenuItem.Enabled = _fileOpen && _textAdapter.CanSave;
                tsbSaveAs.Enabled = _fileOpen && _textAdapter.CanSave;
                findToolStripMenuItem.Enabled = _fileOpen;
                tsbFind.Enabled = _fileOpen;
                propertiesToolStripMenuItem.Enabled = _fileOpen && _textAdapter.FileHasExtendedProperties;
                tsbProperties.Enabled = _fileOpen && _textAdapter.FileHasExtendedProperties;
                tslFontFamily.Enabled = _fileOpen;
                scbFontFamily.Enabled = _fileOpen;
                tslFontSize.Enabled = _fileOpen;
                scbFontSize.Enabled = _fileOpen;

                // Toolbar
                addEntryToolStripMenuItem.Enabled = canAdd && treEntries.Focused;
                tsbEntryAdd.Enabled = canAdd && treEntries.Focused;
                renameEntryToolStripMenuItem.Enabled = canRename && treEntries.Focused;
                tsbEntryRename.Enabled = canRename && treEntries.Focused;
                deleteEntryToolStripMenuItem.Enabled = canDelete && treEntries.Focused;
                tsbEntryDelete.Enabled = canDelete && treEntries.Focused;
                entryPropertiesToolStripMenuItem.Enabled = itemSelected && _textAdapter.EntriesHaveExtendedProperties;
                tsbEntryProperties.Enabled = itemSelected && _textAdapter.EntriesHaveExtendedProperties;
                sortEntriesToolStripMenuItem.Enabled = _fileOpen && _textAdapter.CanSortEntries;
                sortEntriesToolStripMenuItem.Image = _textAdapter.SortEntries ? Resources.menu_sorted : Resources.menu_unsorted;
                tsbManageLabels.Enabled = _fileOpen;
                tsbShowTextPreview.Enabled = _fileOpen;
                tsbShowTextPreview.Checked = Settings.Default.ShowTextPreview;
                tsbSortEntries.Enabled = _fileOpen && _textAdapter.CanSortEntries;
                tsbSortEntries.Image = _textAdapter.SortEntries ? Resources.menu_sorted : Resources.menu_unsorted;

                // Preview
                tsbPreviewEnabled.Enabled = _gameHandler?.HandlerCanGeneratePreviews ?? false;
                tsbPreviewEnabled.Image = Settings.Default.PreviewEnabled ? Resources.menu_preview_visible : Resources.menu_preview_invisible;
                tsbPreviewEnabled.Text = Settings.Default.PreviewEnabled ? "Disable Preview" : "Enable Preview";
                tsbPreviewSave.Enabled = Settings.Default.PreviewEnabled && _gameHandler.HandlerCanGeneratePreviews && _gameHandlerPages != null && _gameHandlerPages.Count > 0;
                tsbPreviewCopy.Enabled = tsbPreviewSave.Enabled;

                // Paging
                tsbPreviousPage.Enabled = _gameHandler != null && _gameHandlerPages != null && _gameHandlerPages.Count > 0 && _page > 0;
                tslPage.Enabled = _gameHandler != null && _gameHandlerPages != null && _gameHandlerPages.Count > 0;
                tsbNextPage.Enabled = _gameHandler != null && _gameHandlerPages != null && _gameHandlerPages.Count > 0 && _page < _gameHandlerPages.Count - 1;

                // Handler Settings
                tsbHandlerSettings.Enabled = _gameHandler != null && _gameHandler.HandlerHasSettings;

                treEntries.Enabled = _fileOpen;

                if (itemSelected)
                {
                    txtEdit.Enabled = entry.HasText;
                    if (!entry.HasText && entry.IsSubEntry)
                        txtEdit.Text = "This entry has no text.";
                    else if (!entry.HasText && !entry.IsSubEntry)
                        txtEdit.Text = "Select a child item to edit the text.";
                    txtOriginal.Enabled = entry.HasText && txtOriginal.Text.Trim().Length > 0;
                }
                else
                {
                    txtEdit.Enabled = itemSelected;
                    txtOriginal.Enabled = itemSelected && txtOriginal.Text.Trim().Length > 0;
                }

                tsbGameSelect.Enabled = itemSelected;
            }

            // Lot
            var _lotExists = _lot != null;
            tsbLotCreate.Enabled = _fileOpen && !_lotExists;
            tsbLotCreate.Visible = !_lotExists;
            tsbLotDelete.Enabled = _fileOpen && _lotExists;
            tsbLotDelete.Visible = _lotExists;
            tabLot.Enabled = _lotExists;

            if (_lotExists)
                treEntries.ImageList = imlEntries;
            else
                treEntries.ImageList = null;

            // Kukki
            tsbKukki.Enabled = File.Exists(Path.Combine(Application.StartupPath, "kukkii.exe"));

            // Karameru
            tsbKarameru.Enabled = File.Exists(Path.Combine(Application.StartupPath, "karameru.exe"));
        }

        private string FileName()
        {
            return _textAdapter == null || _textAdapter.FileInfo == null ? string.Empty : _textAdapter.FileInfo.Name;
        }

        // Import/Export
        private void tsbExportKUP_Click(object sender, EventArgs e)
        {

        }

        private void tsbImportKUP_Click(object sender, EventArgs e)
        {

        }

        private void tsbBatchExportKUP_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the source directory containing text files...";
            fbd.SelectedPath = Settings.Default.LastBatchDirectory == string.Empty ? Settings.Default.LastDirectory : Settings.Default.LastBatchDirectory;
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var dr = MessageBox.Show("Search subdirectories?", "", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Cancel) return;
                var browseSubdirectories = dr == DialogResult.Yes;

                string path = fbd.SelectedPath;
                int fileCount = 0;

                if (Directory.Exists(path))
                {
                    var types = _textAdapters.Select(x => x.Extension.ToLower()).Select(y => y.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(z => z).Distinct().ToList();

                    List<string> files = new List<string>();
                    foreach (string type in types)
                        if (type != "*.kup")
                            files.AddRange(Directory.GetFiles(path, type, browseSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

                    // TODO: Ask how to handle overwrites and backups

                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            ITextAdapter currentAdapter = SelectTextAdapter(file, true);

                            try
                            {
                                if (currentAdapter != null)
                                {
                                    KUP kup = new KUP();

                                    currentAdapter.Load(file, true);
                                    foreach (TextEntry entry in currentAdapter.Entries)
                                    {
                                        Entry kEntry = new Entry();
                                        kEntry.Name = entry.Name;
                                        kEntry.EditedText = entry.EditedText;
                                        kEntry.OriginalText = entry.OriginalText;
                                        kEntry.MaxLength = entry.MaxLength;
                                        kup.Entries.Add(kEntry);

                                        if (currentAdapter.EntriesHaveSubEntries)
                                        {
                                            foreach (TextEntry sub in entry.SubEntries)
                                            {
                                                Entry kSub = new Entry();
                                                kSub.Name = sub.Name;
                                                kSub.EditedText = sub.EditedText;
                                                kSub.OriginalText = sub.OriginalText;
                                                kSub.MaxLength = sub.MaxLength;
                                                kSub.ParentEntry = entry;
                                                kEntry.SubEntries.Add(kSub);
                                            }
                                        }
                                    }

                                    kup.Save(fi.FullName + ".kup");
                                    fileCount++;
                                }
                            }
                            catch (Exception) { }
                        }
                    }

                    MessageBox.Show("Batch export completed successfully. " + fileCount + " file(s) succesfully exported.", "Batch Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            Settings.Default.LastBatchDirectory = fbd.SelectedPath;
            Settings.Default.Save();
        }

        private void tsbBatchImportKUP_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the source directory containing text and kup file pairs...";
            fbd.SelectedPath = Settings.Default.LastBatchDirectory == string.Empty ? Settings.Default.LastDirectory : Settings.Default.LastBatchDirectory;
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var dr = MessageBox.Show("Search subdirectories?", "", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Cancel) return;
                var browseSubdirectories = dr == DialogResult.Yes;

                string path = fbd.SelectedPath;
                int fileCount = 0;
                int importCount = 0;

                if (Directory.Exists(path))
                {
                    var types = _textAdapters.Select(x => x.Extension.ToLower()).Select(y => y.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(z => z).Distinct().ToList();

                    List<string> files = new List<string>();
                    foreach (string type in types)
                        if (type != "*.kup")
                            files.AddRange(Directory.GetFiles(path, type, browseSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            ITextAdapter currentAdapter = SelectTextAdapter(file, true);
                            try
                            {
                                if (currentAdapter != null && currentAdapter.CanSave && File.Exists(fi.FullName + ".kup"))
                                {
                                    KUP kup = KUP.Load(fi.FullName + ".kup");

                                    currentAdapter.Load(file, true);
                                    foreach (TextEntry entry in currentAdapter.Entries)
                                    {
                                        Entry kEntry = kup.Entries.Find(o => o.Name == entry.Name);

                                        if (kEntry != null)
                                            entry.EditedText = kEntry.EditedText;

                                        if (currentAdapter.EntriesHaveSubEntries && kEntry != null)
                                        {
                                            foreach (TextEntry sub in entry.SubEntries)
                                            {
                                                Entry kSub = (Entry)kEntry.SubEntries.Find(o => o.Name == sub.Name);

                                                if (kSub != null)
                                                    sub.EditedText = kSub.EditedText;
                                            }
                                        }
                                    }

                                    currentAdapter.Save();
                                    importCount++;
                                }

                                fileCount++;
                            }
                            catch (Exception) { }
                        }
                    }

                    MessageBox.Show("Batch import completed successfully. " + importCount + " of " + fileCount + " files succesfully imported.", "Batch Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            Settings.Default.LastBatchDirectory = fbd.SelectedPath;
            Settings.Default.Save();
        }

        // List
        private void treEntries_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateTextView();
            UpdatePreview();
            UpdateLot();
            UpdateForm();
        }

        private void treEntries_KeyDown(object sender, KeyEventArgs e)
        {
            if (treEntries.Focused && (e.KeyCode == Keys.Enter))
                tsbEntryProperties_Click(sender, e);
        }

        private void treEntries_DoubleClick(object sender, EventArgs e)
        {
            tsbEntryProperties_Click(sender, e);
        }

        private void treEntries_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treEntries.SelectedNode = e.Node;
        }

        private void treEntries_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.Expand();
        }

        private void treEntries_Enter(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void treEntries_Leave(object sender, EventArgs e)
        {
            UpdateForm();
        }

        // Text
        private void txtEdit_KeyUp(object sender, KeyEventArgs e)
        {
            var entry = (TextEntry)treEntries.SelectedNode.Tag;
            var previous = string.Empty;
            var next = string.Empty;

            previous = _gameHandler.GetKuriimuString(entry.EditedText);
            next = txtEdit.Text.Replace("<null>", "\0").Replace("\r\n", _textAdapter.LineEndings);
            entry.EditedText = _gameHandler.GetRawString(next);
            treEntries.SelectedNode.Text = entry + (Settings.Default.ShowTextPreview ? " - " + entry.EditedText : string.Empty);

            if (next != previous)
            {
                _hasChanges = true;
                UpdatePreview();
            }

            UpdateForm();
        }

        private void txtEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.A)
                txtEdit.SelectAll();

            if (e.Control & (e.KeyCode != Keys.V && e.KeyCode != Keys.C && e.KeyCode != Keys.X && e.KeyCode != Keys.Z && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right))
                e.SuppressKeyPress = true;
        }

        private void txtOriginal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.A)
                txtOriginal.SelectAll();
        }

        // Lot
        private void tsbLotCreate_Click(object sender, EventArgs e)
        {
            _lot = new Lot();
            _lot.Populate(_entries);
            _lot.Save(_textAdapter.FileInfo.FullName + ".lot");
            UpdateLabels();
            UpdateForm();
        }

        private void tsbLotDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete the lot for {_textAdapter.FileInfo.Name}?\r\nAll labels, notes, and screenshots will be deleted.", "Confirm Lot Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _lot = null;
            File.Delete(_textAdapter.FileInfo.FullName + ".lot");
            UpdateForm();
        }

        private void txtLotNotes_KeyUp(object sender, KeyEventArgs e)
        {
            TextEntry entry = (TextEntry)treEntries.SelectedNode.Tag;

            var lotEntry = !entry.IsSubEntry ? _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.Name) : _lot.LotEntries.FirstOrDefault(o => o.Entry == entry.ParentEntry.Name).LotSubEntries.FirstOrDefault(o => o.Entry == entry.Name);

            if (txtLotNotes.Text != lotEntry.Notes)
            {
                lotEntry.Notes = txtLotNotes.Text;
                _hasChanges = true;
            }

            UpdateForm();
        }

        private void txtEdit_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
    }
}