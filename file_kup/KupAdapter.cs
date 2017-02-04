using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using file_kup.Properties;
using KuriimuContract;

namespace file_kup
{
	public class KupAdatper : IFileAdapter
	{
		private FileInfo _fileInfo = null;
		private KUP _kup = null;

		#region Properties

		// Information
		public string Name => "KUP";

		public string Description => "Kuriimu Archive";

		public string Extension => "*.kup";

		public string About => "This is the KUP file adapter for Kuriimu.";

		// Feature Support
		public bool FileHasExtendedProperties => true;

		public bool CanSave => true;

		public bool CanAddEntries => false;

		public bool CanRenameEntries => true;

		public bool CanDeleteEntries => false;

		public bool CanSortEntries => true;

		public bool EntriesHaveSubEntries => false;

		public bool EntriesHaveUniqueNames => false;

		public bool EntriesHaveExtendedProperties => true;

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
			bool result = true;

			try
			{
				KUP.Load(filename);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);

			if (_fileInfo.Exists)
			{
				_kup = KUP.Load(_fileInfo.FullName);

				foreach (Entry entry in _kup.Entries)
					entry.PointerCleanup();
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
				_kup.Save(_fileInfo.FullName);
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
				return _kup.Entries;
			}
		}

		public IEnumerable<string> NameList => null;

		public string NameFilter => @".*";

		public int NameMaxLength => 0;

		// Features
		public bool ShowProperties(Icon icon)
		{
			frmProperties properties = new frmProperties(_kup, icon);
			properties.ShowDialog();
			return properties.HasChanges;
		}

		public IEntry NewEntry() => new Entry();

		public bool AddEntry(IEntry entry)
		{
			bool result = true;

			try
			{
				_kup.Entries.Add((Entry)entry);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public bool RenameEntry(IEntry entry, string name)
		{
			bool result = true;

			try
			{
				entry.Name = name;
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public bool DeleteEntry(IEntry entry)
		{
			bool result = true;

			try
			{
				_kup.Entries.Remove((Entry)entry);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public bool ShowEntryProperties(IEntry entry, Icon icon)
		{
			frmEntryProperties entryProperties = new frmEntryProperties((Entry)entry, icon);
			entryProperties.ShowDialog();
			return entryProperties.HasChanges;
		}

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
}