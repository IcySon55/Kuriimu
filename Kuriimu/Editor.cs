using Be.Windows.Forms;
using Kuriimu.Properties;
using KuriimuContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Kuriimu
{
	public partial class frmEditor : Form
	{
		private IFileAdapter _fileAdapter = null;
		private IGameHandler _gameHandler = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		private Dictionary<string, IFileAdapter> _fileAdapters = null;
		private Dictionary<string, IGameHandler> _gameHandlers = null;

		private IEnumerable<IEntry> _entries = null;

		public frmEditor()
		{
			InitializeComponent();
		}

		private void frmEditor_Load(object sender, EventArgs e)
		{
			Icon = Resources.kuriimu;

			// Load Plugins
			Console.WriteLine("Loading plugins...");

			_fileAdapters = new Dictionary<string, IFileAdapter>();
			foreach (IFileAdapter fileAdapter in PluginLoader<IFileAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "file*.dll"))
				_fileAdapters.Add(fileAdapter.Name, fileAdapter);

			_gameHandlers = Tools.LoadGameHandlers(tsbGameSelect, Resources.game_none, tsbGameSelect_SelectedIndexChanged);

			Tools.DoubleBuffer((Control)treEntries, true);

			UpdateForm();
		}

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
			frmSearch search = new frmSearch();
			search.Entries = _entries;
			search.ShowDialog();

			if (search.Selected != null)
			{
				treEntries.SelectNodeByIEntry(search.Selected);

				if (txtEdit.Text.Contains(Settings.Default.FindWhat))
				{
					txtEdit.SelectionStart = txtEdit.Text.IndexOf(Settings.Default.FindWhat);
					txtEdit.SelectionLength = Settings.Default.FindWhat.Length;
					txtEdit.Focus();

					SelectInHex();
				}
			}
		}

		private void tsbFind_Click(object sender, EventArgs e)
		{
			findToolStripMenuItem_Click(sender, e);
		}

		private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_fileAdapter.ShowProperties(Resources.kuriimu))
			{
				_hasChanges = true;
				UpdateForm();
			}
		}

		private void tsbFileProperties_Click(object sender, EventArgs e)
		{
			propertiesToolStripMenuItem_Click(sender, e);
		}

		private void gBATempToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://gbatemp.net/threads/release-kuriimu-a-general-purpose-game-translation-toolkit-for-authors-of-fan-translations.452375/");
		}

		private void gitHubToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/Icyson55/Kuriimu");
		}

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
			ofd.Filter = Tools.LoadFileFilters(_fileAdapters);

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
					_fileAdapter = SelectFileAdapter(filename);
					if (_fileAdapter != null)
					{
						_fileAdapter.Load(filename);
						_fileOpen = true;
						_hasChanges = false;

						// Select Game Handler
						if (Settings.Default.SelectedGameHandler != string.Empty)
						{
							foreach (ToolStripItem tsi in tsbGameSelect.DropDownItems)
							{
								if (tsi.Text == Settings.Default.SelectedGameHandler)
								{
									if (tsi.Text == "No Game")
										_gameHandler = null;
									else
										_gameHandler = _gameHandlers[tsi.Text];

									tsbGameSelect.Text = tsi.Text;
									tsbGameSelect.Image = tsi.Image;

									break;
								}
							}
						}

						LoadEntries();
						Settings.Default.LastDirectory = new FileInfo(filename).DirectoryName;
						Settings.Default.Save();
						Settings.Default.Reload();
					}
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
			DialogResult dr = DialogResult.OK;

			sfd.Filter = _fileAdapter.Description + " (" + _fileAdapter.Extension + ")|" + _fileAdapter.Extension;

			if (_fileAdapter.FileInfo == null || saveAs)
			{
				sfd.InitialDirectory = Settings.Default.LastDirectory;
				dr = sfd.ShowDialog();
			}

			if ((_fileAdapter.FileInfo == null || saveAs) && dr == DialogResult.OK)
			{
				_fileAdapter.FileInfo = new FileInfo(sfd.FileName);
				Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
				Settings.Default.Save();
				Settings.Default.Reload();
			}

			if (dr == DialogResult.OK)
			{
				_fileAdapter.Save(_fileAdapter.FileInfo.FullName);
				_hasChanges = false;
				UpdateForm();
			}

			return dr;
		}

		private IFileAdapter SelectFileAdapter(string filename)
		{
			IFileAdapter result = null;

			try
			{
				foreach (string key in _fileAdapters.Keys)
				{
					if (_fileAdapters[key].Identify(filename))
					{
						result = _fileAdapters[key];
						break;
					}
				}

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

		private void LoadEntries()
		{
			UpdateEntries();

			treEntries.BeginUpdate();

			IEntry selectedEntry = null;
			if (treEntries.SelectedNode != null)
				selectedEntry = (IEntry)treEntries.SelectedNode.Tag;

			treEntries.Nodes.Clear();
			if (_entries != null)
			{
				foreach (IEntry entry in _entries)
				{
					TreeNode node = new TreeNode(entry.ToString());
					node.Tag = entry;
					if (_fileAdapter.OnlySubEntriesHaveText)
					{
						node.ForeColor = System.Drawing.Color.Gray;
					}
					treEntries.Nodes.Add(node);

					if (_fileAdapter.EntriesHaveSubEntries)
						foreach (IEntry sub in entry.SubEntries)
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
				treEntries.SelectNodeByIEntry(selectedEntry);

			treEntries.EndUpdate();

			treEntries.Focus();
		}

		private void UpdateEntries()
		{
			_entries = _fileAdapter.Entries;

			if (_fileAdapter.SortEntries)
				_entries = _entries.OrderBy(x => x);
		}

		// Utilities
		private void UpdateTextView()
		{
			IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;

			if (entry == null)
			{
				txtEdit.Text = string.Empty;
				txtOriginal.Text = string.Empty;
			}
			else if (_gameHandler != null)
			{
				txtEdit.Text = _gameHandler.GetString(entry.EditedText, entry.Encoding).Replace("\0", "<null>").Replace("\n", "\r\n");
				txtOriginal.Text = _gameHandler.GetString(entry.OriginalText, entry.Encoding).Replace("\0", "<null>").Replace("\n", "\r\n");
			}
			else
			{
				txtEdit.Text = entry.EditedTextString.Replace("\0", "<null>").Replace("\n", "\r\n");
				txtOriginal.Text = entry.OriginalTextString.Replace("\0", "<null>").Replace("\n", "\r\n");
			}

			if (entry != null && !entry.IsResizable)
				txtEdit.MaxLength = entry.MaxLength;
		}

		private void UpdatePreview()
		{
			IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;

			if (entry != null && _gameHandler != null)
			{
				pbxPreview.Image = _gameHandler.GeneratePreview(entry.EditedText, entry.Encoding);
			}
			else
				pbxPreview.Image = null;
		}

		private void UpdateHexView()
		{
			DynamicFileByteProvider dfbp = null;

			try
			{
				IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;

				if (entry != null)
				{
					MemoryStream strm = new MemoryStream(entry.EditedText);
					dfbp = new DynamicFileByteProvider(strm);
					dfbp.Changed += new EventHandler(hbxEdit_Changed);
				}
			}
			catch (Exception)
			{ }

			hbxHexView.ByteProvider = dfbp;
		}

		private void UpdateForm()
		{
			Text = Settings.Default.ApplicationName + " Editor " + Settings.Default.ApplicationVersion + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

			if (_fileOpen)
				tslEntries.Text = (_fileAdapter.Entries?.Count() + " Entries").Trim();
			else
				tslEntries.Text = "Entries";

			bool itemSelected = treEntries.SelectedNode != null;

			splMain.Enabled = _fileOpen;
			splContent.Enabled = _fileOpen;
			splText.Enabled = _fileOpen;
			splPreview.Enabled = _fileOpen;

			if (_fileAdapter != null)
			{
				saveToolStripMenuItem.Enabled = _fileOpen && _fileAdapter.CanSave;
				tsbSave.Enabled = _fileOpen && _fileAdapter.CanSave;
				saveAsToolStripMenuItem.Enabled = _fileOpen && _fileAdapter.CanSave;
				tsbSaveAs.Enabled = _fileOpen && _fileAdapter.CanSave;
				findToolStripMenuItem.Enabled = _fileOpen;
				tsbFind.Enabled = _fileOpen;
				propertiesToolStripMenuItem.Enabled = _fileOpen && _fileAdapter.FileHasExtendedProperties;
				tsbProperties.Enabled = _fileOpen && _fileAdapter.FileHasExtendedProperties;
			}

			IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;

			if (_fileAdapter != null && itemSelected)
			{

				tsbEntryAdd.Enabled = _fileOpen && _fileAdapter.CanAddEntries;
				tsbEntryRename.Enabled = itemSelected && _fileAdapter.CanRenameEntries && !entry.IsSubEntry;
				tsbEntryRemove.Enabled = _fileOpen && _fileAdapter.CanRemoveEntries && !entry.IsSubEntry;
				tsbEntryProperties.Enabled = _fileAdapter.EntriesHaveExtendedProperties && itemSelected && !entry.IsSubEntry;
				tsbSortEntries.Enabled = _fileOpen;
				tsbSortEntries.Image = _fileAdapter.SortEntries ? Resources.menu_sorted : Resources.menu_unsorted;
				treEntries.Enabled = _fileOpen;
			}

			if (_fileAdapter != null && itemSelected && _fileAdapter.OnlySubEntriesHaveText)
			{
				txtEdit.Enabled = entry.IsSubEntry;
				if (!entry.IsSubEntry)
					txtEdit.Text = "Please select a sub entry to edit the text.";
				txtOriginal.Enabled = txtOriginal.Text.Trim().Length > 0 && entry.IsSubEntry;
				hbxHexView.Enabled = entry.IsSubEntry;
			}
			else if (_fileAdapter != null && itemSelected)
			{
				txtEdit.Enabled = itemSelected;
				txtOriginal.Enabled = itemSelected && txtOriginal.Text.Trim().Length > 0;
				hbxHexView.Enabled = itemSelected;
			}
			else
			{
				UpdateTextView();
				UpdatePreview();
				UpdateHexView();
			}

			tsbGameSelect.Enabled = itemSelected;
		}

		private string FileName()
		{
			return _fileAdapter == null || _fileAdapter.FileInfo == null ? string.Empty : _fileAdapter.FileInfo.Name;
		}

		// Toolbar
		private void tsbEntryAdd_Click(object sender, EventArgs e)
		{
			IEntry entry = _fileAdapter.NewEntry();

			frmName name = new frmName(entry, _fileAdapter.EntriesHaveUniqueNames, _fileAdapter.NameList, _fileAdapter.NameFilter, _fileAdapter.NameMaxLength, true);

			if (name.ShowDialog() == DialogResult.OK && name.NameChanged)
			{
				_hasChanges = true;
				entry.Name = name.NewName;
				if (_fileAdapter.AddEntry(entry))
				{
					LoadEntries();
					treEntries.SelectNodeByIEntry(entry);
					UpdateForm();
				}
			}
		}

		private void tsbEntryRename_Click(object sender, EventArgs e)
		{
			IEntry entry = (IEntry)treEntries.SelectedNode.Tag;

			frmName name = new frmName(entry, _fileAdapter.EntriesHaveUniqueNames, _fileAdapter.NameList, _fileAdapter.NameFilter, _fileAdapter.NameMaxLength);

			if (name.ShowDialog() == DialogResult.OK && name.NameChanged)
			{
				_hasChanges = true;
				if (_fileAdapter.RenameEntry(entry, name.NewName))
					treEntries.FindNodeByIEntry(entry).Text = name.NewName;
				UpdateForm();
			}
		}

		private void tsbEntryRemove_Click(object sender, EventArgs e)
		{
			IEntry entry = (IEntry)treEntries.SelectedNode.Tag;

			if (MessageBox.Show("Are you sure you want to remove " + entry.Name + "?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				_hasChanges = true;
				TreeNode nextNode = treEntries.SelectedNode.NextNode;
				_fileAdapter.RemoveEntry(entry);
				UpdateEntries();
				treEntries.Nodes.Remove(treEntries.FindNodeByIEntry(entry));
				treEntries.SelectedNode = nextNode;
			}
		}

		private void tsbEntryProperties_Click(object sender, EventArgs e)
		{
			IEntry entry = (IEntry)treEntries.SelectedNode.Tag;
			if (_fileAdapter.ShowEntryProperties(entry, Resources.kuriimu))
			{
				_hasChanges = true;
				UpdateForm();
			}
		}

		private void tsbSortEntries_Click(object sender, EventArgs e)
		{
			_fileAdapter.SortEntries = !_fileAdapter.SortEntries;
			LoadEntries();
			UpdateForm();
		}

		private void tsbGameSelect_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToolStripItem tsi = (ToolStripItem)sender;

			if (tsi.Text == "No Game")
				_gameHandler = null;
			else
				_gameHandler = _gameHandlers[((ToolStripItem)sender).Text];

			tsbGameSelect.Text = tsi.Text;
			tsbGameSelect.Image = tsi.Image;

			UpdateTextView();
			UpdatePreview();

			Settings.Default.SelectedGameHandler = tsi.Text;
			Settings.Default.Save();
		}

		// List
		private void treEntries_AfterSelect(object sender, TreeViewEventArgs e)
		{
			UpdateTextView();
			UpdatePreview();
			UpdateHexView();
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

		// Text
		private void txtEdit_KeyUp(object sender, KeyEventArgs e)
		{
			IEntry entry = (IEntry)treEntries.SelectedNode.Tag;
			string next = string.Empty;
			string previous = string.Empty;

			if (_gameHandler != null)
			{
				previous = _gameHandler.GetString(entry.EditedText, entry.Encoding);
				next = txtEdit.Text.Replace("<null>", "\0").Replace("\r\n", "\n");
				entry.EditedText = _gameHandler.GetBytes(next, entry.Encoding);
			}
			else
			{
				previous = entry.EditedTextString;
				next = txtEdit.Text.Replace("<null>", "\0").Replace("\r\n", "\n");
				entry.EditedText = entry.Encoding.GetBytes(next);
			}

			UpdatePreview();
			UpdateHexView();
			SelectInHex();

			if (next != previous)
				_hasChanges = true;

			UpdateForm();
		}

		private void txtEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control & e.KeyCode == Keys.A)
				txtEdit.SelectAll();
			SelectInHex();
		}

		private void txtEdit_MouseUp(object sender, MouseEventArgs e)
		{
			SelectInHex();
		}

		private void SelectInHex()
		{
			// Magic
			IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;

			if (entry != null)
			{
				int selectionStart = 0;
				int selectionLength = 0;

				string startToSelection = txtEdit.Text.Substring(0, txtEdit.SelectionStart);
				if (_gameHandler != null)
					selectionStart = entry.Encoding.GetString(_gameHandler.GetBytes(startToSelection.Replace("<null>", "\0").Replace("\r\n", "\n"), entry.Encoding)).Length * (entry.Encoding.IsSingleByte ? 1 : 2);
				else
					selectionStart = startToSelection.Replace("<null>", "\0").Replace("\r\n", "\n").Length * (entry.Encoding.IsSingleByte ? 1 : 2);

				if (_gameHandler != null)
					selectionLength = entry.Encoding.GetString(_gameHandler.GetBytes(txtEdit.SelectedText.Replace("<null>", "\0").Replace("\r\n", "\n"), entry.Encoding)).Length * (entry.Encoding.IsSingleByte ? 1 : 2);
				else
					selectionLength = txtEdit.SelectedText.Replace("<null>", "\0").Replace("\r\n", "\n").Length * (entry.Encoding.IsSingleByte ? 1 : 2);

				hbxHexView.SelectionStart = selectionStart;
				hbxHexView.SelectionLength = selectionLength;
			}
		}

		protected void hbxEdit_Changed(object sender, EventArgs e)
		{
			DynamicFileByteProvider dfbp = (DynamicFileByteProvider)sender;

			IEntry entry = (IEntry)treEntries.SelectedNode?.Tag;

			if (entry != null)
			{
				List<byte> bytes = new List<byte>();
				for (int i = 0; i < (int)dfbp.Length; i++)
					bytes.Add(dfbp.ReadByte(i));
				entry.EditedText = bytes.ToArray();

				UpdateTextView();

				if (txtEdit.Text != txtOriginal.Text)
					_hasChanges = true;

				UpdatePreview();
				UpdateForm();
			}
		}
	}
}