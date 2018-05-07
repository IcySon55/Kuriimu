using System.Runtime.InteropServices;
using Kontract.Interface;

namespace archive_nintendo.MMB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public int TableSize;
        public short FileCount;
        public short Unknown1;
        public int Unknown2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MmbFileEntry
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x24)]
        public string FileName;
        public int Offset;
        public int FileSize;
        public int CtpkSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xC)]
        public byte[] Padding;
    }

    public class MmbFileInfo : ArchiveFileInfo
    {
    }
}
