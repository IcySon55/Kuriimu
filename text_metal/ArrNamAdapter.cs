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
    public sealed class ArrNamAdapter : ITextAdapter
    {
        private ARRNAM _format = null;
        private ARRNAM _formatBackup = null;
        private List<ArrNamEntry> _entries = null;

        #region Properties

        // Information
        public string Name => "NAM";
        public string Description => "MetalMax Name Text Binary";
        public string Extension => "*.nam";
        public string About => "This is the MetalMax NAM text adapter for Kuriimu.";

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
            var arrFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".arr");

            if (!File.Exists(filename) || !File.Exists(arrFilename)) return false;

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
            var arrFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".arr");
            _entries = null;

            if (FileInfo.Exists)
            {
                _format = new ARRNAM(FileInfo.OpenRead(), File.OpenRead(arrFilename));

                var arrFileInfo = new FileInfo(arrFilename);
                var namBackupFilePath = FileInfo.FullName + ".bak";
                var arrBackupFilePath = arrFileInfo.FullName + ".bak";
                if (File.Exists(namBackupFilePath))
                    _formatBackup = new ARRNAM(File.OpenRead(namBackupFilePath), File.OpenRead(arrBackupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, namBackupFilePath);
                    File.Copy(arrFileInfo.FullName, arrBackupFilePath);
                    _formatBackup = new ARRNAM(File.OpenRead(namBackupFilePath), File.OpenRead(arrBackupFilePath));
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
            var arrFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".arr");

            if (filename.Trim() != string.Empty)
            {
                FileInfo = new FileInfo(filename);
                arrFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".ARR");
            }

            try
            {
                _format.Save(FileInfo.Create(), new FileInfo(arrFilename).Create());
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
                        _entries = _format.Entries.Select(o => new ArrNamEntry(o)).ToList();
                    else
                        _entries = _format.Entries.Select(o => new ArrNamEntry(o, _formatBackup.Entries.FirstOrDefault(b => b.Index == o.Index))).ToList();
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

    public sealed class ArrNamEntry : TextEntry
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

        public ArrNamEntry()
        {
            Name = string.Empty;
            ParentEntry = null;
            HasText = true;
        }

        public ArrNamEntry(Entry message) : this()
        {
            Message = message;
        }

        public ArrNamEntry(Entry message, Entry originalMessage) : this(message)
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