using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kontract.Interface;
using Kontract;

namespace text_heroes
{
    public sealed class CsvAdapter : ITextAdapter
    {
        private CSV _format = null;
        private CSV _formatBackup = null;
        private List<CsvEntry> _entries = null;

        #region Properties

        // Information
        public string Name => "CSV";
        public string Description => "DBZ Heroes CSV";
        public string Extension => "*.csv";
        public string About => "This is the DBZ Heroes CSV text adapter for Kuriimu.";

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

        public string LineEndings => "\n";

        #endregion

        public bool Identify(string filename)
        {
            return false; // TODO: Identify the file
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _format = new CSV(FileInfo.OpenRead());

                var namBackupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(namBackupFilePath))
                    _formatBackup = new CSV(File.OpenRead(namBackupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, namBackupFilePath);
                    _formatBackup = new CSV(File.OpenRead(namBackupFilePath));
                }
                else
                    _formatBackup = null;
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
                _format.Save(FileInfo.Create());
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
                    if (_formatBackup == null)
                        _entries = _format.Entries.Select(o => new CsvEntry(o)).ToList();
                    else
                        _entries = _format.Entries.Select(o => new CsvEntry(o, _formatBackup.Entries.FirstOrDefault(b => b.Index == o.Index))).ToList();
                }

                return _entries;
            }
        }

        public IEnumerable<string> NameList => Entries?.Select(o => o.Name);
        public string NameFilter => @"*";
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

    public sealed class CsvEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return Message.Index.ToString("0000"); }
            set { }
        }

        public string OriginalText => OriginalMessage?.Text ?? string.Empty;

        public string EditedText
        {
            get => Message.Text;
            set => Message.Text = value;
        }

        public int MaxLength { get; }
        public TextEntry ParentEntry { get; set; }
        public bool IsSubEntry => ParentEntry != null;
        public bool HasText { get; }
        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Entry Message { get; set; }
        public Entry OriginalMessage { get; }

        public CsvEntry()
        {
            Name = string.Empty;
            ParentEntry = null;
            HasText = true;
        }

        public CsvEntry(Entry message) : this()
        {
            Message = message;
        }

        public CsvEntry(Entry message, Entry originalMessage) : this(message)
        {
            OriginalMessage = originalMessage;
        }

        public override string ToString() => Name;

        public int CompareTo(TextEntry rhs)
        {
            var result = Name.CompareTo(rhs.Name);
            return result;
        }
    }
}