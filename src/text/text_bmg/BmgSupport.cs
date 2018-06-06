using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace text_bmg
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
            return Name == string.Empty ? "" : Name;
        }
    }
    #endregion

    #region Label_Definition
    public sealed class Label
    {
        public string Name;
        public string Text;
        public int TextID;

        public Label()
        {
            Name = string.Empty;
            Text = string.Empty;
            TextID = 0;
        }
    }
    #endregion


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MESGHeader
    {
        public Magic8 magic;
        public int fileSize;
        public int sectionCount;
        public int unk1;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TypeTable
    {
        public int textSecOffset;
        public short ttSectionsNumber;
        public List<TableEntry> entries;
    }

    public class TableEntry
    {
        public short ttSectionID;
        public int ttSectionPos;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BFEntry
    {
        public int ID;
        public int entryID;
        public int stringSize;
        public int stringOffset;
        public int padding;
    }
}