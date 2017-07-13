using System;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using System.Collections.Generic;

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

    public enum Version : uint
    {
        Version1 = 0x00010201,
        Version2 = 0x00010302
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public Version version;
        public Language lang;
        public ulong zero1;

        public uint labelCount;
        public uint secCount;

        public uint labelSize;
        public uint secSize;

        public uint nameSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entryv1
    {
        public uint id;
        public uint unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entryv2
    {
        public uint id;
        public uint unk1;
        public uint unk2;
        public uint labelOffset; //relative to labelDataOffset
        public uint zero1;
    }

    public class XOR
    {
        private static String xor1;
        private static String xor2;

        public XOR(Version version)
        {
            switch (version)
            {
                case Version.Version1:
                    xor1 = "fjfajfahajra;tira9tgujagjjgajgoa";
                    xor2 = "mva;eignhpe/dfkfjgp295jtugkpejfu";
                    break;
                case Version.Version2:
                    xor1 = "e43bcc7fcab+a6c4ed22fcd433/9d2e6cb053fa462-463f3a446b19";
                    xor2 = "861f1dca05a0;9ddd5261e5dcc@6b438e6c.8ba7d71c*4fd11f3af1";
                    break;
                default:
                    xor1 = String.Empty;
                    xor2 = String.Empty;
                    break;
            }
        }

        public static bool IsXORed(Stream input)
        {
            var bk = input.Position;
            input.Position = input.Length - 1;
            bool result = input.ReadByte() != 0x00;
            input.Position = bk;

            return result;
        }

        public static byte[] Deobfuscate(byte[] data)
        {
            for (int index = 0; index < data.Length; ++index)
            {
                char ch1 = xor1[index % xor1.Length];
                char ch2 = xor2[index % xor2.Length];
                data[index] = Convert.ToByte(data[index] ^ ch1 ^ ch2);
            }

            return data;
        }

        public static byte[] Obfuscate(byte[] data) => Deobfuscate(data);
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
}