using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using file_jmsg.Properties;
using KuriimuContract;

namespace file_jmsg
{
	public sealed class JmsgAdapter : IFileAdapter
	{
		private FileInfo _fileInfo = null;
		private JMSG _jmsg = null;
		private JMSG _jmsgBackup = null;
		private List<Entry> _entries = null;

		#region Properties

		// Information
		public string Name => "JMSG";

		public string Description => "Japanese Message";

		public string Extension => "*.jmsg";

		public string About => "This is the JMSG file adapter for Kuriimu.";

		// Feature Support
		public bool FileHasExtendedProperties => false;

		public bool CanSave => true;

		public bool CanAddEntries => false;

		public bool CanRenameEntries => false;

		public bool CanDeleteEntries => false;

		public bool CanSortEntries => true;

		public bool EntriesHaveSubEntries => false;

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

		public string LineEndings => "\n";

		#endregion

		public bool Identify(string filename)
		{
			using (var br = new BinaryReaderX(File.OpenRead(filename)))
			{
				if (br.BaseStream.Length < 4) return false;
				return br.ReadString(4) == "jMSG";
			}
		}

		public LoadResult Load(string filename, bool autoBackup = false)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);
			_entries = null;

			if (_fileInfo.Exists)
			{
				_jmsg = new JMSG(_fileInfo.FullName);

				string backupFilePath = _fileInfo.FullName + ".bak";
				if (File.Exists(backupFilePath))
				{
					_jmsgBackup = new JMSG(backupFilePath);
				}
				else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					File.Copy(_fileInfo.FullName, backupFilePath);
					_jmsgBackup = new JMSG(backupFilePath);
				}
				else
				{
					_jmsgBackup = null;
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
				_jmsg.Save(_fileInfo.FullName);
			}
			catch (Exception)
			{
				result = SaveResult.Failure;
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

					foreach (Label label in _jmsg.Labels)
					{
						if (_jmsgBackup == null)
						{
							Entry entry = new Entry(label);
							_entries.Add(entry);
						}
						else
						{
							Entry entry = new Entry(label, _jmsgBackup.Labels.FirstOrDefault(o => o.TextID == label.TextID));
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

		public IEntry NewEntry() => new Entry();

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
		// Interface
		public string Name
		{
			get { return EditedLabel.Name; }
			set { }
		}

		public string OriginalText => OriginalLabel.Text;

		public string EditedText
		{
			get { return EditedLabel.Text; }
			set { EditedLabel.Text = value; }
		}

		public int MaxLength { get; set; }

		public IEntry ParentEntry { get; set; }

		public bool IsSubEntry => ParentEntry != null;

		public bool HasText { get; }

		public List<IEntry> SubEntries { get; set; }

		// Adapter
		public Label OriginalLabel { get; }
		public Label EditedLabel { get; set; }

		public Entry()
		{
			OriginalLabel = new Label();
			EditedLabel = new Label();

			Name = string.Empty;
			MaxLength = 0;
			ParentEntry = null;
			HasText = true;
			SubEntries = new List<IEntry>();
		}

		public Entry(Label editedLabel) : this()
		{
			if (editedLabel != null)
				EditedLabel = editedLabel;
		}

		public Entry(Label editedLabel, Label originalLabel) : this(editedLabel)
		{
			if (originalLabel != null)
				OriginalLabel = originalLabel;
		}

		public override string ToString()
		{
			return Name == string.Empty ? EditedLabel.TextOffset.ToString("X2") : Name;
		}

		public int CompareTo(IEntry rhs)
		{
			int result = Name.CompareTo(rhs.Name);
			if (result == 0)
				result = EditedLabel.TextID.CompareTo(((Entry)rhs).EditedLabel.TextID);
			return result;
		}
	}
}