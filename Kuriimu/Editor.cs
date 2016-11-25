using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Be.Windows.Forms;
using Kuriimu.Properties;
using KuriimuContract;

namespace Kuriimu
{
	public partial class Editor : Form
	{
		private Main _main = null;
		private FileInfo _file = null;
		private IFileAdapter _fileAdapter = null;
		private IControlCodeHandler _codeHandler = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		Dictionary<string, IFileAdapter> fileAdapters = null;
		Dictionary<string, IControlCodeHandler> codeHandlers = null;

		public Editor(Main main)
		{
			InitializeComponent();
			_main = main;
		}

		private void Editor_Load(object sender, EventArgs e)
		{
			this.Icon = Resources.kuriimu;

			// Load Plugins
			Console.WriteLine("Loading plugins...");

			fileAdapters = new Dictionary<string, IFileAdapter>();
			foreach (IFileAdapter fileAdapter in PluginLoader<IFileAdapter>.LoadPlugins(Settings.Default.PluginDirectory))
				fileAdapters.Add(fileAdapter.Name, fileAdapter);

			tsbGameSelect.DropDownItems.Clear();
			ToolStripMenuItem tsiNoGame = new ToolStripMenuItem("No Game", Resources.game_none, tsbGameSelect_SelectedIndexChanged);
			tsbGameSelect.DropDownItems.Add(tsiNoGame);
			tsbGameSelect.Text = tsiNoGame.Text;
			tsbGameSelect.Image = tsiNoGame.Image;

			codeHandlers = new Dictionary<string, IControlCodeHandler>();
			foreach (IControlCodeHandler codeHandler in PluginLoader<IControlCodeHandler>.LoadPlugins(Settings.Default.PluginDirectory))
			{
				codeHandlers.Add(codeHandler.Name, codeHandler);

				ToolStripMenuItem tsiHandler = new ToolStripMenuItem(codeHandler.Name, codeHandler.Icon, tsbGameSelect_SelectedIndexChanged);
				tsbGameSelect.DropDownItems.Add(tsiHandler);
			}
		}

		private void tsbGameSelect_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToolStripItem tsi = (ToolStripItem)sender;

			if (tsi.Text == "No Game")
				_codeHandler = null;
			else
				_codeHandler = codeHandlers[((ToolStripItem)sender).Text];

			tsbGameSelect.Text = tsi.Text;
			tsbGameSelect.Image = tsi.Image;

			lstEntries_SelectedIndexChanged(sender, e);
		}

