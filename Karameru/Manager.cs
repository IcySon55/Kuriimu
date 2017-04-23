using Karameru.Properties;
using Kuriimu.Contract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Karameru
{
    public partial class Manager : Form
    {
        private IArchiveManager _archiveManager = null;
        private bool _fileOpen = false;
        private bool _hasChanges = false;

        private List<ITextAdapter> _textAdapters = null;
        private List<IImageAdapter> _imageAdapters = null;
        private List<IArchiveManager> _archiveManagers = null;
        private HashSet<string> _textExtensions = null;
        private HashSet<string> _imageExtensions = null;
        private HashSet<string> _archiveExtensions = null;

        private List<ArchiveFileInfo> _files = null;

        // Shared file booleans
        private bool _canAddFiles = false;
        private bool _canExtractFiles = false;
        private bool _canRenameFiles = false;
        private bool _canReplaceFiles = false;
        private bool _canDeleteFiles = false;

        public Manager(string[] args)
        {
            InitializeComponent();

            // Overwrite window themes
            Win32.SetWindowTheme(treDirectories.Handle, "explorer", null);
            Win32.SetWindowTheme(lstFiles.Handle, "explorer", null);

            // Populate image list
            imlFiles.Images.Add("tree-directory", Resources.tree_directory);
            imlFiles.Images.Add("tree-directory-open", Resources.tree_directory_open);
            imlFiles.Images.Add("tree-text-file", Resources.tree_text_file);
            imlFiles.Images.Add("tree-image-file", Resources.tree_image_file);
            imlFiles.Images.Add("tree-archive-file", Resources.tree_archive_file);
            imlFilesLarge.Images.Add("tree-directory", Resources.tree_directory_32);
            imlFilesLarge.Images.Add("tree-directory-open", Resources.tree_directory_open);
            imlFilesLarge.Images.Add("tree-text-file", Resources.tree_text_file_32);
            imlFilesLarge.Images.Add("tree-image-file", Resources.tree_image_file_32);
            imlFilesLarge.Images.Add("tree-archive-file", Resources.tree_archive_file_32);
            treDirectories.ImageList = imlFiles;
            lstFiles.SmallImageList = imlFiles;
            lstFiles.LargeImageList = imlFilesLarge;

            // Load Plugins
            _textAdapters = PluginLoader<ITextAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "text*.dll").ToList();
            _imageAdapters = PluginLoader<IImageAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "image*.dll").ToList();
            _archiveManagers = PluginLoader<IArchiveManager>.LoadPlugins(Settings.Default.PluginDirectory, "archive*.dll").ToList();

            _textExtensions = new HashSet<string>(_textAdapters.SelectMany(s => s.Extension.Split(';')).Select(o => o.TrimStart('*')));
            _imageExtensions = new HashSet<string>(_imageAdapters.SelectMany(s => s.Extension.Split(';')).Select(o => o.TrimStart('*')));
            _archiveExtensions = new HashSet<string>(_archiveManagers.SelectMany(s => s.Extension.Split(';')).Select(o => o.TrimStart('*')));

            // Load passed in file
            if (args.Length > 0 && File.Exists(args[0]))
                OpenFile(args[0]);
        }

        private void frmManager_Load(object sender, EventArgs e)
        {
            Icon = Resources.karameru;

            largeToolStripMenuItem.Tag = View.LargeIcon;
            smallToolStripMenuItem.Tag = View.SmallIcon;
            listToolStripMenuItem.Tag = View.List;
            detailsToolStripMenuItem.Tag = View.Details;
            tileToolStripMenuItem.Tag = View.Tile;
            detailsToolStripMenuItem.Checked = true; // Default for now TODO: Make this saved in user settings

            Tools.DoubleBuffer(treDirectories, true);
            Tools.DoubleBuffer(lstFiles, true);
            UpdateForm();
        }

        private void frmManager_FormClosing(object sender, FormClosingEventArgs e)
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
            //frmSearch search = new frmSearch();
            //search.Entries = _entries;
            //search.ShowDialog();

            //if (search.Selected != null)
            //{
            //	treEntries.SelectNodeByIEntry(search.Selected);

            //	if (txtEdit.Text.Contains(Settings.Default.FindWhat))
            //	{
            //		txtEdit.SelectionStart = txtEdit.Text.IndexOf(Settings.Default.FindWhat);
            //		txtEdit.SelectionLength = Settings.Default.FindWhat.Length;
            //		txtEdit.Focus();
            //	}
            //}
        }
        private void tsbFind_Click(object sender, EventArgs e)
        {
            findToolStripMenuItem_Click(sender, e);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (_archiveManager.ShowProperties(Resources.kuriimu))
            //{
            //	_hasChanges = true;
            //	UpdateForm();
            //}
        }
        private void tsbProperties_Click(object sender, EventArgs e)
        {
            propertiesToolStripMenuItem_Click(sender, e);
        }

        private void tsbKuriimu_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, "kuriimu.exe"));
            start.WorkingDirectory = Application.StartupPath;

            Process p = new Process();
            p.StartInfo = start;
            p.Start();
        }

        private void tsbKukkii_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, "kukkii.exe"));
            start.WorkingDirectory = Application.StartupPath;

            Process p = new Process();
            p.StartInfo = start;
            p.Start();
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (treEntries.Focused)
            //{
            //	IEntry entry = _archiveManager.NewEntry();

            //	frmName name = new frmName(entry, _archiveManager.EntriesHaveUniqueNames, _archiveManager.NameList, _archiveManager.NameFilter, _archiveManager.NameMaxLength, true);

            //	if (name.ShowDialog() == DialogResult.OK && name.NameChanged)
            //	{
            //		entry.Name = name.NewName;
            //		if (_archiveManager.AddEntry(entry))
            //		{
            //			_hasChanges = true;
            //			LoadFiles();
            //			treEntries.SelectNodeByIEntry(entry);
            //			UpdateForm();
            //		}
            //	}
            //}
        }
        private void tsbAddFile_Click(object sender, EventArgs e)
        {
            addFileToolStripMenuItem_Click(sender, e);
        }

        //private void renameEntryToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //	if (treEntries.Focused)
        //	{
        //		IEntry entry = (IEntry)treEntries.SelectedNode.Tag;

        //		frmName name = new frmName(entry, _archiveManager.EntriesHaveUniqueNames, _archiveManager.NameList, _archiveManager.NameFilter, _archiveManager.NameMaxLength);

        //		if (name.ShowDialog() == DialogResult.OK && name.NameChanged)
        //		{
        //			if (_archiveManager.RenameEntry(entry, name.NewName))
        //			{
        //				_hasChanges = true;
        //				treEntries.FindNodeByIEntry(entry).Text = name.NewName;
        //				UpdateForm();
        //			}
        //		}
        //	}
        //}
        //private void tsbEntryRename_Click(object sender, EventArgs e)
        //{
        //	renameEntryToolStripMenuItem_Click(sender, e);
        //}

        //private void deleteEntryToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //	if (treEntries.Focused)
        //	{
        //		IEntry entry = (IEntry)treEntries.SelectedNode.Tag;

        //		if (MessageBox.Show("Are you sure you want to delete " + entry.Name + "?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        //		{
        //			if (_archiveManager.DeleteEntry(entry))
        //			{
        //				_hasChanges = true;
        //				TreeNode nextNode = treEntries.SelectedNode.NextNode;
        //				UpdateEntries();
        //				treEntries.Nodes.Remove(treEntries.FindNodeByIEntry(entry));
        //				treEntries.SelectedNode = nextNode;
        //			}
        //		}
        //	}
        //}
        //private void tsbEntryDelete_Click(object sender, EventArgs e)
        //{
        //	deleteEntryToolStripMenuItem_Click(sender, e);
        //}

        //private void entryPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //	IEntry entry = (IEntry)treEntries.SelectedNode.Tag;
        //	if (_archiveManager.ShowEntryProperties(entry, Resources.kuriimu))
        //	{
        //		_hasChanges = true;
        //		UpdateForm();
        //	}
        //}
        //private void tsbEntryProperties_Click(object sender, EventArgs e)
        //{
        //	entryPropertiesToolStripMenuItem_Click(sender, e);
        //}

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
            //frmAbout about = new frmAbout();
            //about.ShowDialog();
        }

        // File Handling
        private void frmManager_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void frmManager_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (files.Length > 0 && File.Exists(files[0]))
                ConfirmOpenFile(files[0]);
        }

        private void ConfirmOpenFile(string fileName = "")
        {
            var dr = DialogResult.No;

            if (_fileOpen && _hasChanges)
                dr = MessageBox.Show($"You have unsaved changes in {FileName()}. Save changes before opening another file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            switch (dr)
            {
                case DialogResult.Yes:
                    dr = SaveFile();
                    if (dr == DialogResult.OK) OpenFile(fileName);
                    break;
                case DialogResult.No:
                    OpenFile(fileName);
                    break;
            }
        }

        private void OpenFile(string fileName = "")
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = Settings.Default.LastDirectory,
                Filter = Tools.LoadArchiveFilters(_archiveManagers)
            };

            var dr = DialogResult.OK;

            if (fileName == string.Empty)
                dr = ofd.ShowDialog();

            if (dr != DialogResult.OK) return;

            if (fileName == string.Empty)
                fileName = ofd.FileName;

            var tempManager = SelectArchiveManager(fileName);

            try
            {
                if (tempManager != null)
                {
                    tempManager.Load(fileName);

                    _archiveManager?.Unload();
                    _archiveManager = tempManager;
                    _fileOpen = true;
                    _hasChanges = false;

                    LoadDirectories();
                    UpdateForm();
                }

                Settings.Default.LastDirectory = new FileInfo(fileName).DirectoryName;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), tempManager != null ? $"{tempManager.Name} - {tempManager.Description} Manager" : "Supported Format Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DialogResult SaveFile(bool saveAs = false)
        {
            var sfd = new SaveFileDialog();
            var dr = DialogResult.OK;

            sfd.Title = $"Save as {_archiveManager.Description}";
            sfd.FileName = _archiveManager.FileInfo.Name;
            sfd.Filter = _archiveManager.Description + " (" + _archiveManager.Extension + ")|" + _archiveManager.Extension;

            if (_archiveManager.FileInfo == null || saveAs)
            {
                sfd.InitialDirectory = Settings.Default.LastDirectory;
                dr = sfd.ShowDialog();
            }

            if ((_archiveManager.FileInfo == null || saveAs) && dr == DialogResult.OK)
            {
                _archiveManager.FileInfo = new FileInfo(sfd.FileName);
                Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
                Settings.Default.Save();
            }

            if (dr != DialogResult.OK) return dr;

            try
            {
                _archiveManager.Save(saveAs ? _archiveManager.FileInfo?.FullName : string.Empty);
                _hasChanges = false;
                UpdateForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), _archiveManager != null ? $"{_archiveManager.Name} - {_archiveManager.Description} Manager" : "Supported Format Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dr;
        }

        private IArchiveManager SelectArchiveManager(string fileName, bool batchMode = false)
        {
            IArchiveManager result = null;

            // first look for managers whose extension matches that of our file name
            List<IArchiveManager> matchingManagers = _archiveManagers.Where(manager => manager.Extension.Split(';').Any(s => fileName.ToLower().EndsWith(s.Substring(1).ToLower()))).ToList();

            result = matchingManagers.FirstOrDefault(manager => manager.Identify(fileName));

            // if none of them match, then try all other managers
            if (result == null)
                result = _archiveManagers.Except(matchingManagers).FirstOrDefault(manager => manager.Identify(fileName));

            if (result == null && !batchMode)
                MessageBox.Show("None of the installed plugins are able to open the file.", "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return result == null ? null : (IArchiveManager)Activator.CreateInstance(result.GetType());
        }

        // Loading
        private void UpdateFiles()
        {
            _files = _archiveManager.Files.ToList();
        }

        private void LoadDirectories()
        {
            UpdateFiles();

            treDirectories.BeginUpdate();
            treDirectories.Nodes.Clear();

            var lookup = _files.OrderBy(f => f.FileName).ToLookup(f => Path.GetDirectoryName(f.FileName));

            // Build directory tree
            var root = treDirectories.Nodes.Add("root", FileName(), "tree-archive-file", "tree-archive-file");
            foreach (var dir in lookup.Select(g => g.Key))
            {
                dir.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries)
                    .Aggregate(root, (node, part) => node.Nodes[part] ?? node.Nodes.Add(part, part))
                    .Tag = lookup[dir];
            }

            root.Expand();
            treDirectories.SelectedNode = root;
            treDirectories.EndUpdate();

            treDirectories.Focus();
        }

        private void LoadFiles()
        {
            lstFiles.BeginUpdate();
            lstFiles.Items.Clear();

            if (treDirectories.SelectedNode.Tag is IEnumerable<ArchiveFileInfo> files)
            {
                foreach (var file in files)
                {
                    // Get the items from the file system, and add each of them to the ListView,
                    // complete with their corresponding name and icon indices.
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    var kuriimuFile = ext.Length > 0 && _textExtensions.Contains(ext);
                    var kukkiiFile = ext.Length > 0 && _imageExtensions.Contains(ext);
                    var karameruFile = ext.Length > 0 && _archiveExtensions.Contains(ext);

                    var shfi = new Win32.SHFILEINFO();
                    try
                    {
                        if (!imlFiles.Images.ContainsKey(ext))
                        {
                            Win32.SHGetFileInfo(ext, 0, out shfi, Marshal.SizeOf(shfi), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON | Win32.SHGFI_USEFILEATTRIBUTES);
                            imlFiles.Images.Add(ext, Icon.FromHandle(shfi.hIcon));
                        }
                    }
                    finally
                    {
                        if (shfi.hIcon != IntPtr.Zero)
                            Win32.DestroyIcon(shfi.hIcon);
                    }
                    try
                    {
                        if (!imlFilesLarge.Images.ContainsKey(ext))
                        {
                            Win32.SHGetFileInfo(ext, 0, out shfi, Marshal.SizeOf(shfi), Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON | Win32.SHGFI_USEFILEATTRIBUTES);
                            imlFilesLarge.Images.Add(ext, Icon.FromHandle(shfi.hIcon));
                        }
                    }
                    finally
                    {
                        if (shfi.hIcon != IntPtr.Zero)
                            Win32.DestroyIcon(shfi.hIcon);
                    }

                    if (kuriimuFile) ext = "tree-text-file";
                    if (kukkiiFile) ext = "tree-image-file";
                    if (karameruFile) ext = "tree-archive-file";

                    lstFiles.Items.Add(new ListViewItem(new[] { Path.GetFileName(file.FileName), file.FileSize.ToString() }, ext, StateToColor(file.State), Color.Transparent, lstFiles.Font) { Tag = file });
                }

                tslFileCount.Text = $"Files: {files.Count()}";
            }

            lstFiles.EndUpdate();
        }

        private Color StateToColor(ArchiveFileState state)
        {
            Color result = Color.Black;

            switch (state)
            {
                case ArchiveFileState.Empty:
                    result = Color.DarkGray;
                    break;
                case ArchiveFileState.Added:
                    result = Color.Green;
                    break;
                case ArchiveFileState.Replaced:
                    result = Color.Orange;
                    break;
                case ArchiveFileState.Renamed:
                    result = Color.Blue;
                    break;
                case ArchiveFileState.Deleted:
                    result = Color.Red;
                    break;
            }

            return result;
        }

        // Utilities
        private void UpdateForm()
        {
            Text = Settings.Default.ApplicationName + " " + Settings.Default.ApplicationVersion + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty) + (_archiveManager != null ? " - " + _archiveManager.Name + " Manager" : string.Empty);

            openToolStripMenuItem.Enabled = _archiveManagers.Count > 0;
            tsbOpen.Enabled = _archiveManagers.Count > 0;

            if (_archiveManager != null)
            {
                var selectedItem = lstFiles.SelectedItems.Count > 0 ? lstFiles.SelectedItems[0] : null;
                var afi = selectedItem?.Tag as ArchiveFileInfo;

                bool nodeSelected = _fileOpen && treDirectories.SelectedNode != null;

                bool itemSelected = _fileOpen && lstFiles.SelectedItems.Count > 0;

                _canAddFiles = _fileOpen && _archiveManager.CanAddFiles;
                _canExtractFiles = itemSelected && afi.FileSize.HasValue;
                _canReplaceFiles = itemSelected && _archiveManager.CanReplaceFiles;
                _canRenameFiles = itemSelected && _archiveManager.CanRenameFiles;
                _canDeleteFiles = itemSelected && _archiveManager.CanDeleteFiles;

                splMain.Enabled = _fileOpen;

                // Menu
                saveToolStripMenuItem.Enabled = _fileOpen && _archiveManager.CanSave;
                tsbSave.Enabled = _fileOpen && _archiveManager.CanSave;
                saveAsToolStripMenuItem.Enabled = _fileOpen && _archiveManager.CanSave;
                tsbSaveAs.Enabled = _fileOpen && _archiveManager.CanSave;
                //findToolStripMenuItem.Enabled = _fileOpen;
                //tsbFind.Enabled = _fileOpen;
                propertiesToolStripMenuItem.Enabled = _fileOpen && _archiveManager.ArchiveHasExtendedProperties;
                tsbProperties.Enabled = _fileOpen && _archiveManager.ArchiveHasExtendedProperties;

                // Toolbar
                tsbFileAdd.Enabled = _canAddFiles;
                tsbFileExtract.Enabled = _canExtractFiles;
                tsbFileReplace.Enabled = _canReplaceFiles;
                tsbFileRename.Enabled = _canRenameFiles;
                tsbFileDelete.Enabled = _canDeleteFiles;
                //addFileToolStripMenuItem.Enabled = canAdd && treEntries.Focused;
                //renameFileToolStripMenuItem.Enabled = canRename && treEntries.Focused;
                //deleteFileToolStripMenuItem.Enabled = canDelete && treEntries.Focused;
                //filePropertiesToolStripMenuItem.Enabled = itemSelected && _archiveManager.EntriesHaveExtendedProperties;
                //tsbFileProperties.Enabled = itemSelected && _archiveManager.EntriesHaveExtendedProperties;

                treDirectories.Enabled = _fileOpen;
            }

            // Shortcuts
            tsbKuriimu.Enabled = File.Exists(Path.Combine(Application.StartupPath, "kuriimu.exe"));
            tsbKukkii.Enabled = File.Exists(Path.Combine(Application.StartupPath, "kukkii.exe"));
        }

        private string FileName()
        {
            return _archiveManager?.FileInfo?.Name ?? string.Empty;
        }

        // Directory Tree
        private void treEntries_AfterSelect(object sender, TreeViewEventArgs e)
        {
            LoadFiles();
            UpdateForm();
        }

        private void treEntries_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                e.Node.ImageKey = "tree-directory-open";
                e.Node.SelectedImageKey = e.Node.ImageKey;
            }
        }

        private void treEntries_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                e.Node.ImageKey = "tree-directory";
                e.Node.SelectedImageKey = e.Node.ImageKey;
            }
        }

        // File List
        private void lstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateForm();
        }

        // Toolbar
        private void tsbFileExtract_Click(object sender, EventArgs e)
        {
            var selectedItem = lstFiles.SelectedItems.Count > 0 ? lstFiles.SelectedItems[0] : null;
            var afi = selectedItem?.Tag as ArchiveFileInfo;
            ExtractFiles(new List<ArchiveFileInfo>() { afi });
        }

        private void tsbFileReplace_Click(object sender, EventArgs e)
        {
            var selectedItem = lstFiles.SelectedItems.Count > 0 ? lstFiles.SelectedItems[0] : null;
            var afi = selectedItem?.Tag as ArchiveFileInfo;
            ReplaceFile(afi);
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            foreach (ToolStripMenuItem mnuItem in viewToolStripDropDownButton.DropDownItems)
                mnuItem.Checked = false;
            menuItem.Checked = true;
            lstFiles.View = (View)menuItem.Tag;
        }

        // Context Strip
        private void mnuFiles_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var selectedItem = lstFiles.SelectedItems.Count > 0 ? lstFiles.SelectedItems[0] : null;
            var afi = selectedItem?.Tag as ArchiveFileInfo;
            var ext = Path.GetExtension(afi?.FileName).ToLower();

            extractFileToolStripMenuItem.Enabled = _canExtractFiles;
            extractFileToolStripMenuItem.Text = _canExtractFiles ? "E&xtract..." : "Extract is not supported";
            extractFileToolStripMenuItem.Tag = afi;

            replaceFileToolStripMenuItem.Enabled = _canReplaceFiles;
            replaceFileToolStripMenuItem.Text = _canReplaceFiles ? "&Replace..." : "Replace is not supported";
            replaceFileToolStripMenuItem.Tag = afi;

            renameFileToolStripMenuItem.Enabled = _canRenameFiles;
            renameFileToolStripMenuItem.Text = _canRenameFiles ? "Re&name..." : "Rename is not supported";
            renameFileToolStripMenuItem.Tag = afi;

            deleteFileToolStripMenuItem.Enabled = _canDeleteFiles;
            deleteFileToolStripMenuItem.Text = _canDeleteFiles ? "&Delete" : "Delete is not supported";
            deleteFileToolStripMenuItem.Tag = afi;

            // Generate supported application menu items
            var kuriimuVisible = ext.Length > 0 && _textExtensions.Contains(ext);
            var kukkiiVisible = ext.Length > 0 && _imageExtensions.Contains(ext);
            var karameruVisible = ext.Length > 0 && _archiveExtensions.Contains(ext);

            editInKuriimuToolStripMenuItem.Tag = new List<object> { afi, Applications.Kuriimu };
            editInKukkiiToolStripMenuItem.Tag = new List<object> { afi, Applications.Kukkii };
            editInKarameruToolStripMenuItem.Tag = new List<object> { afi, Applications.Karameru };

            editInKuriimuToolStripMenuItem.Visible = kuriimuVisible;
            editInKukkiiToolStripMenuItem.Visible = kukkiiVisible;
            editInKarameruToolStripMenuItem.Visible = karameruVisible;

            editListToolStripSeparator.Visible = kuriimuVisible || kukkiiVisible || karameruVisible;
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            // TODO: Implement multi-selection of files
            var afi = menuItem.Tag as ArchiveFileInfo;
            var files = new List<ArchiveFileInfo>() { afi };
            ExtractFiles(files);
        }

        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            var afi = menuItem.Tag as ArchiveFileInfo;
            ReplaceFile(afi);
        }

        private void editFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            var afi = ((List<object>)menuItem.Tag)[0] as ArchiveFileInfo;
            Applications application = (Applications)((List<object>)menuItem.Tag)[1];
            LaunchFile(afi, application);
        }

        // Functions
        private void LaunchFile(ArchiveFileInfo afi, Applications application)
        {
            var stream = afi?.FileData;
            var fileName = Path.GetFileName(afi?.FileName);

            if (stream == null)
            {
                MessageBox.Show($"Uninitialized file stream. Unable to extract \"{fileName}\".", "Extraction Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (application == Applications.None) return;
            var tempDir = Path.Combine(Application.StartupPath, "temp");
            var outputFileName = Path.Combine(tempDir, fileName);

            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
            using (var fs = File.Create(outputFileName))
            {
                if (stream.CanSeek)
                    stream.Position = 0;

                try
                {
                    if (afi.FileSize > 0)
                        stream.CopyTo(fs);

                    ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, application + ".exe"));
                    start.WorkingDirectory = Application.StartupPath;
                    start.Arguments = outputFileName;

                    Process p = new Process();
                    p.StartInfo = start;
                    p.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Edit Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExtractFiles(List<ArchiveFileInfo> files)
        {
            if (files?.Count > 1)
            {
                MessageBox.Show("No soup for you. :(", "Multi-file Extraction Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (files?.Count == 1)
            {
                var afi = files.First();
                var stream = afi?.FileData;
                var fileName = Path.GetFileName(afi?.FileName);

                if (stream == null)
                {
                    MessageBox.Show($"Uninitialized file stream. Unable to extract \"{fileName}\".", "Extraction Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var extension = Path.GetExtension(fileName).ToLower();
                var sfd = new SaveFileDialog();
                sfd.InitialDirectory = Settings.Default.LastDirectory;
                sfd.FileName = fileName;
                sfd.Filter = $"{extension.ToUpper().TrimStart('.')} File (*{extension})|*{extension}";

                if (sfd.ShowDialog() != DialogResult.OK) return;
                using (var fs = File.Create(sfd.FileName))
                {
                    if (stream.CanSeek)
                        stream.Position = 0;

                    try
                    {
                        if (afi.FileSize > 0)
                            stream.CopyTo(fs);

                        MessageBox.Show($"\"{fileName}\" extracted successfully.", "Extraction Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Extraction Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ReplaceFile(ArchiveFileInfo afi)
        {
            if (afi == null) return;
            var fileName = Path.GetFileName(afi.FileName);

            var ofd = new OpenFileDialog();
            ofd.Title = $"Select a file to replace {fileName} with...";
            ofd.InitialDirectory = Settings.Default.LastDirectory;

            // TODO: Implement file type filtering if replacement filetype matters
            ofd.Filter = "All Files (*.*)|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) return;
            try
            {
                afi.FileData = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.None);
                afi.State = ArchiveFileState.Replaced;
                lstFiles.SelectedItems[0].ForeColor = StateToColor(afi.State);
                MessageBox.Show($"{fileName} has been replaced with {Path.GetFileName(ofd.FileName)}.", "File Replaced", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Replace Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            _hasChanges = true;
            UpdateForm();
        }
    }
}