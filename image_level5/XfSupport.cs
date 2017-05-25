using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;

namespace image_xf
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CharSizeInfo
    {
        public sbyte offset_x;
        public sbyte offset_y;
        public byte glyph_width;
        public byte glyph_height;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct CharacterMap
    {
        public char code_point;
        ushort char_size;
        int image_offset;

        public int CharSizeInfoIndex => char_size % 1024;
        public int CharWidth => char_size / 1024;
        public int ColorChannel => image_offset % 16;
        public int ImageOffsetX => image_offset / 16 % 16384;
        public int ImageOffsetY => image_offset / 16 / 16384;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct XpckHeader
    {
        public Magic magic; //XPCK
        public byte fileEntries;
        public byte unk1;
        public short fileInfoOffset;
        public short filenameTableOffset;
        public short dataOffset;
        public short fileInfoSize;
        public short filenameTableSize;
        public int dataSize;

        public void CorrectHeader()
        {
            fileInfoOffset *= 4;
            filenameTableOffset *= 4;
            dataOffset *= 4;
            fileInfoSize *= 4;
            filenameTableSize *= 4;
            dataSize *= 4;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FileEntry
    {
        public int unk1;
        public short unk2;
        public ushort offsetTmp;
        public ushort fileSizeTmp;
        public ushort multiplier;

        public int fileSize => fileSizeTmp + ((multiplier / 256) * 0x10000) + (multiplier % 256);
        public int offset => offsetTmp * 4;
    }
}
