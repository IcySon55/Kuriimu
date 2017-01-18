using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using file_btxt.Properties;
using KuriimuContract;

namespace file_btxt
{
	class BtxtAdapter : IFileAdapter
	{
		private FileInfo _fileInfo = null;
		private BTXT _btxt = null;
		private BTXT _btxtBackup = null;
		private List<Entry> _entries = null;

		#region Properties

		// Information
		public string Name => "BTXT";

		public string Description => "Binary Text";

		public string Extension => "*.btxt";

		public string About => "This is the BTXT file adapter for Kuriimu.";

		// Feature Support
		public bool FileHasExtendedProperties => false;

		public bool CanSave => true;

		public bool CanAddEntries => false;

		public bool CanRenameEntries => false;

		public bool CanDeleteEntries => false;

		public bool CanSortEntries => true;

		public bool EntriesHaveSubEntries => false;

		public bool OnlySubEntriesHaveText => false;

		public bool EntriesHaveUniqueNames => true;

		public bool EntriesHaveExtendedProperties => false;

		public FileInfo FileInfo
		{
			get
			{
				return _fileInfo;
			}
			set
			{
				_fileInfo = value;
			}
		}

		#endregion

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);
			_entries = null;

			if (_fileInfo.Exists)
			{
				try
				{
					_btxt = new BTXT(_fileInfo.FullName);

					string backupFilePath = _fileInfo.FullName + ".bak";
					if (File.Exists(backupFilePath))
					{
						_btxtBackup = new BTXT(backupFilePath);
					}
					else if (MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						File.Copy(_fileInfo.FullName, backupFilePath);
						_btxtBackup = new BTXT(backupFilePath);
					}
					else
					{
						_btxtBackup = null;
					}
				}
				catch (Exception)
				{
					result = LoadResult.Failure;
				}
			}
			else
				result = LoadResult.FileNotFound;

			return result;
		}

		public SaveResult Save(string filename = "")
		{
			SaveResult result = SaveResult.Success;

			if (filename.Trim() != string.Empty)
				_fileInfo = new FileInfo(filename);

			try
			{
				//_btxt.Save(_fileInfo.FullName);
			}
			catch (Exception)
			{
				result = SaveResult.Failure;
			}

			return result;
		}

		public bool Identify(string filename)
		{
			bool result = true;

			try
			{
				new BTXT(filename);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		// Entries
		public IEnumerable<IEntry> Entries
		{
			get
			{
				if (_entries == null)
				{
					_entries = new List<Entry>();

					foreach (Label label in _btxt.Labels)
					{
						if (_btxtBackup == null)
						{
							Entry entry = new Entry(_btxt.FileEncoding, label);
							_entries.Add(entry);
						}
						else
						{
							Entry entry = new Entry(_btxt.FileEncoding, label, _btxtBackup.Labels.FirstOrDefault(o => o.Name == label.Name));
							_entries.Add(entry);
						}
					}
				}

				return _entries;
			}
		}

		public IEnumerable<string> NameList => Entries?.Select(o => o.Name);

		public string NameFilter => @".*";

		public int NameMaxLength => 0;

		// Features
		public bool ShowProperties(Icon icon) => false;

		public IEntry NewEntry() => new Entry(_btxt.FileEncoding);

		public bool AddEntry(IEntry entry) => false;

		public bool RenameEntry(IEntry entry, string newName) => false;

		public bool DeleteEntry(IEntry entry) => false;

		public bool ShowEntryProperties(IEntry entry, Icon icon) => false;

		// Settings
		public bool SortEntries
		{
			get { return Settings.Default.SortEntries; }
			set
			{
				Settings.Default.SortEntries = value;
				Settings.Default.Save();
			}
		}
	}

	public sealed class Entry : IEntry
	{
		public Encoding Encoding { get; set; }

		public string Name
		{
			get { return EditedLabel.Name; }
			set {; }
		}

		public byte[] OriginalText
		{
			get { return OriginalLabel.Text; }
			set {; }
		}

		public string OriginalTextString
		{
			get { return Encoding.GetString(OriginalLabel.Text); }
			set {; }
		}

		public byte[] EditedText
		{
			get { return EditedLabel.Text; }
			set { EditedLabel.Text = value; }
		}

		public string EditedTextString
		{
			get { return Encoding.GetString(EditedLabel.Text); }
			set { EditedLabel.Text = Encoding.GetBytes(value); }
		}

		public int MaxLength { get; set; }

		public bool IsResizable => true;

		public List<IEntry> SubEntries { get; set; }
		public bool IsSubEntry { get; set; }

		public Label EditedLabel { get; set; }
		public Label OriginalLabel { get; }

		public Entry()
		{
			Encoding = Encoding.Unicode;
			EditedLabel = new Label();
			OriginalLabel = new Label();
			Name = string.Empty;
			MaxLength = 0;
			OriginalText = new byte[] { };
			EditedText = new byte[] { };
			SubEntries = new List<IEntry>();
		}

		public Entry(Encoding encoding) : this()
		{
			Encoding = encoding;
		}

		public Entry(Encoding encoding, Label editedLabel) : this(encoding)
		{
			if (editedLabel != null)
				EditedLabel = editedLabel;
		}

		public Entry(Encoding encoding, Label editedLabel, Label originalLabel) : this(encoding, editedLabel)
		{
			if (originalLabel != null)
				OriginalLabel = originalLabel;
		}

		public override string ToString()
		{
			return Name;
		}

		public int CompareTo(IEntry rhs)
		{
			int result = Name.CompareTo(rhs.Name);
			return result;
		}
	}
}