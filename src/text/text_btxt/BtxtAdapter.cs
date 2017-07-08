using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using text_btxt.Properties;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace text_btxt
{
    class BtxtAdapter : ITextAdapter
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

        public string About => "This is the BTXT text adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => false;

        public bool CanSave => false;

        public bool CanAddEntries => false;

        public bool CanRenameEntries => false;

        public bool CanDeleteEntries => false;

        public bool CanSortEntries => false;

        public bool EntriesHaveSubEntries => true;

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
                return br.ReadBytes(8).SequenceEqual(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x24, 0x10, 0x12, 0xFF });
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);
            _entries = null;

            if (_fileInfo.Exists)
            {
                _btxt = new BTXT(_fileInfo.FullName);

                string backupFilePath = _fileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                {
                    _btxtBackup = new BTXT(backupFilePath);
                }
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(_fileInfo.FullName, backupFilePath);
                    _btxtBackup = new BTXT(backupFilePath);
                }
                else
                {
                    _btxtBackup = null;
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

        // Entries
        public IEnumerable<TextEntry> Entries
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
                                subEntry.ParentEntry = entry;
                                entry.SubEntries.Add(subEntry);
                            }
                            else
                            {
                                Entry subEntry = new Entry(str, _btxtBackup.Labels.FirstOrDefault(o => o.Name == label.Name).Strings.FirstOrDefault(j => j.ID == str.ID));
                                subEntry.ParentEntry = entry;
                                entry.SubEntries.Add(subEntry);
                            }
                        }
                    }
                }

                if (SortEntries)
                    return _entries.OrderBy(e => e.Name);

                return _entries;
            }
        }

        public IEnumerable<string> NameList => Entries?.Select(o => o.Name);

        public string NameFilter => @".*";

        public int NameMaxLength => 0;

        // Features
        public bool ShowProperties(Icon icon) => false;

        public TextEntry NewEntry() => new Entry();

        public bool AddEntry(TextEntry entry) => false;

        public bool RenameEntry(TextEntry entry, string newName) => false;

        public bool DeleteEntry(TextEntry entry) => false;

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

    public sealed class Entry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return IsSubEntry ? EditedString.ID.ToString() : Label.Name; }
            set { }
        }

        public string OriginalText => OriginalString.Text;

        public string EditedText
        {
            get { return EditedString.Text; }
            set { EditedString.Text = value; }
        }

        public int MaxLength { get; set; }

        public TextEntry ParentEntry { get; set; }

        public bool IsSubEntry => ParentEntry != null;

        public bool HasText { get; }

        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Label Label { get; set; }
        public String OriginalString { get; }
        public String EditedString { get; set; }

        public Entry()
        {
            Label = new Label();
            OriginalString = new String();
            EditedString = new String();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = false;
            SubEntries = new List<TextEntry>();
        }

        public Entry(Label lbl) : this()
        {
            Label = lbl;
        }

        public Entry(String editedString) : this()
        {
            if (editedString != null)
                EditedString = editedString;
            HasText = true;
        }

        public Entry(String editedString, String originalString) : this(editedString)
        {
            if (originalString != null)
                OriginalString = originalString;
        }

        public override string ToString()
        {
            return IsSubEntry ? EditedString.ID.ToString() : Name;
        }

        public int CompareTo(TextEntry rhs)
        {
            int result = Name.CompareTo(rhs.Name);
            return result;
        }
    }
}