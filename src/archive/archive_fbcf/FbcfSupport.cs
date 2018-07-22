using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;

namespace archive_fbcf
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public short const1;
        public int const2;
        public short entryCount;
        public int fileOffsetTableOffset;
        public int fileSizeOffsetTableOffset;
        public int fileNameOffsetTableOffset;
        public int dataOffset;  //relative to fileOffsetTableOffset
        public int fileSizeOffset;  //relative to fileSizeOffsetTableOffset
        public int fileNameOffset;  //relative to fileNameOffsetTableOffset

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] padding;

        public Magic magic;
    }
}
