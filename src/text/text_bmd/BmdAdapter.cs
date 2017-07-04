using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kuriimu.Kontract;
using Kuriimu.IO;
using text_bmd.Properties;

namespace text_bmd
{
    class BmdAdapter : ITextAdapter
    {
        private BMD _bmd = null;
        private BMD _bmdBackup = null;
        private List<Heading> _entries = null;

        #region Properties

        // Information
        public string Name => "BMD";
        public string Description => "Battle Message Data";
        public string Extension => "*.bmd";
        public string About => "This is the BMD file adapter for Kuriimu.";

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

        public string LineEndings => "\n";

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 8) return false;
                return br.ReadString(4) == "MLTB";
            }
        }

        public LoadResult Load(string filename, bool autoBackup = false)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);
            _entries = null;

            if (FileInfo.Exists)
            {
                _bmd = new BMD(FileInfo.OpenRead());

                var backupFilePath = FileInfo.FullName + ".bak";
                if (File.Exists(backupFilePath))
                    _bmdBackup = new BMD(File.OpenRead(backupFilePath));
                else if (autoBackup || MessageBox.Show("Would you like to create a backup of " + FileInfo.Name + "?\r\nA backup allows the Original text box to display the source text before edits were made.", "Create Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(FileInfo.FullName, backupFilePath);
                    _bmdBackup = new BMD(File.OpenRead(backupFilePath));
                }
                else
                    _bmdBackup = null;
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
                _bmd.Save(FileInfo.Create());
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
                    _entries = new List<Heading>();

                    // Speakers
                    var speakerHeading = new Heading("Speakers");
                    _entries.Add(speakerHeading);
                    speakerHeading.SubEntries.AddRange(_bmd.Speakers.Select(s => new SpeakerEntry(s, _bmdBackup?.Speakers.FirstOrDefault(sb => sb.Name == s.Name))));

                    // Messages
                    var messageHeading = new Heading("Messages");
                    _entries.Add(messageHeading);
                    messageHeading.SubEntries.AddRange(_bmd.Messages.Select(m => new MessageEntry(m, _bmdBackup?.Messages.FirstOrDefault(mb => mb.Name == m.Name))));
                }

                if (SortEntries)
                    return _entries.OrderBy(e => e.Name);

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
            get { return Settings.Default.SortEntries; }
            set
            {
                Settings.Default.SortEntries = value;
                Settings.Default.Save();
            }
        }
    }

    public sealed class Heading : TextEntry
    {
        // Interface
        public string Name { get; set; }
        public string OriginalText => string.Empty;
        public string EditedText { get; set; }
        public int MaxLength { get; }
        public TextEntry ParentEntry { get; set; }
        public bool IsSubEntry => ParentEntry != null;
        public bool HasText { get; }
        public List<TextEntry> SubEntries { get; set; }

        public Heading(string name)
        {
            Name = name;
            MaxLength = 0;
            ParentEntry = null;
            HasText = false;
            SubEntries = new List<TextEntry>();
        }

        public override string ToString() => Name;

        public int CompareTo(TextEntry rhs)
        {
            var result = Name.CompareTo(rhs.Name);
            return result;
        }
    }

    public sealed class SpeakerEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return Speaker.Name; }
            set { }
        }

        public string OriginalText => OriginalSpeaker?.Text ?? string.Empty;

        public string EditedText
        {
            get => Speaker.Text;
            set => Speaker.Text = value;
        }

        public int MaxLength { get; }
        public TextEntry ParentEntry { get; set; }
        public bool IsSubEntry => ParentEntry != null;
        public bool HasText { get; }
        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Speaker Speaker { get; set; }
        public Speaker OriginalSpeaker { get; }

        public SpeakerEntry()
        {
            Name = string.Empty;
            MaxLength = BMD.SpeakerLength;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public SpeakerEntry(Speaker speaker) : this()
        {
            Speaker = speaker;
        }

        public SpeakerEntry(Speaker speaker, Speaker originalSpeaker) : this(speaker)
        {
            OriginalSpeaker = originalSpeaker;
        }

        public override string ToString() => Name;

        public int CompareTo(TextEntry rhs)
        {
            var result = Name.CompareTo(rhs.Name);
            return result;
        }
    }

    public sealed class MessageEntry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return Message.Name; }
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
        public Message Message { get; set; }
        public Message OriginalMessage { get; }

        public MessageEntry()
        {
            Name = string.Empty;
            MaxLength = BMD.MessageLength;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public MessageEntry(Message message) : this()
        {
            Message = message;
        }

        public MessageEntry(Message message, Message originalMessage) : this(message)
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
