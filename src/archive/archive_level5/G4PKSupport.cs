using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kontract;

namespace archive_level5.G4PK
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic = "G4PK";
        public short headerSize = 0x40;
        public short fileType = 0x64;
        public int unk1 = 0x00100000;
        public int contentSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] zeroes1 = new byte[0x10];
        public int fileCount;
        public short table2EntryCount;
        public short table3EntryCount;
        public short unk2;
        public short unk3;
    }
}
