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
        private IGMD _gmd;
        private IGMD _gmdBackup;
        private Ident _ident;
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
            _ident = Support.Identify(filename);

            return _ident != Ident.NotFound && _ident != Ident.NotSupported;
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                var ident = Support.Identify(filename);
                switch (ident)
                {
                    case Ident.v1:
                        _gmd = new GMDv1();
                        _gmdBackup = new GMDv1();
                        break;

                    case Ident.v2:
                        _gmd = new GMDv2();
                        _gmdBackup = new GMDv2();
                        break;
                }
                _gmd.Load(filename);

                OpenOrCreateBackup(autoBackup);
            }
            else
                result = LoadResult.FileNotFound;

            return result;
        }

        private void OpenOrCreateBackup(bool autoBackup)
        {
            var backupFilePath = FileInfo.FullName + ".bak";
            if (File.Exists(backupFilePath))
            {
                _gmdBackup.Load(backupFilePath);
            }
            else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                File.Copy(FileInfo.FullName, backupFilePath);
                _gmdBackup.Load(backupFilePath);
            }
            else
                _gmdBackup = null;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _gmd.Save(FileInfo.FullName, (Platform)Enum.Parse(typeof(Platform), Settings.Default.Platform), (Game)Enum.Parse(typeof(Game), Settings.Default.Game));
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

                    foreach (Label label in _gmd.GMDContent.Content)
                    {
                        if (_gmdBackup == null)
                            _entries.Add(new Entry(label));
                        else
                            _entries.Add(new Entry(label, _gmdBackup.GMDContent.Content.FirstOrDefault(o => o.TextID == label.TextID)));
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
            var result = true;

            try
            {
                var ent = (Entry)entry;
                ent.Name = newName;
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
            get => Settings.Default.SortEntries;
            set
            {
                Settings.Default.SortEntries = value;
                Settings.Default.Save();
            }
        }
    }
}
