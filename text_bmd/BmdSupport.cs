using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace text_bmd
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Header
    {
        public Magic Magic;
        public int NumberOfTableEntries;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class TableEntry
    {
        public int GroupCount;
        public int Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class DialogGroup
    {
        public ushort Unk0; // Order?
        public ushort Unk1;
        public int Unk2;
        public int Unk3;
        public int SpeakerOffset;
        public int MessageOffset;
    }

    public sealed class Speaker
    {
        public string Name;
        public string Text;
    }

    public sealed class Message
    {
        public string Name;
        public string Text;
    }
}