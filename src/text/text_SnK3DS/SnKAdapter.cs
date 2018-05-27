using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using text_SnK3DS.Properties;
using Kontract.Interface;
using Kontract;
using Kontract.IO;

namespace text_SnK3DS
{
    //Koei

    public sealed class SnKAdapter : ITextAdapter
    {
        private FileInfo _fileInfo = null;
        private SnK _snk = null;
        private SnK _snkBackup = null;
        private List<Entry> _entries = null;

        #region Properties

        // Information
        public string Name => "SnK Text";
        public string Description => "Text for Shingeki no Kyojin: Shichi kara no Dasshutsu 3DS";
        public string Extension => "*.bin";
        public string About => "This is the SnK text adapter for Kuriimu.";

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
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length == 0) return false;

                var bytes = br.ReadAllBytes();
                var index = bytes.ToList().FindIndex(b => b == 0x04);
                if (index == -1) return false;
                if (index + 3 >= br.BaseStream.Length) return false;

                br.BaseStream.Position = index + 1;
                var size = br.ReadUInt16();
                if (br.BaseStream.Position + size + 1 >= br.BaseStream.Length) return false;
                br.BaseStream.Position += size - 1;

                var check1 = br.ReadByte();
                var check2 = br.ReadByte();

                return (check1 == 0 && check2 == 0x1e);
            }
        }

        //Load file, make backup if needed
        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);
            _entries = null;

            if (_fileInfo.Exists)
            {
                _snk = new SnK(_fileInfo.FullName);

                string backupFilePath = _fileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                {
                    _snkBackup = new SnK(backupFilePath);
                }
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(_fileInfo.FullName, backupFilePath);
                    _snkBackup = new SnK(backupFilePath);
                }
                else
                {
                    _snkBackup = null;
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
                _snk.Save(_fileInfo.FullName);
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

                    foreach (Label label in _snk.Labels)
                    {
                        if (_snkBackup == null)
                        {
                            Entry entry = new Entry(label);
                            _entries.Add(entry);
                        }
                        else
                        {
                            Entry entry = new Entry(label, _snkBackup.Labels.FirstOrDefault(o => o.TextID == label.TextID));
                            _entries.Add(entry);
                        }
                    }
                }

                if (SortEntries)
                    return _entries.OrderBy(e => e.Name).ThenBy(e => e.EditedLabel.TextID);

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
}
