using System;
using System.Collections.Generic;

namespace file_msbt
{
    public enum EncodingByte : byte
    {
        UTF8 = 0x00,
        Unicode = 0x01
    }

    public sealed class Header
    {
        public string Identifier; // MsgStdBn
        public byte[] ByteOrderMark;
        public ushort Unknown1; // Always 0x0000
        public EncodingByte EncodingByte;
        public byte Unknown2; // Always 0x03
        public ushort NumberOfSections;
        public ushort Unknown3; // Always 0x0000
        public uint FileSize;
        public byte[] Unknown4; // Always 0x0000 0000 0000 0000 0000

        public uint FileSizeOffset;
    }

    public abstract class Section
    {
        public string Identifier;
        public uint SectionSize; // Begins after Unknown1
        public byte[] Padding1; // Always 0x0000 0000
    }

    public sealed class LBL1 : Section
    {
        public uint NumberOfGroups;

        public List<Group> Groups;
        public List<Label> Labels;

        public LBL1()
        {
            Groups = new List<Group>();
            Labels = new List<Label>();
        }
    }

    public sealed class Group
    {
        public uint NumberOfLabels;
        public uint Offset;
    }

    public sealed class Label
    {
        public uint Length { get; set; }
        public string Name { get; set; }
        public uint Index { get; set; }

        public uint Checksum { get; set; }
        public String String { get; set; }

        public string Text
        {
            get
            {
                return String.Text;
            }
            set
            {
                String.Text = value;
            }
        }

        public Label()
        {
            Length = 0;
            Name = string.Empty;
            Index = 0;
            Checksum = 0;
            String = new String();
        }
    }

    public sealed class NLI1 : Section
    {
        public byte[] Unknown2; // Tons of unknown data
    }

    public sealed class ATO1 : Section
    {
        public byte[] Unknown2; // Large collection of 0xFF
    }

    public sealed class ATR1 : Section
    {
        public uint NumberOfAttributes;
        public byte[] Unknown2; // Tons of unknown data
    }

    public sealed class TSY1 : Section
    {
        public byte[] Unknown2; // Tons of unknown data
    }

    public sealed class TXT2 : Section
    {
        public uint NumberOfStrings;

        public List<String> Strings;

        public TXT2()
        {
            Strings = new List<String>();
        }
    }

    public sealed class String
    {
        public string Text { get; set; }

        public uint Index { get; set; }

        public String()
        {
            Text = string.Empty;
            Index = 0;
        }
    }

    public sealed class InvalidMSBTException : Exception
    {
        public InvalidMSBTException(string message) : base(message) { }
    }
}