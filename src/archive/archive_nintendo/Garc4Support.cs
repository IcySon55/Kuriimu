using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.GARC4
{
    public class GARC4FileInfo : ArchiveFileInfo
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
        public uint largestFileSize;
    }

    //File Allocation Table Offsets
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FatoHeader
    {
        public Magic magic;
        public uint headerSize;
        public ushort entryCount;
        //ushort padding with 0xff
    }

    //FATB
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FatbHeader
    {
        public Magic magic;
        public uint headerSize;
        public uint entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FatbEntry
    {
        public uint unk1;
        public uint offset;
        public uint endOffset;
        public uint size;
    }

    //FIMB
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FimbHeader
    {
        public Magic magic;
        public uint headerSize;
        public uint dataSize;
    }
}
