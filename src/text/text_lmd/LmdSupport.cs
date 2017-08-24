using System;
using Kuriimu.Kontract;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace text_lmd
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
        public uint offset;

        public Label()
        {
            Name = string.Empty;
            Text = string.Empty;
            TextID = 0;
            offset = 0;
        }
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public uint const1;
        public uint entryCount;
        public uint sec2Size;
        public uint textCount;
        public uint entryOffset;
        public uint sec2Offset;
        public uint textOffset;
        public uint fileNameOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Sec1Entry
    {
        public uint hash;
        public ushort offset;
        public ushort unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextClass
    {
        public uint offset;
        public uint size1;
        public uint size2;
    }
}