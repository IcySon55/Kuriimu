using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Karameru.Properties;
using Kuriimu.Contract;
using System.Drawing;

namespace Karameru
{
    public partial class Manager : Form
    {
        private IArchiveManager _archiveManager = null;
        private bool _fileOpen = false;
        private bool _hasChanges = false;

        private List<IFileAdapter> _fileAdapters = null;
        private List<IImageAdapter> _imageAdapters = null;
        private List<IArchiveManager> _archiveManagers = null;
        private HashSet<string> _fileExtensions = null;
        private HashSet<string> _imageExtensions = null;
        private HashSet<string> _archiveExtensions = null;

        private List<ArchiveFileInfo> _files = null;

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
            imlFiles.Images.Add("tree-binary-file", Resources.tree_binary_file);
            treDirectories.ImageList = imlFiles;
            lstFiles.SmallImageList = imlFiles;

            // Load Plugins
            _fileAdapters = PluginLoader<IFileAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "file*.dll").ToList();
            _imageAdapters = PluginLoader<IImageAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "image*.dll").ToList();
            _archiveManagers = PluginLoader<IArchiveManager>.LoadPlugins(Settings.Default.PluginDirectory, "archive*.dll").ToList();

            _fileExtensions = new HashSet<string>(_fileAdapters.SelectMany(s => s.Extension.Split(';')).Select(o => o.TrimStart('*')));
            _imageExtensions = new HashSet<string>(_imageAdapters.SelectMany(s => s.Extension.Split(';')).Select(o => o.TrimStart('*')));
            _archiveExtensions = new HashSet<string>(_archiveManagers.SelectMany(s => s.Extension.Split(';')).Select(o => o.TrimStart('*')));

            // Load passed in file
            if (args.Length > 0 && File.Exists(args[0]))
                OpenFile(args[0]);
        }

        private void frmManager_Load(object sender, EventArgs e)
        {
            //Icon = Resources.kuriimu;
            Tools.DoubleBuffer(treDirectories, true);
            LoadForm();
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
            //			LoadDirectories();
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

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ConfirmOpenFile(string filename = "")
        {
            DialogResult dr = DialogResult.No;

            if (_fileOpen && _hasChanges)
                dr = MessageBox.Show($"You have unsaved changes in {FileName()}. Save changes before opening another file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

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
            ofd.Filter = Tools.LoadArchiveFilters(_archiveManagers);

            DialogResult dr = DialogResult.OK;

            if (filename == string.Empty)
                dr = ofd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                if (filename == string.Empty)
                    filename = ofd.FileName;

                IArchiveManager _tempManager = SelectArchiveManager(filename);

                try
                {
                    if (_tempManager?.Load(filename) == LoadResult.Success)
                    {
                        _archiveManager?.Unload();
                        _archiveManager = _tempManager;
                        _fileOpen = true;
                        _hasChanges = false;

                        LoadDirectories();
                        UpdateForm();
                    }

                    Settings.Default.LastDirectory = new FileInfo(filename).DirectoryName;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    if (_tempManager != null)
                        MessageBox.Show(this, ex.ToString(), _tempManager.Name + " - " + _tempManager.Description + " Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show(this, ex.ToString(), "Supported Format Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private DialogResult SaveFile(bool saveAs = false)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            DialogResult dr = DialogResult.OK;

            sfd.Title = "Save as " + _archiveManager.Description;
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

            if (dr == DialogResult.OK)
            {
                _archiveManager.Save(saveAs ? _archiveManager.FileInfo.FullName : string.Empty);
                _hasChanges = false;
                UpdateForm();
            }

            return dr;
        }

        private IArchiveManager SelectArchiveManager(string filename, bool batchMode = false)
        {
            IArchiveManager result = null;

            // first look for managers whose extension matches that of our filename
            List<IArchiveManager> matchingManagers = _archiveManagers.Where(manager => manager.Extension.Split(';').Any(s => filename.ToLower().EndsWith(s.Substring(1).ToLower()))).ToList();

            result = matchingManagers.FirstOrDefault(manager => manager.Identify(filename));

            // if none of them match, then try all other managers
            if (result == null)
                result = _archiveManagers.Except(matchingManagers).FirstOrDefault(manager => manager.Identify(filename));

            if (result == null && !batchMode)
                MessageBox.Show("None of the installed plugins are able to open the file.", "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return result == null ? null : (IArchiveManager)Activator.CreateInstance(result.GetType());
        }

        // Loading
        private void LoadForm()
        {

        }

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

            LoadForm();
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
                    if (!imlFiles.Images.ContainsKey(ext))
                    {
                        var shfi = new Win32.SHFILEINFO();
                        try
                        {
                            Win32.SHGetFileInfo(ext, 0, out shfi, Marshal.SizeOf(shfi), Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON | Win32.SHGFI_USEFILEATTRIBUTES);
                            imlFiles.Images.Add(ext, Icon.FromHandle(shfi.hIcon));
                        }
                        finally
                        {
                            if (shfi.hIcon != IntPtr.Zero)
                                Win32.DestroyIcon(shfi.hIcon);
                        }
                    }

                    lstFiles.Items.Add(new ListViewItem(new[] { Path.GetFileName(file.FileName), file.FileSize.ToString() }, ext) { Tag = file });
                }
            }

            lstFiles.EndUpdate();
            
        }

        // Utilities
        private void UpdateForm()
        {
            Text = Settings.Default.ApplicationName + " " + Settings.Default.ApplicationVersion + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty) + (_archiveManager != null ? " - " + _archiveManager.Name + " Manager" : string.Empty);

            tslFileCount.Text = _fileOpen ? $"Files: {_archiveManager.Files?.Count()}" : "";

            openToolStripMenuItem.Enabled = _archiveManagers.Count > 0;
            tsbOpen.Enabled = _archiveManagers.Count > 0;

            if (_archiveManager != null)
            {
                var selectedItem = lstFiles.SelectedItems.Count > 0 ? lstFiles.SelectedItems[0] : null;
                var afi = selectedItem?.Tag as ArchiveFileInfo;
                var application = Applications.None;
                var canReadFileData = false;

                if (selectedItem?.Tag is ArchiveFileInfo)
                {
                    var ext = Path.GetExtension(afi.FileName);

                    if (_fileExtensions.Contains(ext))
                        application = Applications.Kuriimu;
                    else if (_imageExtensions.Contains(ext))
                        application = Applications.Kukkii;
                    else if (_archiveExtensions.Contains(ext))
                        application = Applications.Karameru;

                    canReadFileData = afi.FileSize.HasValue;
                }

                bool nodeSelected = _fileOpen && treDirectories.SelectedNode != null;
                bool itemSelected = _fileOpen && lstFiles.SelectedItems.Count > 0;
                //bool itemIsFile = _fileOpen && itemSelected && treDirectories.SelectedNode.Tag != null;
                bool canEdit = _fileOpen && application != Applications.None && !canReadFileData;
                bool canAdd = _fileOpen && _archiveManager.CanAddFiles && treDirectories.Focused;

                bool canExtractFile = _fileOpen && itemSelected && !canReadFileData;
                bool canRenameFile = itemSelected && _archiveManager.CanRenameFiles && treDirectories.Focused;
                bool canReplaceFile = itemSelected && _archiveManager.CanReplaceFiles && treDirectories.Focused;
                bool canDeleteFile = itemSelected && _archiveManager.CanDeleteFiles && treDirectories.Focused;

                splMain.Enabled = _fileOpen;

                // Menu
                saveToolStripMenuItem.Enabled = _fileOpen && _archiveManager.CanSave;
                tsbSave.Enabled = _fileOpen && _archiveManager.CanSave;
                saveAsToolStripMenuItem.Enabled = _fileOpen && _archiveManager.CanSave;
                tsbSaveAs.Enabled = _fileOpen && _archiveManager.CanSave;
                findToolStripMenuItem.Enabled = _fileOpen;
                tsbFind.Enabled = _fileOpen;
                propertiesToolStripMenuItem.Enabled = _fileOpen && _archiveManager.ArchiveHasExtendedProperties;
                tsbProperties.Enabled = _fileOpen && _archiveManager.ArchiveHasExtendedProperties;

                // Toolbar
                tsbFileEdit.Enabled = canEdit;
                tsbFileExtract.Enabled = canExtractFile;
                tsbFileAdd.Enabled = canAdd;
                tsbFileRename.Enabled = canRenameFile;
                tsbFileReplace.Enabled = canReplaceFile;
                tsbFileDelete.Enabled = canDeleteFile;
                //addFileToolStripMenuItem.Enabled = canAdd && treEntries.Focused;
                //renameFileToolStripMenuItem.Enabled = canRename && treEntries.Focused;
                //deleteFileToolStripMenuItem.Enabled = canDelete && treEntries.Focused;
                //filePropertiesToolStripMenuItem.Enabled = itemSelected && _archiveManager.EntriesHaveExtendedProperties;
                //tsbFileProperties.Enabled = itemSelected && _archiveManager.EntriesHaveExtendedProperties;

                // File Context Menu
                editFileToolStripMenuItem.Enabled = canEdit;
                editFileToolStripMenuItem.Text = canEdit ? "&Edit in " + application : "&Edit";
                extractFileToolStripMenuItem.Enabled = canExtractFile;
                extractFileToolStripMenuItem.Text = canExtractFile ? "E&xtract" : "E&xtract is not supported";
                replaceFileToolStripMenuItem.Enabled = canReplaceFile;

                // Preview
                //tsbPreviewEnabled.Enabled = _gameHandler != null ? _gameHandler.HandlerCanGeneratePreviews : false;
                //tsbPreviewEnabled.Image = Settings.Default.PreviewEnabled ? Resources.menu_preview_visible : Resources.menu_preview_invisible;
                //tsbPreviewEnabled.Text = Settings.Default.PreviewEnabled ? "Disable Preview" : "Enable Preview";
                //tsbPreviewSave.Enabled = Settings.Default.PreviewEnabled && _gameHandler.HandlerCanGeneratePreviews && _gameHandlerPages.Count > 0;
                //tsbPreviewCopy.Enabled = tsbPreviewSave.Enabled;

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

        //private void treEntries_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        //{
        //    LaunchFile();
        //}

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

        private void lstFiles_DoubleClick(object sender, EventArgs e)
        {
            LaunchFile();
        }

        // Toolbar
        private void tsbFileEdit_Click(object sender, EventArgs e)
        {
            LaunchFile();
        }

        private void tsbFileExtract_Click(object sender, EventArgs e)
        {
            ExtractFile();
        }

        private void tsbFileReplace_Click(object sender, EventArgs e)
        {
            ReplaceFile();
        }

        // Context Strip
        private void editFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchFile();
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExtractFile();
        }

        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReplaceFile();
        }

        // Functions
        private void LaunchFile()
        {
            var selectedItem = lstFiles.SelectedItems[0];

            var afi = selectedItem.Tag as ArchiveFileInfo;
            var application = Applications.None;
            var ext = Path.GetExtension(afi.FileName);

            if (_fileExtensions.Contains(ext))
                application = Applications.Kuriimu;
            else if (_imageExtensions.Contains(ext))
                application = Applications.Kukkii;
            else if (_archiveExtensions.Contains(ext))
                application = Applications.Karameru;

            if (application != Applications.None)
            {
                var tempDir = Path.Combine(Application.StartupPath, "temp");
                var fileName = Path.Combine(tempDir, Path.GetFileName(afi.FileName));

                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                using (var fs = File.Create(fileName))
                    afi.FileData.CopyTo(fs);

                ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, application + ".exe"));
                start.WorkingDirectory = Application.StartupPath;
                start.Arguments = fileName;

                Process p = new Process();
                p.StartInfo = start;
                p.Start();
            }
        }

        private void ExtractFile()
        {
            var selectedItem = lstFiles.SelectedItems[0];
            var afi = selectedItem.Tag as ArchiveFileInfo;
            var stream = afi?.FileData;
            var filename = Path.GetFileName(afi?.FileName);

            if (stream == null)
            {
                MessageBox.Show($"Uninitialized file stream. Unable to extract \"{filename}\".", "Extraction Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var extension = Path.GetExtension(afi.FileName).ToLower();
            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = Settings.Default.LastDirectory;
            sfd.FileName = filename;
            sfd.Filter = $"{extension.ToUpper().TrimStart('.')} File (*{extension.ToLower()})|*{extension.ToLower()}";

            if (sfd.ShowDialog() == DialogResult.OK)
                using (var fs = File.Create(sfd.FileName))
                {
                    stream.CopyTo(fs);
                    MessageBox.Show($"\"{filename}\" extracted successfully.", "Extraction Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
        }

        private void ReplaceFile()
        {
            TreeNode selectedNode = treDirectories.SelectedNode;

            if (selectedNode.Tag != null)
            {
                var afi = selectedNode.Tag as ArchiveFileInfo;

                if (afi != null)
                {
                    string fileName = Path.GetFileName(afi.FileName);

                    var ofd = new OpenFileDialog();
                    ofd.Title = $"Select a file to replace {fileName} with...";
                    ofd.InitialDirectory = Settings.Default.LastDirectory;
                    ofd.Filter = "All Files (*.*)|*.*";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string newFilename = Path.GetFileName(ofd.FileName);
                        afi.FileData = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.None);
                        afi.State = ArchiveFileState.Replaced;
                        MessageBox.Show($"{fileName} has been replaced with {newFilename}.", "File Replaced", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
    }
}