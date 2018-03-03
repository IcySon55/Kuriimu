using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using text_gmd.Properties;
using Kontract.Interface;
using Kontract;
using Kontract.IO;

namespace text_gmd
{
    public class GmdAdapter : ITextAdapter
    {
        private GMD _gmd;
        private GMD _gmdBackup;
        private List<Entry> _entries;

        #region Properties

        // Information
        public string Name => "GMD";
        public string Description => "Game Message Data";
        public string Extension => "*.gmd";
        public string About => "This is the GMD text adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanAddEntries => false;
        public bool CanRenameEntries => true;
        public bool CanDeleteEntries => false;
        public bool CanSortEntries => false;
        public bool EntriesHaveSubEntries => false;
        public bool EntriesHaveUniqueNames => true;
        public bool EntriesHaveExtendedProperties => false;

        public FileInfo FileInfo { get; set; }

        public string LineEndings => "\r\n";

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                var magic = br.ReadString(4);
                return magic == "GMD" || magic == "\0DMG";
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _gmd = new GMD(FileInfo.OpenRead());

                var backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                    _gmdBackup = new GMD(File.OpenRead(backupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _gmdBackup = new GMD(File.OpenRead(backupFilePath));
                }
                else
                    _gmdBackup = null;
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
                _gmd.Save(FileInfo.Create());
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

                    foreach (Label label in _gmd.Labels)
                    {
                        if (_gmdBackup == null)
                            _entries.Add(new Entry(label));
                        else
                            _entries.Add(new Entry(label, _gmdBackup.Labels.FirstOrDefault(o => o.TextID == label.TextID)));
                    }
                }

                if (SortEntries)
                    return _entries.OrderBy(e => e.EditedLabel.TextID);

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
        
        public bool RenameEntry(TextEntry entry, string newName)
        {
            bool result = true;

            try
            {
                Entry ent = (Entry)entry;
                _gmd.RenameLabel(ent.EditedLabel.TextID, newName);
                
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

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
