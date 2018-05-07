using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Interface;

namespace text_smas
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Header
    {
        public Magic Magic; // SMAS
        public int EntryCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Entry
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string Label;
        public int Unknown1;
        public int Offset;
    }

    public sealed class String
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }

        public String()
        {
            Text = string.Empty;
            Index = 0;
        }
    }

    public sealed class SmasEntry : TextEntry
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
        public String OriginalLabel { get; }
        public String EditedLabel { get; set; }

        public SmasEntry()
        {
            OriginalLabel = new String();
            EditedLabel = new String();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public SmasEntry(String editedLabel) : this()
        {
            if (editedLabel != null)
                EditedLabel = editedLabel;
        }

        public SmasEntry(String editedLabel, String originalLabel) : this(editedLabel)
        {
            if (originalLabel != null)
                OriginalLabel = originalLabel;
        }

        public override string ToString()
        {
            return Name == string.Empty ? EditedLabel.Index.ToString() : Name;
        }
    }

    public sealed class InvalidSMASException : Exception
    {
        public InvalidSMASException(string message) : base(message) { }
    }
}