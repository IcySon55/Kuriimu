using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace text_msbt
{
    public enum EncodingByte : byte
    {
        UTF8 = 0x00,
        Unicode = 0x01
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Header
    {
        public Magic8 Magic; // MsgStdBn
        public ByteOrder ByteOrder;
        public ushort Unknown1; // Always 0x0000
        public EncodingByte EncodingByte;
        public byte Unknown2; // Always 0x03
        public ushort NumberOfSections;
        public ushort Unknown3; // Always 0x0000
        public uint FileSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xA)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Section
    {
        public Magic Magic;
        public uint Size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8)]
        public byte[] Padding;
    }

    public sealed class LBL1
    {
        public Section Section;
        public uint NumberOfGroups;

        public List<Group> Groups = new List<Group>();
        public List<Label> Labels = new List<Label>();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Group
    {
        public uint NumberOfLabels;
        public uint Offset;
    }

    public sealed class Label
    {
        public string Name { get; set; }
        public uint Index { get; set; }

        public uint Checksum { get; set; }
        public String String { get; set; }

        public string Text
        {
            get => String.Text;
            set => String.Text = value;
        }

        public Label()
        {
            Name = string.Empty;
            Index = 0;
            Checksum = 0;
            String = new String();
        }
    }

    public sealed class NLI1
    {
        public Section Section;
        public byte[] Unknown; // Tons of unknown data
    }

    public sealed class ATO1
    {
        public Section Section;
        public byte[] Unknown; // Large collection of 0xFF
    }

    public sealed class ATR1
    {
        public Section Section;
        public byte[] Unknown; // Tons of unknown data
    }

    public sealed class TSY1
    {
        public Section Section;
        public byte[] Unknown; // Tons of unknown data
    }

    public sealed class TXT2
    {
        public Section Section;
        public uint NumberOfStrings;

        public List<String> Strings = new List<String>();
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