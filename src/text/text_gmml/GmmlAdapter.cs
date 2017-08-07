﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using text_gmml.Properties;
using Kuriimu.Kontract;

namespace text_gmml
{
    public sealed class GmmlAdapter : ITextAdapter
    {
        private FileInfo _fileInfo = null;
        private GMML _gmml = null;
        private GMML _gmmlBackup = null;
        private List<Entry> _entries = null;

        #region Properties

        // Information
        public string Name => "GMML";

        public string Description => "Game Message Markup Language";

        public string Extension => "*.gmm;*.gmml";

        public string About => "This is the GMML text adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => false;

        public bool CanSave => true;

        public bool CanAddEntries => false;

        public bool CanRenameEntries => false;

        public bool CanDeleteEntries => false;

        public bool CanSortEntries => true;

        public bool EntriesHaveSubEntries => true;

        public bool EntriesHaveUniqueNames => false;

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
            bool result = true;

            try
            {
                GMML.Load(filename);
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

            _fileInfo = new FileInfo(filename);
            _entries = null;

            if (_fileInfo.Exists)
            {
                _gmml = GMML.Load(_fileInfo.FullName);

                string backupFilePath = _fileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                {
                    _gmmlBackup = GMML.Load(backupFilePath);
                }
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + _fileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(_fileInfo.FullName, backupFilePath);
                    _gmmlBackup = GMML.Load(backupFilePath);
                }
                else
                {
                    _gmmlBackup = null;
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
                _gmml.Save(_fileInfo.FullName);
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

                    foreach (Row row in _gmml.Body.Rows)
                    {
                        Entry entry = new Entry(row);
                        _entries.Add(entry);

                        foreach (Language lang in row.Languages)
                        {
                            if (_gmmlBackup == null)
                            {
                                Entry subEntry = new Entry(lang);
                                subEntry.ParentEntry = entry;
                                entry.SubEntries.Add(subEntry);
                            }
                            else
                            {
                                Entry subEntry = new Entry(lang, _gmmlBackup.Body.Rows.FirstOrDefault(o => o.ID == row.ID).Languages.FirstOrDefault(j => j.Name == lang.Name));
                                subEntry.ParentEntry = entry;
                                entry.SubEntries.Add(subEntry);
                            }
                        }
                    }
                }

                if (SortEntries)
                    return _entries.OrderBy(e => e.Name).ThenBy(e => e.Row.ID);

                return _entries;
            }
        }

        public IEnumerable<string> NameList => _gmml?.Body.Rows.Select(o => o.Comment);

        public string NameFilter => @".*";

        public int NameMaxLength => 0;

        // Features
        public bool ShowProperties(Icon icon) => false;

        public TextEntry NewEntry() => null;

        public bool AddEntry(TextEntry entry) => false;

        public bool RenameEntry(TextEntry entry, string name) => false;

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
            get { return IsSubEntry ? EditedLanguage.Name : string.Join(" - ", Row.ID, Row.Comment); }
            set { }
        }

        public string OriginalText => OriginalLanguage.Text;

        public string EditedText
        {
            get { return EditedLanguage.Text; }
            set { EditedLanguage.Text = value; }
        }

        public int MaxLength { get; set; }

        public TextEntry ParentEntry { get; set; }

        public bool IsSubEntry => ParentEntry != null;

        public bool HasText { get; }

        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Row Row { get; set; }
        public Language OriginalLanguage { get; set; }
        public Language EditedLanguage { get; set; }

        public Entry()
        {
            Row = new Row();
            OriginalLanguage = new Language();
            EditedLanguage = new Language();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = false;
            SubEntries = new List<TextEntry>();
        }

        public Entry(Row row) : this()
        {
            Row = row;
        }

        public Entry(Language editedLanguage) : this()
        {
            if (editedLanguage != null)
                EditedLanguage = editedLanguage;
            HasText = true;
        }

        public Entry(Language editedLanguage, Language originalLanguage) : this(editedLanguage)
        {
            if (originalLanguage != null)
                OriginalLanguage = originalLanguage;
        }

        public override string ToString()
        {
            return IsSubEntry ? EditedLanguage.Name : Name == string.Empty ? Row.ID : Name;
        }
    }
}