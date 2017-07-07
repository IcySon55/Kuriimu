using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace text_td3
{
    public class Td3Adapter : ITextAdapter
    {
        private TD3 _td3 = null;
        private TD3 _td3Backup = null;
        private List<Td3Entry> _entries = null;

        #region Properties

        // Information
        public string Name => "TD3";
        public string Description => "Compiled LUA 5.1 Scripts";
        public string Extension => "*.dat";
        public string About => "This is the TD3 text adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanAddEntries => false;
        public bool CanRenameEntries => false;
        public bool CanDeleteEntries => false;
        public bool CanSortEntries => false;
        public bool EntriesHaveSubEntries => true;
        public bool EntriesHaveUniqueNames => true;
        public bool EntriesHaveExtendedProperties => false;

        public FileInfo FileInfo { get; set; }

        public string LineEndings => @"\n";

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.BaseStream.Length > 8 && br.ReadString(5) == "\x1BLuaQ";
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _td3 = new TD3(FileInfo.OpenRead());

                var backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                    _td3Backup = new TD3(File.OpenRead(backupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _td3Backup = new TD3(File.OpenRead(backupFilePath));
                }
                else
                    _td3Backup = null;
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
                _td3.Save(FileInfo.Create());
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
                    _entries = new List<Td3Entry>();

                    foreach (var method in _td3.Entries)
                    {
                        Td3Entry entry = new Td3Entry(method);
                        _entries.Add(entry);

                        // Text
                        //foreach (var subMethod in method.Parameters)
                        //{
                        //    if (_td3Backup == null)
                        //    {
                        //        Td3Entry subEntry = new Td3Entry(subMethod);
                        //        subEntry.ParentEntry = entry;
                        //        subEntry.HasText = true;
                        //        entry.SubEntries.Add(subEntry);
                        //    }
                        //    else
                        //    {
                        //        Td3Entry subEntry = new Td3Entry(subMethod, _td3Backup.Entries.FirstOrDefault(o => o.ID == method.ID).Parameters.FirstOrDefault(j => j.ID == subMethod.ID));
                        //        subEntry.ParentEntry = entry;
                        //        entry.SubEntries.Add(subEntry);
                        //    }
                        //}
                    }

                    //_entries.AddRange(_td3Backup == null ? _td3.Entries.Where(t => t.Parameters.Count > 0).Select(t => new Td3Entry(t)) : _td3.Entries.Select(t => new Td3Entry(t, _td3Backup.Entries.Find(o => o.ID == t.ID))));
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

    public sealed class Td3Entry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return Method.ID; }
            set { }
        }

        public string OriginalText => OriginalMethod?.Content ?? string.Empty;

        public string EditedText
        {
            get => Method.Content;
            set => Method.Content = value;
        }

        public int MaxLength { get; }
        public TextEntry ParentEntry { get; set; }
        public bool IsSubEntry => ParentEntry != null;
        public bool HasText { get; set; }
        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Method Method { get; set; }
        public Method OriginalMethod { get; }

        public Td3Entry()
        {
            Name = string.Empty;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public Td3Entry(Method method) : this()
        {
            Method = method;
        }

        public Td3Entry(Method method, Method originalMethod) : this(method)
        {
            OriginalMethod = originalMethod;
        }

        public override string ToString() => Name;

        public int CompareTo(TextEntry rhs)
        {
            var result = Name.CompareTo(rhs.Name);
            return result;
        }
    }
}
