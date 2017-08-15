using System.Collections.Generic;
using Kuriimu.Kontract;
using System.Runtime.InteropServices;

namespace text_t2b
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
        public uint relOffset;

        public Label()
        {
            Name = string.Empty;
            Text = string.Empty;
            TextID = 0;
            relOffset = 0;
        }
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public int entryCount;
        public int stringSecOffset;
        public int stringSecSize;
        public int stringCount;
    }

    public class StringEntry
    {
        public uint entryTypeID;
        public byte entryLength;
        public byte[] typeMask;
        public List<TypeEntry> data = new List<TypeEntry>();

        public class TypeEntry
        {
            //0 - string offset
            //1 - integer
            //2 - float
            public byte type;
            public uint value;
        }
    }
}
