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

		public bool CanSave => false;

		public bool CanAddEntries => false;

		public bool CanRenameEntries => false;

		public bool CanDeleteEntries => false;

		public bool CanSortEntries => false;

		public bool EntriesHaveSubEntries => true;

		public bool OnlySubEntriesHaveText => true;

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
						Entry entry = new Entry(label);
						_entries.Add(entry);

						foreach (String str in label.Strings)
						{
							if (_btxtBackup == null)
							{
								Entry subEntry = new Entry(str);
								entry.SubEntries.Add(subEntry);
							}
							else
							{
								Entry subEntry = new Entry(str, _btxtBackup.Labels.FirstOrDefault(o => o.Name == label.Name).Strings.FirstOrDefault(j => j.Attribute2 == str.Attribute2));
								entry.SubEntries.Add(subEntry);
							}
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
		public Encoding Encoding { get; set; }

		public string Name
		{
			get { return IsSubEntry ? EditedString.Attribute2.ToString() : Temp.Name; }
			set {; }
		}

		public byte[] OriginalText
		{
			get { return OriginalString.Text; }
			set {; }
		}

		public string OriginalTextString
		{
			get { return Encoding.GetString(OriginalText); }
			set {; }
		}

		public byte[] EditedText
		{
			get { return EditedString.Text; }
			set { EditedString.Text = value; }
		}

		public string EditedTextString
		{
			get { return Encoding.GetString(EditedText); }
			set { EditedText = Encoding.GetBytes(value); }
		}

		public int MaxLength { get; set; }

		public bool IsResizable => true;

		public List<IEntry> SubEntries { get; set; }
		public bool IsSubEntry { get; set; }

		public Label Temp { get; set; }
		public String EditedString { get; set; }
		public String OriginalString { get; }

		public Entry()
		{
			Encoding = Encoding.Unicode;
			Temp = new Label();
			EditedString = new String();
			OriginalString = new String();
			Name = string.Empty;
			MaxLength = 0;
			OriginalText = new byte[] { };
			EditedText = new byte[] { };
			SubEntries = new List<IEntry>();
		}

		public Entry(Label row) : this()
		{
			Temp = row;
		}

		public Entry(String editedString) : this()
		{
			if (editedString != null)
				EditedString = editedString;
			IsSubEntry = true;
		}

		public Entry(String editedString, String originalString) : this(editedString)
		{
			if (originalString != null)
				OriginalString = originalString;
		}

		public override string ToString()
		{
			return IsSubEntry ? EditedString.Attribute2.ToString() : Name;
		}

		public int CompareTo(IEntry rhs)
		{
			int result = Name.CompareTo(rhs.Name);
			return result;
		}
	}
}