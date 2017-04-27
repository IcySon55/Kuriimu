using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Contract;
using Kuriimu.Properties;

namespace Kuriimu
{
    public partial class Editor : Form
    {
        private ITextAdapter _textAdapter = null;
        private IGameHandler _gameHandler = null;
        private IList<Bitmap> _gameHandlerPages = new List<Bitmap>();
        private bool _fileOpen = false;
        private bool _hasChanges = false;

        private List<ITextAdapter> _textAdapters = null;
        private List<IGameHandler> _gameHandlers = null;
        private List<IExtension> _extensions = null;

        private List<TextEntry> _entries = null;

        private int _page = 0;

        public Editor(string[] args)
        {
            InitializeComponent();

            // Load Plugins
            _textAdapters = PluginLoader<ITextAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "text*.dll").ToList();
            _gameHandlers = Tools.LoadGameHandlers(Settings.Default.PluginDirectory, tsbGameSelect, Resources.game_none, tsbGameSelect_SelectedIndexChanged);
            _extensions = PluginLoader<IExtension>.LoadPlugins(Settings.Default.PluginDirectory, "ext*.dll").ToList();

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

                Name name = new Name(entry, _textAdapter.EntriesHaveUniqueNames, _textAdapter.NameList, _textAdapter.NameFilter, _textAdapter.NameMaxLength, true);

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

                Name name = new Name(entry, _textAdapter.EntriesHaveUniqueNames, _textAdapter.NameList, _textAdapter.NameFilter, _textAdapter.NameMaxLength);

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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Settings.Default.LastDirectory;

            // Supported Types
            ofd.Filter = Tools.LoadFileFilters(_textAdapters);

            DialogResult dr = DialogResult.OK;

            if (filename == string.Empty)
                dr = ofd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                if (filename == string.Empty)
                    filename = ofd.FileName;

                ITextAdapter _tempAdapter = SelectTextAdapter(filename);

                try
                {
                    if (_tempAdapter != null && _tempAdapter.Load(filename) == LoadResult.Success)
                    {
                        _textAdapter = _tempAdapter;
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
                        UpdateTextView();
                        UpdatePreview();
                        UpdateForm();
                    }

                    Settings.Default.LastDirectory = new FileInfo(filename).DirectoryName;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    if (_tempAdapter != null)
                        MessageBox.Show(this, ex.ToString(), _tempAdapter.Name + " - " + _tempAdapter.Description + " Adapter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show(this, ex.ToString(), "Supported Format Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private DialogResult SaveFile(bool saveAs = false)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            DialogResult dr = DialogResult.OK;

            sfd.Title = "Save as " + _textAdapter.Description;
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

            if (dr == DialogResult.OK)
            {
                _textAdapter.Save(_textAdapter.FileInfo.FullName);
                _hasChanges = false;
                UpdateForm();
            }

            return dr;
        }

        private ITextAdapter SelectTextAdapter(string filename, bool batchMode = false)
        {
            ITextAdapter result = null;

            // first look for adapters whose extension matches that of our filename
            List<ITextAdapter> matchingAdapters = _textAdapters.Where(adapter => adapter.Extension.Split(';').Any(s => filename.ToLower().EndsWith(s.Substring(1).ToLower()))).ToList();

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
            string[] fontSizes = new string[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "30", "36", "48", "72" };
            FontFamily[] fontFamilies = new InstalledFontCollection().Families;

            scbFontFamily.SelectedIndexChanged -= new EventHandler(scbFontFamily_SelectedIndexChanged);
            scbFontFamily.TextChanged -= new EventHandler(scbFontFamily_TextChanged);

            scbFontFamily.Items.Clear();
            scbFontFamily.Items.AddRange(fontFamilies.Select(o => o.Name).ToArray());
            scbFontFamily.Text = Settings.Default.FontFamily;

            scbFontFamily.SelectedIndexChanged += new EventHandler(scbFontFamily_SelectedIndexChanged);
            scbFontFamily.TextChanged += new EventHandler(scbFontFamily_TextChanged);

            scbFontSize.SelectedIndexChanged -= new EventHandler(scbFontSize_SelectedIndexChanged);
            scbFontSize.TextChanged -= new EventHandler(scbFontSize_TextChanged);

            scbFontSize.Items.Clear();
            scbFontSize.Items.AddRange(fontSizes);
            scbFontSize.Text = Settings.Default.FontSize;

            scbFontSize.SelectedIndexChanged += new EventHandler(scbFontSize_SelectedIndexChanged);
            scbFontSize.TextChanged += new EventHandler(scbFontSize_TextChanged);

            SetFont();

            // Tools
            UI.CompressionTools.LoadCompressionTools(compressionToolStripMenuItem);

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
            else if (_page > _gameHandlerPages.Count - 1)
                _page = _gameHandlerPages.Count - 1;

            if (_gameHandlerPages.Count > 0)
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
                foreach (TextEntry entry in _entries)
                {
                    TreeNode node = new TreeNode(entry.ToString());
                    node.Tag = entry;
                    if (!entry.HasText)
                    {
                        node.ForeColor = System.Drawing.Color.Gray;
                    }
                    treEntries.Nodes.Add(node);

                    if (_textAdapter.EntriesHaveSubEntries)
                        foreach (TextEntry sub in entry.SubEntries)
                        {
                            TreeNode subNode = new TreeNode(sub.ToString());
                            subNode.Tag = sub;
                            node.Nodes.Add(subNode);
                        }

                    node.Expand();
                }
            }

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

            if (entry == null)
            {
                txtEdit.Text = string.Empty;
                txtOriginal.Text = string.Empty;
            }
            else
            {
                txtEdit.Text = _gameHandler.GetKuriimuString(entry.EditedText).Replace("\0", "<null>").Replace(_textAdapter.LineEndings, "\r\n");
                txtOriginal.Text = _gameHandler.GetKuriimuString(entry.OriginalText).Replace("\0", "<null>").Replace(_textAdapter.LineEndings, "\r\n");
            }

            if (entry != null && entry.MaxLength != 0)
                txtEdit.MaxLength = entry.MaxLength == 0 ? int.MaxValue : entry.MaxLength;
        }

