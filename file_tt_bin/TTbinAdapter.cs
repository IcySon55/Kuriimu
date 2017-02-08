using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using file_ttbin.Properties;
using KuriimuContract;

namespace file_ttbin
{
	public sealed class TTBinAdapter : IFileAdapter
	{
		private FileInfo _fileInfo = null;
		private TTBIN _ttbin = null;
		private TTBIN _ttbinBackup = null;
		private List<Entry> _entries = null;

		#region Properties

		// Information
		public string Name => "TTBIN";

		public string Description => "Time Travelers Binary Text";

		public string Extension => "*.cfg.bin";

		public string About => "This is the TTBin file adapter for Kuriimu.";

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
				br.BaseStream.Position = 0x18;
				uint t1 = br.ReadUInt32();
				br.BaseStream.Position = 0x24;
				uint t2 = br.ReadUInt32();
				br.BaseStream.Position = 0x30;
				uint t3 = br.ReadUInt32();
				return (t1 == 0x0 && t2 == 0x14 && t3 == 0x24);
			}
		}

		//Load file, make backup if needed
		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);
			_entries = null;

			if (_fileInfo.Exists)
			{
				_ttbin = new TTBIN(_fileInfo.FullName);

				string backupFilePath = _fileInfo.FullName + ".bak";
				if (File.Exists(backupFilePath))
				{
					_ttbinBackup = new TTBIN(backupFilePath);
				}
				else if (MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					File.Copy(_fileInfo.FullName, backupFilePath);
					_ttbinBackup = new TTBIN(backupFilePath);
				}
				else
				{
					_ttbinBackup = null;
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
				//_ttbin.Save(_fileInfo.FullName);
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

					foreach (Label label in _ttbin.Labels)
					{
						if (_ttbinBackup == null)
						{
							Entry entry = new Entry(label);
							_entries.Add(entry);
						}
						else
						{
							Entry entry = new Entry(label, _ttbinBackup.Labels.FirstOrDefault(o => o.TextID == label.TextID));
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
}