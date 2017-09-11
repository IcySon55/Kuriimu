using System.Runtime.InteropServices;

namespace archive_nintendo.VIW
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfHeader
    {
        public int FileCount;
        public int NameCount;
        public int Table0Offset;
        public int Table1Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfEntry
    {
        public int Offset;
        public int CompressedSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfMetaEntry
    {
        public short Unk1;
        public short Unk2;
        public short Unk3;
        public short Unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ViwEntry
    {
        public int ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x14)]
        public string Name;
    }
}