		private void Editor_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_main != null)
				_main.Show();
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
			List<string> types = new List<string>();
			foreach (string key in fileAdapters.Keys)
				types.Add(fileAdapters[key].Description + " (" + fileAdapters[key].Extension + ")|" + fileAdapters[key].Extension);
			types.Add("All Files (*.*)|*.*");
			ofd.Filter = string.Join("|", types.ToArray());

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
						_file = new FileInfo(filename);
						_fileOpen = true;
						_hasChanges = false;
						_fileAdapter.Load(filename);
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

		private IFileAdapter SelectFileAdapter(string filename)
		{
			IFileAdapter result = null;
			string extension = new FileInfo(filename).Extension;

			foreach (string key in fileAdapters.Keys)
			{
				if (fileAdapters[key].Extension.EndsWith(extension))
				{
					result = fileAdapters[key];
					break;
				}
			}

			return result;
		}

		private void LoadEntries()
		{
			int selectedIndex = lstEntries.SelectedIndex;

			lstEntries.Items.Clear();

			for (int i = 0; i < _fileAdapter.Entries.Count; i++)
				lstEntries.Items.Add(_fileAdapter.Entries[i]);

			if (selectedIndex > lstEntries.Items.Count - 1)
				selectedIndex = lstEntries.Items.Count - 1;

			if (lstEntries.Items.Count > 0)
				lstEntries.SelectedIndex = selectedIndex;
		}

		private DialogResult SaveFile(bool saveAs = false)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			DialogResult dr = DialogResult.OK;

			sfd.Filter = _fileAdapter.Description + " (" + _fileAdapter.Extension + ")|" + _fileAdapter.Extension;

			if (_file == null || saveAs)
			{
				sfd.InitialDirectory = Settings.Default.LastDirectory;
				dr = sfd.ShowDialog();
			}

			if ((_file == null || saveAs) && dr == DialogResult.OK)
			{
				_file = new FileInfo(sfd.FileName);
				Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
				Settings.Default.Save();
				Settings.Default.Reload();
			}

			if (dr == DialogResult.OK)
			{
				_fileAdapter.Save(sfd.FileName);
				_hasChanges = false;
				UpdateForm();
			}

			return dr;
		}

		// Utilities
		private void UpdateTextView()
		{
			IEntry entry = (IEntry)lstEntries.SelectedItem;

			if (_codeHandler != null)
			{
				txtEdit.Text = _codeHandler.GetString(entry.EditedText, entry.Encoding).Replace("\0", "<null>");
				txtOriginal.Text = _codeHandler.GetString(entry.OriginalText, entry.Encoding).Replace("\0", "<null>");
			}
			else
			{
				txtEdit.Text = entry.GetEditedString().Replace("\0", "<null>");
				txtOriginal.Text = entry.GetOriginalString().Replace("\0", "<null>");
			}
		}

		private void UpdateHexView()
		{
			DynamicFileByteProvider dfbp = null;

			try
			{
				IEntry ent = (IEntry)lstEntries.SelectedItem;
				MemoryStream strm = new MemoryStream(ent.EditedText);

				dfbp = new DynamicFileByteProvider(strm);
				dfbp.Changed += new EventHandler(hbxEdit_Changed);
			}
			catch (Exception)
			{ }

			hbxHexView.ByteProvider = dfbp;
		}

		private void UpdateForm()
		{
			this.Text = Settings.Default.ApplicationName + " Editor" + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

			if (_fileOpen)
				tslEntries.Text = _fileAdapter.Entries.Count + " Entries";
			else
				tslEntries.Text = "Entries";

			bool itemSelected = lstEntries.SelectedIndex >= 0;

			tsbRename.Enabled = itemSelected;
			tsbProperties.Enabled = _fileAdapter.EntriesHaveExtendedProperties && itemSelected;
			tsbGameSelect.Enabled = itemSelected;
		}

		private string FileName()
		{
			return _fileAdapter == null || _fileAdapter.TargetFile == null ? string.Empty : _fileAdapter.TargetFile.Name;
		}

		private void lstEntries_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateTextView();
			UpdateHexView();
			UpdateForm();
		}

		// Toolbar
		private void tsbRename_Click(object sender, EventArgs e)
		{
			IEntry entry = (IEntry)lstEntries.SelectedItem;

			Name name = new Name(entry, null, _fileAdapter.EntriesHaveUniqueNames);
			
			if (name.ShowDialog() == DialogResult.OK)
				LoadEntries();
		}

		private void tsbProperties_Click(object sender, EventArgs e)
		{
			IEntry entry = (IEntry)lstEntries.SelectedItem;
			_fileAdapter.EntryProperties(entry, Properties.Resources.kuriimu);
		}

		private void lstEntries_KeyUp(object sender, KeyEventArgs e)
		{
			if (lstEntries.Focused && e.KeyCode == Keys.F8)
				tsbProperties_Click(sender, e);
		}

		// Text
		private void txtEdit_KeyUp(object sender, KeyEventArgs e)
		{
			IEntry entry = (IEntry)lstEntries.SelectedItem;

			if (_codeHandler != null)
				entry.EditedText = _codeHandler.GetBytes(txtEdit.Text.Replace("<null>", "\0"), entry.Encoding);
			else
				entry.EditedText = entry.Encoding.GetBytes(txtEdit.Text.Replace("<null>", "\0"));

			UpdateHexView();

			if (txtEdit.Text != txtOriginal.Text)
				_hasChanges = true;

			UpdateForm();
		}

		protected void hbxEdit_Changed(object sender, EventArgs e)
		{
			DynamicFileByteProvider dfbp = (DynamicFileByteProvider)sender;

			IEntry entry = (IEntry)lstEntries.SelectedItem;
			List<byte> bytes = new List<byte>();
			for (int i = 0; i < (int)dfbp.Length; i++)
				bytes.Add(dfbp.ReadByte(i));
			entry.EditedText = bytes.ToArray();

			UpdateTextView();

			if (txtEdit.Text != txtOriginal.Text)
				_hasChanges = true;

			UpdateForm();
		}
	}
}