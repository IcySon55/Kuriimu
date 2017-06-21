using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace text_metal
{
    public sealed class NamAdapter : ITextAdapter
    {
        private NAM _nam = null;
        private NAM _namBackup = null;
        private List<NamEntry> _entries = null;

        #region Properties

        // Information
        public string Name => "NAM";
        public string Description => "MetalMax Name Text Binary";
        public string Extension => "*.nam";
        public string About => "This is the MetalMax NAM text adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;
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
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                var good = br.ReadBytes(2).SequenceEqual(new List<byte> { 0, 0 });
                br.BaseStream.Seek(-2, SeekOrigin.End);
                good &= br.ReadBytes(2).SequenceEqual(new List<byte> { 0, 0 });
                return good;
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _nam = new NAM(FileInfo.OpenRead());

                string backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                {
                    _namBackup = new NAM(File.OpenRead(backupFilePath));
                }
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _namBackup = new NAM(File.OpenRead(backupFilePath));
                }
                else
                {
                    _namBackup = null;
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
                _nam.Save(FileInfo.Create());
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
                    if (_namBackup == null)
                        _entries = _nam.Entries.Select(o => new NamEntry(o)).ToList();
                    else
                        _entries = _nam.Entries.Select(o => new NamEntry(o, _namBackup.Entries.FirstOrDefault(b => b == o))).ToList();
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

    public sealed class NamEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return Message; }
            set { }
        }

        public string OriginalText => OriginalMessage ?? string.Empty;

        public string EditedText
        {
            get => Message;
            set => Message = value;
        }

        public int MaxLength { get; }
        public TextEntry ParentEntry { get; set; }
        public bool IsSubEntry => ParentEntry != null;
        public bool HasText { get; }
        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public string Message { get; set; }
        public string OriginalMessage { get; }

        public NamEntry()
        {
            Name = string.Empty;
            ParentEntry = null;
            HasText = true;
        }

        public NamEntry(string message) : this()
        {
            Message = message;
        }

        public NamEntry(string message, string originalMessage) : this(message)
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