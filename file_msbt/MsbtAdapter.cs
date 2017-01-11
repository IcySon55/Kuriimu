using file_msbt.Properties;
using KuriimuContract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace file_msbt
{
	public sealed class MsbtAdapter : IFileAdapter
	{
		private FileInfo _fileInfo = null;
		private MSBT _msbt = null;
		private MSBT _msbtBackup = null;
		private List<Entry> _entries = null;

		#region Properties

		// Information
		public string Name => "MSBT";

		public string Description => "Message Binary Text";

		public string Extension => "*.msbt";

		public string About => "This is the MSBT file adapter for Kuriimu.";

		// Feature Support
		public bool FileHasExtendedProperties => false;

		public bool CanSave => true;

		public bool CanAddEntries => true;

		public bool CanRenameEntries => true;

		public bool CanRemoveEntries => true;

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
					_msbt = new MSBT(_fileInfo.FullName);

					string backupFilePath = _fileInfo.FullName + ".bak";
					if (File.Exists(backupFilePath))
					{
						_msbtBackup = new MSBT(backupFilePath);
					}
					else if (MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						File.Copy(_fileInfo.FullName, backupFilePath);
						_msbtBackup = new MSBT(backupFilePath);
					}
					else
					{
						_msbtBackup = null;
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
				if (_entries == null)
				{
					if (_msbtBackup == null)
						_entries = _msbt.LBL1.Labels.OrderBy(o => o.Index).Select(o => new Entry(_msbt.FileEncoding, o)).ToList();
					else
						_entries = _msbt.LBL1.Labels.OrderBy(o => o.Index).Select(o => new Entry(_msbt.FileEncoding, o, _msbtBackup.LBL1.Labels.FirstOrDefault(b => b.Name == o.Name))).ToList();
				}

				return _entries;
			}
		}

		public IEnumerable<string> NameList => Entries?.Select(o => o.Name);

		public string NameFilter => MSBT.LabelFilter;

		public int NameMaxLength => MSBT.LabelMaxLength;

		// Features
		public bool ShowProperties(Icon icon) => false;

		public IEntry NewEntry() => new Entry(_msbt.FileEncoding);

		public bool AddEntry(IEntry entry)
		{
			bool result = true;

			try
			{
				Entry ent = (Entry)entry;
				ent.EditedLabel = _msbt.AddLabel(entry.Name);
				_entries.Add((Entry)entry);
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
				Entry ent = (Entry)entry;
				_msbt.RenameLabel(ent.EditedLabel, newName);
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
				Entry ent = (Entry)entry;
				_msbt.RemoveLabel(ent.EditedLabel);
				_entries.Remove(ent);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

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
			set { EditedLabel.Name = value; }
		}

		public byte[] OriginalText
		{
			get { return OriginalLabel.String.Text; }
			set {; }
		}

		public string OriginalTextString
		{
			get { return Encoding.GetString(OriginalLabel.String.Text); }
			set {; }
		}

		public byte[] EditedText
		{
			get { return EditedLabel.String.Text; }
			set { EditedLabel.String.Text = value; }
		}

		public string EditedTextString
		{
			get { return Encoding.GetString(EditedLabel.String.Text); }
			set { EditedLabel.String.Text = Encoding.GetBytes(value); }
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
			return Name == string.Empty ? EditedLabel.String.Index.ToString() : Name;
		}

		public int CompareTo(IEntry rhs)
		{
			int result = Name.CompareTo(rhs.Name);
			if (result == 0)
				result = EditedLabel.String.Index.CompareTo(((Entry)rhs).EditedLabel.String.Index);
			return result;
		}
	}
}