using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kukki.Properties;
using KuriimuContract;

namespace Kukki
{
	public partial class frmConverter : Form
	{
		private IFileAdapter _imageAdapter = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		public frmConverter(string[] args)
		{
			InitializeComponent();
		}

		private void frmConverter_Load(object sender, EventArgs e)
		{
			Icon = Resources.kukki;

			//Tools.DoubleBuffer(treEntries, true);
			//LoadForm();
			UpdateForm();
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

		private string FileName()
		{
			return _imageAdapter == null || _imageAdapter.FileInfo == null ? string.Empty : _imageAdapter.FileInfo.Name;
		}
	}
}