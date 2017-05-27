using System.Runtime.InteropServices;

namespace archive_fa
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PlainFAHeader
    {
        public int fileCount;
        public int fileSize;
        public ulong padding;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class PlainFAEntry
    {
        public uint fileOffset;
        public uint fileSize;
        public ulong padding;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
        public string filename;
    }
}
