using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Kuriimu.Properties;
using KuriimuContract;

namespace Kuriimu
{
	public partial class Editor : Form
	{
		private Main _main = null;
		private FileInfo _file = null;
		private IFileAdapter _fileAdapter = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		Dictionary<string, IFileAdapter> fileAdapters = null;

		public Editor(Main main)
		{
			InitializeComponent();
			_main = main;
		}

		private void Editor_Load(object sender, EventArgs e)
		{
			this.Icon = Resources.Kuriimu;

			// Load Plugins
			fileAdapters = new Dictionary<string, IFileAdapter>();
			foreach (var fileAdapter in PluginLoader<IFileAdapter>.LoadPlugins(Settings.Default.PluginDirectory))
				fileAdapters.Add(fileAdapter.Name, fileAdapter);
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
			lstEntries.Items.Clear();

			for (int i = 0; i < _fileAdapter.Entries.Count; i++)
				lstEntries.Items.Add(_fileAdapter.Entries[i]);

			if (lstEntries.Items.Count > 0)
				lstEntries.SelectedIndex = 0;
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
		private void UpdateForm()
		{
			this.Text = Settings.Default.ApplicationName + " Editor" + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

			if (_fileOpen)
				tslEntries.Text = _fileAdapter.Entries.Count + " Entries";
			else
				tslEntries.Text = "Entries";
		}

		private string FileName()
		{
			return _fileAdapter == null || _fileAdapter.TargetFile == null ? string.Empty : _fileAdapter.TargetFile.Name;
		}

		private void lstEntries_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtEdit.Text = ((IEntry)lstEntries.SelectedItem).GetOriginalString();
		}
	}
}