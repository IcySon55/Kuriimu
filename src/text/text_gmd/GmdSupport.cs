using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.Linq;

namespace text_gmd
{
    public enum Language : int
    {
        JAPANESE,
        ENGLISH,
        FRENCH,
        SPANISH,
        GERMAN,
        ITALIAN
    }

    public enum Versions : uint
    {
        Version1 = 0x00010201,
        Version2 = 0x00010302
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public Versions Version;
        public Language Language;
        public ulong Zero1;
        public uint LabelCount;
        public uint SectionCount;
        public uint LabelSize;
        public uint SectionSize;
        public uint NameSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class EntryV1
    {
        public uint ID;
        public uint Unknown1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class EntryV2
    {
        public uint ID;
        public uint Unknown1;
        public uint Unknown2;
        public uint LabelOffset; // Relative to labelDataOffset
        public uint Zero1;
    }

    public class XOR
    {
        private static string XOR1;
        private static string XOR2;

        public XOR(Versions version)
        {
            switch (version)
            {
                case Versions.Version1:
                    XOR1 = "fjfajfahajra;tira9tgujagjjgajgoa";
                    XOR2 = "mva;eignhpe/dfkfjgp295jtugkpejfu";
                    break;
                case Versions.Version2:
                    XOR1 = "e43bcc7fcab+a6c4ed22fcd433/9d2e6cb053fa462-463f3a446b19";
                    XOR2 = "861f1dca05a0;9ddd5261e5dcc@6b438e6c.8ba7d71c*4fd11f3af1";
                    break;
                default:
                    XOR1 = string.Empty;
                    XOR2 = string.Empty;
                    break;
            }
        }

        public byte[] Deobfuscate(byte[] data) => data.Select((b, i) => (byte)(b ^ XOR1[i % XOR1.Length] ^ XOR2[i % XOR2.Length])).ToArray();

        public byte[] Obfuscate(byte[] data) => Deobfuscate(data);

        public static bool IsXORed(Stream input)
        {
            var bk = input.Position;
            input.Position = input.Length - 1;
            bool result = input.ReadByte() != 0x00;
            input.Position = bk;

            return result;
        }
    }

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
}