using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using text_msbt.Properties;
using Kontract.Interface;
using Kontract;
using Kontract.IO;

namespace text_msbt
{
    //Nintendo

    public sealed class MsbtAdapter : ITextAdapter
    {
        private MSBT _msbt;
        private MSBT _msbtBackup;
        private List<MsbtEntry> _entries;

        #region Properties

        // Information
        public string Name => "MSBT";
        public string Description => "Message Binary Text";
        public string Extension => "*.msbt";
        public string About => "This is the MSBT text adapter for Kuriimu.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanAddEntries => true;
        public bool CanRenameEntries => true;
        public bool CanDeleteEntries => true;
        public bool CanSortEntries => true;
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
                if (br.BaseStream.Length < 8) return false;
                return br.ReadString(8) == "MsgStdBn";
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            var result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _msbt = new MSBT(FileInfo.OpenRead());

                var backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                    _msbtBackup = new MSBT(File.OpenRead(backupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _msbtBackup = new MSBT(File.OpenRead(backupFilePath));
                }
                else
                    _msbtBackup = null;
            }
            else
                result = LoadResult.FileNotFound;

            return result;
        }

        public SaveResult Save(string filename = "")
        {
            var result = SaveResult.Success;

            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _msbt.Save(FileInfo.Create());
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
                    if (_msbtBackup == null)
                    {
                        if (_msbt.HasLabels)
                            _entries = _msbt.LBL1.Labels.OrderBy(o => o.Index).Select(o => new MsbtEntry(o)).ToList();
                        else if (_msbt.HasIDs)
                            _entries = _msbt.TXT2.Strings.OrderBy(o => o.Index).Select(o => new MsbtEntry(new Label { Index = o.Index, String = o, Name = _msbt.NLI1.GlobalIDs[o.Index].ToString() })).ToList();
                        else
                            _entries = _msbt.TXT2.Strings.OrderBy(o => o.Index).Select(o => new MsbtEntry(new Label { Index = o.Index, String = o })).ToList();
                    }
                    else
                    {
                        if (_msbt.HasLabels)
                            _entries = _msbt.LBL1.Labels.OrderBy(o => o.Index).Select(o => new MsbtEntry(o, _msbtBackup.LBL1.Labels.FirstOrDefault(b => b.Name == o.Name))).ToList();
                        else if (_msbt.HasIDs)
                        {
                            _entries = _msbt.TXT2.Strings.OrderBy(o => o.Index).Select(o =>
                            {
                                var originalString = _msbtBackup.TXT2.Strings.FirstOrDefault(b => b.Index == o.Index);
                                return new MsbtEntry(new Label { Index = o.Index, String = o, Name = _msbt.NLI1.GlobalIDs[o.Index].ToString() }, new Label { Index = originalString?.Index ?? 0, String = originalString, Name = _msbt.NLI1.GlobalIDs[originalString?.Index ?? 0].ToString() });
                            }).ToList();
                        }
                        else
                        {
                            _entries = _msbt.TXT2.Strings.OrderBy(o => o.Index).Select(o =>
                            {
                                var originalString = _msbtBackup.TXT2.Strings.FirstOrDefault(b => b.Index == o.Index);
                                return new MsbtEntry(new Label { Index = o.Index, String = o }, new Label { Index = originalString?.Index ?? 0, String = originalString });
                            }).ToList();
                        }
                    }
                }

                if (SortEntries)
                    return _entries.OrderBy(e => e.Name).ThenBy(e => e.EditedLabel.Index);

                return _entries;
            }
        }

        public IEnumerable<string> NameList => Entries?.Select(o => o.Name);
        public string NameFilter => MSBT.LabelFilter;
        public int NameMaxLength => MSBT.LabelMaxLength;

        // Features
        public bool ShowProperties(Icon icon) => false;

        public TextEntry NewEntry() => new MsbtEntry();

        public bool AddEntry(TextEntry entry)
        {
            var result = true;

            try
            {
                var ent = (MsbtEntry)entry;
                ent.EditedLabel = _msbt.AddLabel(entry.Name);
                _entries.Add((MsbtEntry)entry);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool RenameEntry(TextEntry entry, string newName)
        {
            var result = true;

            try
            {
                var ent = (MsbtEntry)entry;
                _msbt.RenameLabel(ent.EditedLabel, newName);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool DeleteEntry(TextEntry entry)
        {
            var result = true;

            try
            {
                var ent = (MsbtEntry)entry;
                _msbt.RemoveLabel(ent.EditedLabel);
                _entries.Remove(ent);
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

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

    public sealed class MsbtEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return EditedLabel.Name; }
            set { EditedLabel.Name = value; }
        }

        public string OriginalText => OriginalLabel.Text;

        public string EditedText
        {
            get { return EditedLabel.Text; }
            set { EditedLabel.Text = value; }
        }

        public int MaxLength { get; set; }

        public TextEntry ParentEntry { get; set; }

        public bool IsSubEntry => ParentEntry != null;

        public bool HasText { get; }

        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Label OriginalLabel { get; }
        public Label EditedLabel { get; set; }

        public MsbtEntry()
        {
            OriginalLabel = new Label();
            EditedLabel = new Label();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public MsbtEntry(Label editedLabel) : this()
        {
            if (editedLabel != null)
                EditedLabel = editedLabel;
        }

        public MsbtEntry(Label editedLabel, Label originalLabel) : this(editedLabel)
        {
            if (originalLabel != null)
                OriginalLabel = originalLabel;
        }

        public override string ToString()
        {
            return Name == string.Empty ? EditedLabel.Index.ToString() : Name;
        }
    }
}