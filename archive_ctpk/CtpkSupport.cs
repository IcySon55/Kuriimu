using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;
using Cetera.Image;

namespace archive_ctpk
{
    public class CTPKFileInfo : ArchiveFileInfo
    {
        public Entry Entry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        Magic magic;
        ushort version;
        public short texCount;
        public int texSecOffset;
        public int texSecSize;
        public int hashSecOffset;
        public int texInfoOffset;
        int zero0;
        int zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public int nameOffset;
        public int texDataSize;
        public int texOffset;
        public int format;
        public short width;
        public short height;
        public byte mipLvl;
        public byte type;
        public short unk1;
        public int bitmapSizeOffset;
        public uint timeStamp;
    }
}
