using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using KuriimuContract;
using file_msbt.Properties;

namespace file_msbt
{
	public class MsbtAdapter : IFileAdapter
	{
		private FileInfo _fileInfo = null;
		private MSBT _msbt = null;

		#region Properties

		// Information
		public string Name
		{
			get { return "MSBT"; }
		}

		public string Description
		{
			get { return "Message Binary Text"; }
		}

		public string Extension
		{
			get { return "*.msbt"; }
		}

		public string About
		{
			get { return "This is the MSBT file adapter for Kuriimu."; }
		}

		// Feature Support
		public bool FileHasExtendedProperties
		{
			get { return false; }
		}

		public bool CanSave
		{
			get { return true; }
		}

		public bool CanAddEntries
		{
			get { return true; }
		}

		public bool CanRemoveEntries
		{
			get { return true; }
		}

		public bool EntriesHaveSubEntries
		{
			get { return false; }
		}

		public bool EntriesHaveUniqueNames
		{
			get { return true; }
		}

		public bool EntriesHaveExtendedProperties
		{
			get { return false; }
		}

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
					_msbt = new MSBT(_fileInfo.FullName);
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
				_msbt.Save(_fileInfo.FullName);
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
				new MSBT(filename);
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
				return _msbt.Entries;
			}
		}

		public List<string> NameList
		{
			get
			{
				List<string> names = new List<string>();
				foreach (Entry entry in _msbt.Entries)
					names.Add(entry.Name);
				return names;
			}
		}

		public string NameFilter
		{
			get { return MSBT.LabelFilter; }
		}

		public int NameMaxLength
		{
			get { return MSBT.LabelMaxLength; }
		}

		// Features
		public bool ShowProperties(Icon icon)
		{
			return false;
		}

		public IEntry NewEntry()
		{
			return new Entry(_msbt.FileEncoding);
		}

		public bool AddEntry(IEntry entry)
		{
			bool result = true;

			try
			{
				_msbt.AddEntry((Entry)entry);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public bool RenameEntry(IEntry entry, string newName)
		{
			bool result = true;

			try
			{
				_msbt.RenameEntry((Entry)entry, newName);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public bool RemoveEntry(IEntry entry)
		{
			bool result = true;

			try
			{
				_msbt.RemoveEntry((Entry)entry);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public bool ShowEntryProperties(IEntry entry, Icon icon)
		{
			return false;
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