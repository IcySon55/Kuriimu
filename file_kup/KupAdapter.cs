using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using KuriimuContract;

namespace file_kup
{
	public class KupAdatper : IFileAdapter
	{
		private FileInfo _targetFile = null;
		private KUP _kup = null;

		#region Properties

		// Information
		public string Name
		{
			get { return "KUP"; }
		}

		public string Description
		{
			get { return "Kuriimu Archive"; }
		}

		public string Extension
		{
			get { return "*.kup"; }
		}

		public string About
		{
			get { return "This is the KUP file adapter for Kuriimu."; }
		}

		// Feature Support
		public bool FileHasExtendedProperties
		{
			get { return true; }
		}

		public bool CanSave
		{
			get { return true; }
		}

		public bool CanAddEntries
		{
			get { return false; }
		}

		public bool CanRemoveEntries
		{
			get { return false; }
		}

		public bool EntriesHaveUniqueNames
		{
			get { return false; }
		}

		public bool EntriesHaveExtendedProperties
		{
			get { return true; }
		}

		#endregion

		public FileInfo FileInfo
		{
			get
			{
				return _targetFile;
			}
			set
			{
				_targetFile = value;
			}
		}

		public LoadResult Load(string filename)
		{
			Encoding encoding = Encoding.Unicode;
			LoadResult result = LoadResult.Success;

			_targetFile = new FileInfo(filename);

			if (_targetFile.Exists)
			{
				try
				{
					_kup = KUP.Load(filename);

					foreach (Entry entry in _kup.Entries)
						entry.PointerCleanup();
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
				_targetFile = new FileInfo(filename);

			try
			{
				_kup.Save(filename);
			}
			catch (Exception)
			{
				result = SaveResult.Failure;
			}

			return result;
		}

		// Entries
		public bool HasEntries
		{
			get { return _kup.Entries.Count > 0; }
		}

		public List<IEntry> Entries
		{
			get
			{
				List<IEntry> entries = new List<IEntry>();
				entries.AddRange(_kup.Entries);
				return entries;
			}
		}

		public List<string> NameList
		{
			get { return null; }
		}

		// Features
		public bool ShowProperties(Icon icon)
		{
			return false;
		}

		public IEntry NewEntry()
		{
			return new Entry(_kup.Encoding); ;
		}

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

		public bool RemoveEntry(IEntry entry)
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
			frmEntryProperties properties = new frmEntryProperties((Entry)entry, icon);
			properties.ShowDialog();
			return properties.HasChanges;
		}
	}
}