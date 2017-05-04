using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Contract;
using Kuriimu.IO;
using text_bmd.Properties;

namespace text_bmd
{
    class BmdAdapter : ITextAdapter
    {
        private BMD _bmd = null;
        private BMD _bmdBackup = null;
        private List<Entry> _entries = null;

        #region Properties

        // Information
        public string Name => "BMD";
        public string Description => "Battle Message Data";
        public string Extension => "*.bmd";
        public string About => "This is the BMD file adapter for Kuriimu.";

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

        public FileInfo FileInfo { get; set; }

        public string LineEndings => "\n";

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 8) return false;
                return br.ReadString(4) == "MLTB";
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _bmd = new BMD(FileInfo.OpenRead());

                var backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                {
                    _bmdBackup = new BMD(File.OpenRead(backupFilePath));
                }
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _bmdBackup = new BMD(File.OpenRead(backupFilePath));
                }
                else
                {
                    _bmdBackup = null;
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
                FileInfo = new FileInfo(filename);

            try
            {
                _bmd.Save(FileInfo.OpenWrite());
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

                    //foreach (Label label in _bmd.Labels)
                    //{
                    //    Entry entry = new Entry(label);
                    //    _entries.Add(entry);

                    //    foreach (String str in label.Strings)
                    //    {
                    //        if (_bmdBackup == null)
                    //        {
                    //            Entry subEntry = new Entry(str);
                    //            subEntry.ParentEntry = entry;
                    //            entry.SubEntries.Add(subEntry);
                    //        }
                    //        else
                    //        {
                    //            Entry subEntry = new Entry(str, _bmdBackup.Labels.FirstOrDefault(o => o.Name == label.Name).Strings.FirstOrDefault(j => j.ID == str.ID));
                    //            subEntry.ParentEntry = entry;
                    //            entry.SubEntries.Add(subEntry);
                    //        }
                    //    }
                    //}
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
            get { return ""; }
            set { }
        }

        public string OriginalText => OriginalSpeakerText.Text;

        public string EditedText
        {
            get => SpeakerText.Text;
            set => SpeakerText.Text = value;
        }

        public int MaxLength { get; set; }
        public TextEntry ParentEntry { get; set; }
        public bool IsSubEntry => ParentEntry != null;
        public bool HasText { get; }
        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Speaker SpeakerText { get; set; }
        public Speaker OriginalSpeakerText { get; }

        public Entry()
        {
            Name = string.Empty;
            MaxLength = BMD.MessageLength;
            ParentEntry = null;
            HasText = false;
            SubEntries = new List<TextEntry>();
        }

        public Entry(Label lbl) : this()
        {
            //Label = lbl;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(TextEntry rhs)
        {
            var result = Name.CompareTo(rhs.Name);
            return result;
        }
    }

    //public sealed class MessageEntry : TextEntry
    //{
    //    // Interface
    //    public string Name
    //    {
    //        get => Message.Name;
    //        set { }
    //    }

    //    public string OriginalText => OriginalMessage.Text;

    //    // Adapter
    //    public Message Message { get; set; }
    //    public Message OriginalMessage { get; }
    //}
}
