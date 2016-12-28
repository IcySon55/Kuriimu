using file_gmml.Properties;
using KuriimuContract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace file_gmml
{
	public class GmmlAdapter : IFileAdapter
	{
		private FileInfo _fileInfo = null;
		private GMML _gmm = null;
		private List<Entry> _entries = null;

		#region Properties

		// Information
		public string Name => "GMML";

		public string Description => "GMML Text File";

		public string Extension => "*.gmm;*.gmml";

		public string About => "This is the GMML file adapter for Kuriimu.";

		// Feature Support
		public bool FileHasExtendedProperties => false;

		public bool CanSave => true;

		public bool CanAddEntries => false;

		public bool CanRenameEntries => false;

		public bool CanRemoveEntries => false;

		public bool EntriesHaveSubEntries => true;

		public bool OnlySubEntriesHaveText => true;

		public bool EntriesHaveUniqueNames => false;

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

			if (_fileInfo.Exists)
			{
				try
				{
					_gmm = GMML.Load(_fileInfo.FullName);
				}
				catch (XmlException)
				{
					result = LoadResult.TypeMismatch;
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
				_gmm.Save(_fileInfo.FullName);
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
				GMML.Load(filename);
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
				_entries = new List<Entry>();

				foreach (Row row in _gmm.Body.Rows)
				{
					Entry entry = new Entry(row);
					_entries.Add(entry);

					foreach (Language lang in row.Languages)
					{
						Entry subEntry = new Entry(lang);
						entry.SubEntries.Add(subEntry);
					}
				}

				return _entries;
			}
		}

		public List<string> NameList => null;

		public string NameFilter => @".*";

		public int NameMaxLength => 0;

		// Features
		public bool ShowProperties(Icon icon) => false;

		public IEntry NewEntry() => null;

		public bool AddEntry(IEntry entry) => false;

		public bool RenameEntry(IEntry entry, string name) => false;

		public bool RemoveEntry(IEntry entry) => false;

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

	public class Entry : IEntry
	{
		public Encoding Encoding { get; set; }

		public string Name
		{
			get { return IsSubEntry ? EditedLanguage.Name : Row.Comment; }
			set
			{
				if (IsSubEntry)
					EditedLanguage.Name = value;
				else
					Row.Comment = value;
			}
		}

		public byte[] OriginalText
		{
			get { return Encoding.GetBytes(OriginalLanguage.Text); }
			set {; }
		}

		public string OriginalTextString
		{
			get { return Encoding.GetString(OriginalText); }
			set {; }
		}

		public byte[] EditedText
		{
			get { return Encoding.GetBytes(EditedLanguage.Text); }
			set { EditedLanguage.Text = Encoding.GetString(value); }
		}

		public string EditedTextString
		{
			get { return Encoding.GetString(EditedText); }
			set { EditedText = Encoding.GetBytes(value); }
		}

		public int MaxLength { get; set; }

		public bool IsResizable
		{
			get { return MaxLength == 0; }
		}

		public List<IEntry> SubEntries { get; set; }
		public bool IsSubEntry { get; set; }

		public Row Row { get; set; }
		public Language OriginalLanguage { get; set; }
		public Language EditedLanguage { get; set; }

		public Entry()
		{
			Encoding = Encoding.Unicode;
			Row = new Row();
			OriginalLanguage = new Language();
			EditedLanguage = new Language();
			Name = string.Empty;
			MaxLength = 0;
			OriginalText = new byte[] { };
			EditedText = new byte[] { };
			SubEntries = new List<IEntry>();
			IsSubEntry = false;
		}

		public Entry(Row row) : this()
		{
			Row = row;
			IsSubEntry = false;
		}

		public Entry(Language editedLanguage) : this()
		{
			EditedLanguage = editedLanguage;
			IsSubEntry = true;
		}

		public override string ToString()
		{
			return IsSubEntry ? EditedLanguage.Name : Name == string.Empty ? Row.IDString : Name;
		}

		public int CompareTo(IEntry rhs)
		{
			int result = Name.CompareTo(rhs.Name);
			if (result == 0)
				result = Row.ID.CompareTo(((Entry)rhs).Row.ID);
			return result;
		}
	}
}