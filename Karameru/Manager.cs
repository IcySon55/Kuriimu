using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Karameru.Properties;
using KuriimuContract;

namespace Karameru
{
	public partial class frmManager : Form
	{
		private IArchiveManager _archiveManager = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		private List<IArchiveManager> _archiveManagers = null;

		private IEnumerable<ArchiveFileInfo> _files = null;

		public frmManager(string[] args)
		{
			InitializeComponent();

			// Load Plugins
			_archiveManagers = PluginLoader<IArchiveManager>.LoadPlugins(Settings.Default.PluginDirectory, "archive*.dll").ToList();

			// Load passed in file
			if (args.Length > 0 && File.Exists(args[0]))
				OpenFile(args[0]);
		}

		private void frmManager_Load(object sender, EventArgs e)
		{
			//Icon = Resources.kuriimu;
			Tools.DoubleBuffer(treEntries, true);
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

		private void addEntryToolStripMenuItem_Click(object sender, EventArgs e)
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
		private void tsbEntryAdd_Click(object sender, EventArgs e)
		{
			addEntryToolStripMenuItem_Click(sender, e);
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

		// UI Toolbars
		private void tsbPreviewEnabled_Click(object sender, EventArgs e)
		{
			//Settings.Default.PreviewEnabled = !Settings.Default.PreviewEnabled;
			Settings.Default.Save();
			UpdatePreview();
			UpdateForm();
		}

		private void tsbPreviewSave_Click(object sender, EventArgs e)
		{
			//SaveFileDialog sfd = new SaveFileDialog();
			//sfd.Title = "Save Preview as PNG...";
			//sfd.InitialDirectory = Settings.Default.LastDirectory;
			//sfd.FileName = "preview.png";
			//sfd.Filter = "Portable Network Graphics (*.png)|*.png";
			//sfd.AddExtension = true;

			//if (sfd.ShowDialog() == DialogResult.OK)
			//{
			//	Bitmap bmp = (Bitmap)pbxPreview.Image;
			//	bmp.Save(sfd.FileName, ImageFormat.Png);

			//	Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
			//	Settings.Default.Save();
			//}
		}

		private void tsbPreviewCopy_Click(object sender, EventArgs e)
		{
			//Clipboard.SetImage(pbxPreview.Image);
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
			ofd.Filter = Tools.LoadArchiveFilters(_archiveManagers);

			DialogResult dr = DialogResult.OK;

			if (filename == string.Empty)
				dr = ofd.ShowDialog();

			if (dr == DialogResult.OK)
			{
				if (filename == string.Empty)
					filename = ofd.FileName;

				IArchiveManager _tempAdapter = SelectArchiveManager(filename);

				try
				{
					if (_tempAdapter != null && _tempAdapter.Load(filename) == LoadResult.Success)
					{
						_archiveManager = _tempAdapter;
						_fileOpen = true;
						_hasChanges = false;

						LoadFiles();
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
				_archiveManager.Save(_archiveManager.FileInfo.FullName);
				_hasChanges = false;
				UpdateForm();
			}

			return dr;
		}

		private IArchiveManager SelectArchiveManager(string filename, bool batchMode = false)
		{
			IArchiveManager result = null;

			// first look for adapters whose extension matches that of our filename
			List<IArchiveManager> matchingAdapters = _archiveManagers.Where(adapter => adapter.Extension.Split(';').Any(s => filename.ToLower().EndsWith(s.Substring(1).ToLower()))).ToList();

			result = matchingAdapters.FirstOrDefault(adapter => adapter.Identify(filename));

			// if none of them match, then try all other adapters
			if (result == null)
				result = _archiveManagers.Except(matchingAdapters).FirstOrDefault(adapter => adapter.Identify(filename));

			if (result == null && !batchMode)
				MessageBox.Show("None of the installed plugins are able to open the file.", "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Information);

			return result;
		}

		// Loading
		private void LoadForm()
		{

		}

		private void LoadFiles()
		{
			UpdateFiles();

			treEntries.BeginUpdate();

			ArchiveFileInfo selectedFile = null;
			if (treEntries.SelectedNode != null)
				selectedFile = (ArchiveFileInfo)treEntries.SelectedNode.Tag;

			treEntries.Nodes.Clear();
			if (_files != null)
			{
				// Build directory tree
				foreach (ArchiveFileInfo file in _files)
				{
					// TODO: This part is not as easy as I'd like it to be
				}

				foreach (ArchiveFileInfo file in _files)
				{
					TreeNode node = new TreeNode(file.Filename);
					node.Tag = file;
					treEntries.Nodes.Add(node);

					//node.Expand();
				}
			}

			//if ((selectedEntry == null || !_files.Contains(selectedEntry)) && treEntries.Nodes.Count > 0)
			//	treEntries.SelectedNode = treEntries.Nodes[0];
			//else
			//	treEntries.SelectNodeByIEntry(selectedEntry);

			treEntries.EndUpdate();

			treEntries.Focus();
		}

		private void UpdateFiles()
		{
			_files = _archiveManager.Files;

			//if (_archiveManager.SortEntries)
			//	_files = _files.OrderBy(x => x);
		}

		// Utilities
		private void UpdatePreview()
		{
			//IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;
			//_gameHandlerPages = _gameHandler.GeneratePreviews(entry);
			//SetPage(0);

			//if (entry != null && _gameHandler.HandlerCanGeneratePreviews && Settings.Default.PreviewEnabled && _gameHandlerPages.Count > 0)
			//	pbxPreview.Image = _gameHandlerPages[_page];
			//else
			//	pbxPreview.Image = null;
		}

		private void UpdateForm()
		{
			Text = Settings.Default.ApplicationName + " " + Settings.Default.ApplicationVersion + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty) + (_archiveManager != null ? " - " + _archiveManager.Name + " Adapter" : string.Empty);

			if (_fileOpen)
				tslFiles.Text = (_archiveManager.Files?.Count() + " Files").Trim();
			else
				tslFiles.Text = "Files";

			if (_archiveManager != null)
			{
				bool itemSelected = _fileOpen && treEntries.SelectedNode != null;
				bool canAdd = _fileOpen && _archiveManager.CanAddFiles;
				bool canRename = itemSelected && _archiveManager.CanRenameFiles;
				bool canDelete = itemSelected && _archiveManager.CanDeleteFiles;

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
				//addEntryToolStripMenuItem.Enabled = canAdd && treEntries.Focused;
				tsbEntryAdd.Enabled = canAdd && treEntries.Focused;
				//renameEntryToolStripMenuItem.Enabled = canRename && treEntries.Focused;
				tsbEntryRename.Enabled = canRename && treEntries.Focused;
				//deleteEntryToolStripMenuItem.Enabled = canDelete && treEntries.Focused;
				tsbEntryDelete.Enabled = canDelete && treEntries.Focused;
				//entryPropertiesToolStripMenuItem.Enabled = itemSelected && _archiveManager.EntriesHaveExtendedProperties;
				//tsbEntryProperties.Enabled = itemSelected && _archiveManager.EntriesHaveExtendedProperties;

				// Preview
				//tsbPreviewEnabled.Enabled = _gameHandler != null ? _gameHandler.HandlerCanGeneratePreviews : false;
				//tsbPreviewEnabled.Image = Settings.Default.PreviewEnabled ? Resources.menu_preview_visible : Resources.menu_preview_invisible;
				//tsbPreviewEnabled.Text = Settings.Default.PreviewEnabled ? "Disable Preview" : "Enable Preview";
				//tsbPreviewSave.Enabled = Settings.Default.PreviewEnabled && _gameHandler.HandlerCanGeneratePreviews && _gameHandlerPages.Count > 0;
				//tsbPreviewCopy.Enabled = tsbPreviewSave.Enabled;

				treEntries.Enabled = _fileOpen;
			}

			// Shortcuts
			tsbKuriimu.Enabled = File.Exists(Path.Combine(Application.StartupPath, "kuriimu.exe"));
			tsbKukkii.Enabled = File.Exists(Path.Combine(Application.StartupPath, "kukkii.exe"));
		}

		private string FileName()
		{
			return _archiveManager == null || _archiveManager.FileInfo == null ? string.Empty : _archiveManager.FileInfo.Name;
		}

		// List
		private void treEntries_AfterSelect(object sender, TreeViewEventArgs e)
		{
			UpdatePreview();
			UpdateForm();
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
	}
}