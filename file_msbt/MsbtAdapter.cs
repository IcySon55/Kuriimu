using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using file_msbt.Properties;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace file_msbt
{
    public sealed class MsbtAdapter : IFileAdapter
    {
        private FileInfo _fileInfo = null;
        private MSBT _msbt = null;
        private MSBT _msbtBackup = null;
        private List<MsbtEntry> _entries = null;

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

        public bool CanDeleteEntries => true;

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
                if (br.BaseStream.Length < 8) return false;
                return br.ReadString(8) == "MsgStdBn";
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);
            _entries = null;

            if (_fileInfo.Exists)
            {
                _msbt = new MSBT(_fileInfo.FullName);

                string backupFilePath = _fileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                {
                    _msbtBackup = new MSBT(backupFilePath);
                }
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(_fileInfo.FullName, backupFilePath);
                    _msbtBackup = new MSBT(backupFilePath);
                }
                else
                {
                    _msbtBackup = null;
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

        // Entries
        public IEnumerable<TextEntry> Entries
        {
            get
            {
                if (_entries == null)
                {
                    if (_msbtBackup == null)
                        _entries = _msbt.LBL1.Labels.OrderBy(o => o.Index).Select(o => new MsbtEntry(o)).ToList();
                    else
                        _entries = _msbt.LBL1.Labels.OrderBy(o => o.Index).Select(o => new MsbtEntry(o, _msbtBackup.LBL1.Labels.FirstOrDefault(b => b.Name == o.Name))).ToList();
                }

                if (SortEntries)
                    return _entries.OrderBy(e => e.Name).ThenBy(e => e.EditedLabel.Index);

                return _entries;
            }
        }

        public IEnumerable<string> NameList => Entries?.Select(o => o.Name);

        public string NameFilter => MSBT.LabelFilter;

        public int NameMaxLength => MSBT.LabelMaxLength;

        // Features
        public bool ShowProperties(Icon icon) => false;

        public TextEntry NewEntry() => new MsbtEntry();

        public bool AddEntry(TextEntry entry)
        {
            bool result = true;

            try
            {
                MsbtEntry ent = (MsbtEntry)entry;
                ent.EditedLabel = _msbt.AddLabel(entry.Name);
                _entries.Add((MsbtEntry)entry);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool RenameEntry(TextEntry entry, string newName)
        {
            bool result = true;

            try
            {
                MsbtEntry ent = (MsbtEntry)entry;
                _msbt.RenameLabel(ent.EditedLabel, newName);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool DeleteEntry(TextEntry entry)
        {
            bool result = true;

            try
            {
                MsbtEntry ent = (MsbtEntry)entry;
                _msbt.RemoveLabel(ent.EditedLabel);
                _entries.Remove(ent);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool ShowEntryProperties(TextEntry entry, Icon icon) => false;

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

    public sealed class MsbtEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return EditedLabel.Name; }
            set { EditedLabel.Name = value; }
        }

        public string OriginalText => OriginalLabel.Text;

        public string EditedText
        {
            get { return EditedLabel.Text; }
            set { EditedLabel.Text = value; }
        }

        public int MaxLength { get; set; }

        public TextEntry ParentEntry { get; set; }

        public bool IsSubEntry => ParentEntry != null;

        public bool HasText { get; }

        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Label OriginalLabel { get; }
        public Label EditedLabel { get; set; }

        public MsbtEntry()
        {
            OriginalLabel = new Label();
            EditedLabel = new Label();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public MsbtEntry(Label editedLabel) : this()
        {
            if (editedLabel != null)
                EditedLabel = editedLabel;
        }

        public MsbtEntry(Label editedLabel, Label originalLabel) : this(editedLabel)
        {
            if (originalLabel != null)
                OriginalLabel = originalLabel;
        }

        public override string ToString()
        {
            return Name == string.Empty ? EditedLabel.Index.ToString() : Name;
        }
    }
}