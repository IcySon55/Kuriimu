using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kuriimu.Kontract;
using Cetera.Image;
using System.Drawing;

namespace image_nintendo.CTPK
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class BitmapClass
    {
        public Bitmap bmp;
        public Format format;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public short version;
        public short texCount;
        public int texSecOffset;
        public int texSecSize;
        public int crc32SecOffset;
        public int texInfoOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CtpkEntry
    {
        public TexEntry texEntry;
        public List<uint> dataSizes = new List<uint>();
        public string name;
        public HashEntry hash;
        public MipmapEntry mipmapEntry;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TexEntry
    {
        public int nameOffset;
        public int texDataSize;
        public int texOffset;
        public int imageFormat;
        public short width;
        public short height;
        public byte mipLvl;
        public byte type;
        public short zero0;
        public int bitmapSizeOffset;
        public uint timeStamp;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HashEntry
    {
        public uint crc32;
        public int id;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MipmapEntry
    {
        public byte mipmapFormat;
        public byte mipLvl;
        //never used compression specifications?
        public byte compression;
        public byte compMethod;
    }

    public sealed class CtpkBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public Format Format { get; set; }
    }
}
