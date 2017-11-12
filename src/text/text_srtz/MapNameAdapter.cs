using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kontract.Interface;
using Kontract;
using Kontract.IO;

namespace text_srtz
{
    public class MapNameAdapter : ITextAdapter
    {
        private MAPNAME _mapName = null;
        private MAPNAME _mapNameBackup = null;
        private List<MapNameEntry> _entries = null;

        #region Properties

        // Information
        public string Name => "MAPNAME";
        public string Description => "Super Robot Wars Z Map Names";
        public string Extension => "*.bin";
        public string About => "This is the MAPNAME text adapter for Kuriimu.";

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

        public string LineEndings => @"\n";

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.BaseStream.Length == 0xC300; // This format has no identifiers
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _mapName = new MAPNAME(FileInfo.OpenRead());

                var backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                    _mapNameBackup = new MAPNAME(File.OpenRead(backupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _mapNameBackup = new MAPNAME(File.OpenRead(backupFilePath));
                }
                else
                    _mapNameBackup = null;
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
                _mapName.Save(FileInfo.Create());
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
                    _entries = new List<MapNameEntry>();

                    // Messages
                    _entries.AddRange(_mapNameBackup == null ? _mapName.MapNames.Select(m => new MapNameEntry(m)) : _mapName.MapNames.Select(m => new MapNameEntry(m, _mapNameBackup.MapNames.Find(o => o.Index == m.Index))));
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

    public sealed class MapNameEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return Message.Index.ToString("000"); }
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
        public MapName Message { get; set; }
        public MapName OriginalMessage { get; }

        public MapNameEntry()
        {
            Name = string.Empty;
            MaxLength = MAPNAME.MessageLength;
            ParentEntry = null;
            HasText = true;
        }

        public MapNameEntry(MapName message) : this()
        {
            Message = message;
        }

        public MapNameEntry(MapName message, MapName originalMessage) : this(message)
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
