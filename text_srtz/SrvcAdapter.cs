using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Contract;
using Kuriimu.IO;
using text_srtz.Properties;

namespace text_srtz
{
    public class SrvcAdapter : ITextAdapter
    {
        private SRVC _srvc = null;
        private SRVC _srvcBackup = null;
        private List<SrvcEntry> _entries = null;

        #region Properties

        // Information
        public string Name => "SRVC";
        public string Description => "Super Robot Wars Z Battle Scripts";
        public string Extension => "*.bin";
        public string About => "This is the SRVC file adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanAddEntries => false;
        public bool CanRenameEntries => false;
        public bool CanDeleteEntries => false;
        public bool CanSortEntries => false;
        public bool EntriesHaveSubEntries => false;
        public bool EntriesHaveUniqueNames => true;
        public bool EntriesHaveExtendedProperties => false;

        public FileInfo FileInfo { get; set; }

        public string LineEndings => "\x5c\x6e";

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.BaseStream.Length >= 0x140 && br.ReadUInt16() == 0x4F00;
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _srvc = new SRVC(FileInfo.OpenRead());

                var backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                    _srvcBackup = new SRVC(File.OpenRead(backupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _srvcBackup = new SRVC(File.OpenRead(backupFilePath));
                }
                else
                    _srvcBackup = null;
            }
            else
                result = LoadResult.FileNotFound;

            return result;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _srvc.Save(FileInfo.Create());
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
                    _entries = new List<SrvcEntry>();

                    // Text
                    _entries.AddRange(_srvcBackup == null ? _srvc.Entries.Select(t => new SrvcEntry(t)) : _srvc.Entries.Select(t => new SrvcEntry(t, _srvcBackup.Entries.Find(o => o.ID == t.ID))));
                }

                return _entries;
            }
        }

        public IEnumerable<string> NameList => Entries?.Select(o => o.Name);
        public string NameFilter => @".*";
        public int NameMaxLength => 0;

        // Features
        public bool ShowProperties(Icon icon) => false;
        public TextEntry NewEntry() => null;
        public bool AddEntry(TextEntry entry) => false;
        public bool RenameEntry(TextEntry entry, string newName) => false;
        public bool DeleteEntry(TextEntry entry) => false;
        public bool ShowEntryProperties(TextEntry entry, Icon icon) => false;

        // Settings
        public bool SortEntries
        {
            get => false;
            set => value = false;
        }
    }

    public sealed class SrvcEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return Entry.ID.ToString(); }
            set { }
        }

        public string OriginalText => OriginalEntry?.Text ?? string.Empty;

        public string EditedText
        {
            get => Entry.Text;
            set => Entry.Text = value;
        }

        public int MaxLength { get; }
        public TextEntry ParentEntry { get; set; }
        public bool IsSubEntry => ParentEntry != null;
        public bool HasText { get; }
        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Entry Entry { get; set; }
        public Entry OriginalEntry { get; }

        public SrvcEntry()
        {
            Name = string.Empty;
            ParentEntry = null;
            HasText = true;
        }

        public SrvcEntry(Entry message) : this()
        {
            Entry = message;
        }

        public SrvcEntry(Entry message, Entry originalMessage) : this(message)
        {
            OriginalEntry = originalMessage;
        }

        public override string ToString() => Name;

        public int CompareTo(TextEntry rhs)
        {
            var result = Name.CompareTo(rhs.Name);
            return result;
        }
    }
}
