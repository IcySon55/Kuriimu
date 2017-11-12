using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;
using System.IO;
using Kontract.IO;

namespace archive_dpk.DG2
{
    public class DG2FileInfo : ArchiveFileInfo
    {
        public Entry entry;
        public int Write(Stream input, int offset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                entry.offset = offset;
                entry.fileSize = (int)FileSize;
                entry.padFileSize = ((int)FileSize + 0x7ff) & ~0x7ff;

                bw.BaseStream.Position = offset;
                FileData.CopyTo(bw.BaseStream);

                return (offset + (int)FileSize + 0x7ff) & ~0x7ff;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public int headerSize;
        public int unk1;
        public int dataOffset;
        public int fileCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public uint unk1;
        public uint unk2;
        public uint unk3;
        public uint unk4;
        public int fileSize;
        public int padFileSize;
        public int offset;
        public int zero1;
    }
}
