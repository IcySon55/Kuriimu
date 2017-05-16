using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using text_kup.Properties;
using Kuriimu.Contract;
using System.Linq;

namespace text_kup
{
    public class KupAdatper : ITextAdapter
    {
        private KUP _kup = null;

        #region Properties

        // Information
        public string Name => "KUP";
        public string Description => "Kuriimu Archive";
        public string Extension => "*.kup";
        public string About => "This is the KUP file adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => true;
        public bool CanSave => true;
        public bool CanAddEntries => false;
        public bool CanRenameEntries => true;
        public bool CanDeleteEntries => false;
        public bool CanSortEntries => true;
        public bool EntriesHaveSubEntries => false;
        public bool EntriesHaveUniqueNames => false;
        public bool EntriesHaveExtendedProperties => true;

        public FileInfo FileInfo { get; set; }

        public string LineEndings => "\n";

        #endregion

        public bool Identify(string filename)
        {
            bool result = true;

            try
            {
                KUP.Load(filename);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _kup = KUP.Load(FileInfo.FullName);
                
                foreach (Entry entry in _kup.Entries)
                    entry.PointerCleanup();
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
                _kup.Save(FileInfo.FullName);
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
                if (SortEntries)
                    return _kup.Entries.OrderBy(e => e.Name).ThenBy(e => e.Offset);
                return _kup.Entries;
            }
        }

        public IEnumerable<string> NameList => null;
        public string NameFilter => @".*";
        public int NameMaxLength => 0;

        // Features
        public bool ShowProperties(Icon icon)
        {
            FileProperties properties = new FileProperties(_kup, icon);
            properties.ShowDialog();
            return properties.HasChanges;
        }

        public TextEntry NewEntry() => new Entry();

        public bool AddEntry(TextEntry entry)
        {
            bool result = true;

            try
            {
                _kup.Entries.Add((Entry)entry);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool RenameEntry(TextEntry entry, string name)
        {
            bool result = true;

            try
            {
                entry.Name = name;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool DeleteEntry(TextEntry entry)
        {
            bool result = true;

            try
            {
                _kup.Entries.Remove((Entry)entry);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool ShowEntryProperties(TextEntry entry, Icon icon)
        {
            EntryProperties entryProperties = new EntryProperties((Entry)entry, icon);
            entryProperties.ShowDialog();
            return entryProperties.HasChanges;
        }

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