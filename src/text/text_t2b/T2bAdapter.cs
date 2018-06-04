using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using text_t2b.Properties;
using Kontract.Interface;
using Kontract;
using Kontract.IO;

namespace text_t2b
{
    //Level5

    public sealed class XsAdapter : ITextAdapter
    {
        private FileInfo _fileInfo = null;
        private T2B _t2b = null;
        private T2B _t2bBackup = null;
        private List<Entry> _entries = null;

        #region Properties

        // Information
        public string Name => "T2B";

        public string Description => "Level5 Binary Text";

        public string Extension => "*.cfg.bin";

        public string About => "This is the T2B text adapter for Kuriimu.";

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
                br.BaseStream.Position = br.BaseStream.Length - 0xf;
                return br.PeekString((uint)br.BaseStream.Length - 0xf, 3) == "t2b";
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
                _t2b = new T2B(_fileInfo.FullName);

                string backupFilePath = _fileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                {
                    _t2bBackup = new T2B(backupFilePath);
                }
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(_fileInfo.FullName, backupFilePath);
                    _t2bBackup = new T2B(backupFilePath);
                }
                else
                {
                    _t2bBackup = null;
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
                _t2b.Save(_fileInfo.FullName);
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

                    foreach (Label label in _t2b.Labels)
                    {
                        if (_t2bBackup == null)
                        {
                            Entry entry = new Entry(label);
                            _entries.Add(entry);
                        }
                        else
                        {
                            Entry entry = new Entry(label, _t2bBackup.Labels.FirstOrDefault(o => o.TextID == label.TextID));
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
