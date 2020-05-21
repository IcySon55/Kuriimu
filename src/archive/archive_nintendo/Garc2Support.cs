using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract;
using System.IO;
using Kontract.IO;

namespace archive_nintendo.GARC2
{
    public class GARC2FileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint headerSize;
        public ushort byteOrder;
        public ushort version;
        public uint secCount;
        public uint dataOffset;
        public uint fileSize;
        // misses largest file size from GARC4
    }

    //File Allocation Table Offsets
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FatoHeader
    {
        public Magic magic;
        public uint sectionSize;
        public ushort entryCount;
        //ushort padding with 0xff
    }

    //FATB
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FatbHeader
    {
        public Magic magic;
        public uint sectionSize;
        public uint entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FatbEntry
    {
        public uint unk1;
        public uint offset;
        public uint endOffset;
        // misses size from GARC4
    }

    //FIMB
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FimbHeader
    {
        public Magic magic;
        public uint sectionSize;
        public uint dataSize;
    }
}
