using System.Runtime.InteropServices;

namespace text_srtz
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Header
    {
        public ushort Magic;
        public byte AugmentA;
        public byte AugmentB;
        public ushort AugmentC;
        public ushort StringCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class EntryData
    {
        public uint ID;
        public uint Offset;
    }

    public sealed class Entry
    {
        public uint ID;
        public string Text;
    }
}
