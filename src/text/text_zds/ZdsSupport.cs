using System;
using Kuriimu.Kontract;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace text_zds
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
            return Name == string.Empty ? "!NoName!" : Name;
        }
    }
    #endregion

    #region Label_Definition
    public sealed class Label
    {
        public string Name;
        public string Text;
        public int TextID;
        public TextClass textEntry;

        public Label()
        {
            Name = string.Empty;
            Text = string.Empty;
            TextID = 0;
        }
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PartitionHeader
    {
        public Magic8 magic;
        public uint zero1;
        public uint unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public uint entryCount;
        public uint unk1;
        public uint headerSize;
        public uint entryOffset;
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextClass
    {
        public uint offset1;
        public uint offset2;
        public uint unk1;
        public uint unk2;
    }
}