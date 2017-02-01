using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kukkii.Properties;
using KuriimuContract;

namespace Kukkii
{
	public partial class frmConverter : Form
	{
		private IImageAdapter _imageAdapter = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		private List<IImageAdapter> _imageAdapters = null;

		public frmConverter(string[] args)
		{
			InitializeComponent();

			// Load Plugins
			_imageAdapters = PluginLoader<IImageAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "image*.dll").ToList();
		}

		private void frmConverter_Load(object sender, EventArgs e)
		{
			Icon = Resources.kukkii;

			Tools.DoubleBuffer(pbxPreview, true);
			//LoadForm();
			UpdateForm();
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
			ofd.Filter = Tools.LoadImageFilters(_imageAdapters);

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
					IImageAdapter _tempAdapter = SelectImageAdapter(filename);

					if (_tempAdapter != null)
					{
						_imageAdapter = _tempAdapter;

						if (_imageAdapter != null && _imageAdapter.Load(filename) == LoadResult.Success)
						{
							_fileOpen = true;
							_hasChanges = false;

							//LoadEntries();
							//UpdateTextView();
							UpdatePreview();
							UpdateForm();
						}
					}

					Settings.Default.LastDirectory = new FileInfo(filename).DirectoryName;
					Settings.Default.Save();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK);
					_fileOpen = false;
					_hasChanges = false;
					UpdateForm();
				}
			}
		}

		private DialogResult SaveFile(bool saveAs = false)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			DialogResult dr = DialogResult.OK;

			sfd.FileName = _imageAdapter.FileInfo.Name;
			sfd.Filter = _imageAdapter.Description + " (" + _imageAdapter.Extension + ")|" + _imageAdapter.Extension;

			if (_imageAdapter.FileInfo == null || saveAs)
			{
				sfd.InitialDirectory = Settings.Default.LastDirectory;
				dr = sfd.ShowDialog();
			}

			if ((_imageAdapter.FileInfo == null || saveAs) && dr == DialogResult.OK)
			{
				_imageAdapter.FileInfo = new FileInfo(sfd.FileName);
				Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
				Settings.Default.Save();
			}

			if (dr == DialogResult.OK)
			{
				_imageAdapter.Save(_imageAdapter.FileInfo.FullName);
				_hasChanges = false;
				UpdateForm();
			}

			return dr;
		}

		private IImageAdapter SelectImageAdapter(string filename)
		{
			IImageAdapter result = null;

			try
			{
				// first look for adapters whose extension matches that of our filename
				List<IImageAdapter> matchingAdapters = _imageAdapters.Where(adapter => adapter.Extension.Split(';').Any(s => filename.ToLower().EndsWith(s.Substring(1).ToLower()))).ToList();

				result = matchingAdapters.FirstOrDefault(adapter => adapter.Identify(filename));

				// if none of them match, then try all other adapters
				if (result == null)
					result = _imageAdapters.Except(matchingAdapters).FirstOrDefault(adapter => adapter.Identify(filename));

				if (result == null)
					MessageBox.Show("None of the installed plugins were able to open the file.", "Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK);
				_fileOpen = false;
				_hasChanges = false;
			}

			return result;
		}


		private void UpdateForm()
		{
			Text = Settings.Default.ApplicationName + " " + Settings.Default.ApplicationVersion + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

			//IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;

			//if (_fileOpen)
			//	tslEntries.Text = (_fileAdapter.Entries?.Count() + " Entries").Trim();
			//else
			//	tslEntries.Text = "Entries";

			if (_imageAdapter != null)
			{
				//bool itemSelected = _fileOpen && treEntries.SelectedNode != null;
				//bool canAdd = _fileOpen && _fileAdapter.CanAddEntries;
				//bool canRename = itemSelected && _fileAdapter.CanRenameEntries && entry.ParentEntry == null;
				//bool canDelete = itemSelected && _fileAdapter.CanDeleteEntries && entry.ParentEntry == null;

				//splMain.Enabled = _fileOpen;
				//splContent.Enabled = _fileOpen;
				//splText.Enabled = _fileOpen;
				//splPreview.Enabled = _fileOpen;

				//// Menu
				//saveToolStripMenuItem.Enabled = _fileOpen && _fileAdapter.CanSave;
				//tsbSave.Enabled = _fileOpen && _fileAdapter.CanSave;
				//saveAsToolStripMenuItem.Enabled = _fileOpen && _fileAdapter.CanSave;
				//tsbSaveAs.Enabled = _fileOpen && _fileAdapter.CanSave;
				//findToolStripMenuItem.Enabled = _fileOpen;
				//tsbFind.Enabled = _fileOpen;
				//propertiesToolStripMenuItem.Enabled = _fileOpen && _fileAdapter.FileHasExtendedProperties;
				//tsbProperties.Enabled = _fileOpen && _fileAdapter.FileHasExtendedProperties;

				//// Toolbar
				exportToolStripMenuItem.Enabled = _fileOpen;
				//addEntryToolStripMenuItem.Enabled = canAdd;
				//tsbEntryAdd.Enabled = canAdd;
				//renameEntryToolStripMenuItem.Enabled = canRename;
				//tsbEntryRename.Enabled = canRename;
				//deleteEntryToolStripMenuItem.Enabled = canDelete;
				//tsbEntryDelete.Enabled = canDelete;
				//entryPropertiesToolStripMenuItem.Enabled = itemSelected && _fileAdapter.EntriesHaveExtendedProperties;
				//tsbEntryProperties.Enabled = itemSelected && _fileAdapter.EntriesHaveExtendedProperties;
				//sortEntriesToolStripMenuItem.Enabled = _fileOpen && _fileAdapter.CanSortEntries;
				//sortEntriesToolStripMenuItem.Image = _fileAdapter.SortEntries ? Resources.menu_sorted : Resources.menu_unsorted;
				//tsbSortEntries.Enabled = _fileOpen && _fileAdapter.CanSortEntries;
				//tsbSortEntries.Image = _fileAdapter.SortEntries ? Resources.menu_sorted : Resources.menu_unsorted;
				//tsbPreviewEnabled.Enabled = _gameHandler != null ? _gameHandler.HandlerCanGeneratePreviews : false;
				//tsbPreviewEnabled.Image = Settings.Default.PreviewEnabled ? Resources.menu_preview_visible : Resources.menu_preview_invisible;
				//tsbPreviewEnabled.Text = Settings.Default.PreviewEnabled ? "Disable Preview" : "Enable Preview";

				//treEntries.Enabled = _fileOpen;

				//if (itemSelected)
				//{
				//	txtEdit.Enabled = entry.HasText;
				//	if (!entry.HasText && entry.IsSubEntry)
				//		txtEdit.Text = "This entry has no text.";
				//	else if (!entry.HasText && !entry.IsSubEntry)
				//		txtEdit.Text = "Select a child item to edit the text.";
				//	txtOriginal.Enabled = entry.HasText && txtOriginal.Text.Trim().Length > 0;
				//}
				//else
				//{
				//	txtEdit.Enabled = itemSelected;
				//	txtOriginal.Enabled = itemSelected && txtOriginal.Text.Trim().Length > 0;
				//}

				//tsbGameSelect.Enabled = itemSelected;
			}
		}

		private void UpdatePreview()
		{
			Bitmap bmp = _imageAdapter.Bitmaps.ToList()[0];

			pbxPreview.Image = bmp;

			pbxPreview.Location = new Point(pnlPreview.Width / 2 - bmp.Width / 2, pnlPreview.Height / 2 - bmp.Height / 2);
		}

		private string FileName()
		{
			return _imageAdapter == null || _imageAdapter.FileInfo == null ? string.Empty : _imageAdapter.FileInfo.Name;
		}

		private void frmConverter_Resize(object sender, EventArgs e)
		{
			UpdatePreview();
		}

		private void exportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.InitialDirectory = Settings.Default.LastDirectory;
			sfd.Filter = "Portable Network Graphics (*.png)|*.png";

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				Bitmap bmp = _imageAdapter.Bitmaps.ToList()[0];
				bmp.Save(sfd.FileName, ImageFormat.Png);
			}
		}
	}
}