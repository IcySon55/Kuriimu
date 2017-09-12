﻿using System.Collections.Generic;
using Kuriimu.Kontract;
using System.Runtime.InteropServices;

namespace text_bmd.msg1
{
    #region Entry_Definition
    public sealed class Entry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return EditedLabel.Name; }
            set { }
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

        public Entry()
        {
            OriginalLabel = new Label();
            EditedLabel = new Label();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public Entry(Label editedLabel) : this()
        {
            if (editedLabel != null)
                EditedLabel = editedLabel;
        }

        public Entry(Label editedLabel, Label originalLabel) : this(editedLabel)
        {
            if (originalLabel != null)
                OriginalLabel = originalLabel;
        }

        public override string ToString()
        {
            return Name == string.Empty ? EditedLabel.TextOffset.ToString("X2") : Name;
        }
    }
    #endregion

    #region Label_Definition
    public sealed class Label
    {
        public string Name;

        public uint TextID;
        public uint TextOffset;
        public string Text;

        public Label()
        {
            Name = string.Empty;

            TextID = 0;
            TextOffset = 0;
            Text = string.Empty;
        }
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public int const1;
        public int fileSize;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string magic;
        public int endBlockOff;
        public int endBlockSize;
        public int entryCount;
        public int const2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntryTableEntry
    {
        public int type;
        public int offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextHeader
    {
        public int nameBlockOff;
        public int nameCount;
        public long zero1;
    }
}