        private void UpdatePreview()
        {
            TextEntry entry = (TextEntry)treEntries.SelectedNode?.Tag;
            _gameHandlerPages = _gameHandler.GeneratePreviews(entry);
            SetPage(0);

            if (entry != null && _gameHandler.HandlerCanGeneratePreviews && Settings.Default.PreviewEnabled && _gameHandlerPages.Count > 0)
                pbxPreview.Image = _gameHandlerPages[_page];
            else
                pbxPreview.Image = null;
        }

        private void UpdateForm()
        {
            Text = Settings.Default.ApplicationName + " " + Settings.Default.ApplicationVersion + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty) + (_textAdapter != null ? " - " + _textAdapter.Name + " Adapter" : string.Empty);

            TextEntry entry = (TextEntry)treEntries.SelectedNode?.Tag;

            if (_fileOpen)
                tslEntries.Text = (_textAdapter.Entries?.Count() + " Entries").Trim();
            else
                tslEntries.Text = "Entries";

            if (_textAdapter != null)
            {
                bool itemSelected = _fileOpen && treEntries.SelectedNode != null;
                bool canAdd = _fileOpen && _textAdapter.CanAddEntries;
                bool canRename = itemSelected && _textAdapter.CanRenameEntries && entry.ParentEntry == null;
                bool canDelete = itemSelected && _textAdapter.CanDeleteEntries && entry.ParentEntry == null;

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
                tsbSortEntries.Enabled = _fileOpen && _textAdapter.CanSortEntries;
                tsbSortEntries.Image = _textAdapter.SortEntries ? Resources.menu_sorted : Resources.menu_unsorted;

                // Preview
                tsbPreviewEnabled.Enabled = _gameHandler != null ? _gameHandler.HandlerCanGeneratePreviews : false;
                tsbPreviewEnabled.Image = Settings.Default.PreviewEnabled ? Resources.menu_preview_visible : Resources.menu_preview_invisible;
                tsbPreviewEnabled.Text = Settings.Default.PreviewEnabled ? "Disable Preview" : "Enable Preview";
                tsbPreviewSave.Enabled = Settings.Default.PreviewEnabled && _gameHandler.HandlerCanGeneratePreviews && _gameHandlerPages.Count > 0;
                tsbPreviewCopy.Enabled = tsbPreviewSave.Enabled;

                // Paging
                tsbPreviousPage.Enabled = _gameHandler != null && _gameHandlerPages.Count > 0 && _page > 0;
                tslPage.Enabled = _gameHandler != null && _gameHandlerPages.Count > 0;
                tsbNextPage.Enabled = _gameHandler != null && _gameHandlerPages.Count > 0 && _page < _gameHandlerPages.Count - 1;

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

            // Kukki
            tsbKukki.Enabled = File.Exists(Path.Combine(Application.StartupPath, "kukkii.exe"));
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
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Import KUP...";
            ofd.InitialDirectory = Settings.Default.LastDirectory;
            ofd.Filter = "Kuriimu Archive (*.kup)|*.kup";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //var bmp = (Bitmap)Image.FromFile(filename);
                    //_imageAdapter.Bitmap = bmp;
                    UpdatePreview();
                    //MessageBox.Show(filename + " imported successfully.", "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tsbBatchExportKUP_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the source directory containing text files...";
            fbd.SelectedPath = Settings.Default.LastBatchDirectory == string.Empty ? Settings.Default.LastDirectory : Settings.Default.LastBatchDirectory;
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                int fileCount = 0;

                if (Directory.Exists(path))
                {
                    string[] types = _textAdapters.Select(x => x.Extension.ToLower()).ToArray();

                    List<string> files = new List<string>();
                    foreach (string type in types)
                    {
                        if (type != "*.kup")
                        {
                            string[] subTypes = type.Split(';');
                            foreach (string subType in subTypes)
                                files.AddRange(Directory.GetFiles(path, subType, SearchOption.AllDirectories));
                        }
                    }

                    // TODO: Ask how to handle overwrites and backups

                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            ITextAdapter currentAdapter = SelectTextAdapter(file, true);

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
                                            entry.SubEntries.Add(kSub);
                                        }
                                    }
                                }

                                kup.Save(fi.FullName + ".kup");
                                fileCount++;
                            }
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
                string path = fbd.SelectedPath;
                int fileCount = 0;
                int importCount = 0;

                if (Directory.Exists(path))
                {
                    string[] types = _textAdapters.Select(x => x.Extension.ToLower()).ToArray();

                    List<string> files = new List<string>();
                    foreach (string type in types)
                    {
                        if (type != "*.kup")
                        {
                            string[] subTypes = type.Split(';');
                            foreach (string subType in subTypes)
                                files.AddRange(Directory.GetFiles(path, subType, SearchOption.AllDirectories));
                        }
                    }

                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            ITextAdapter currentAdapter = SelectTextAdapter(file, true);

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
            TextEntry entry = (TextEntry)treEntries.SelectedNode.Tag;
            string next = string.Empty;
            string previous = string.Empty;

            previous = _gameHandler.GetKuriimuString(entry.EditedText);
            next = txtEdit.Text.Replace("<null>", "\0").Replace("\r\n", _textAdapter.LineEndings);
            entry.EditedText = _gameHandler.GetRawString(next);

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
        }
    }
}