using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Interface;
using System.IO;
using Kontract.IO;

namespace archive_level5.LPC2
{
    public class LPC2FileInfo : ArchiveFileInfo
    {
        public FileEntry entry;

        public (int newNameOff, int newFileOff) Write(Stream input, int nameOffset, int fileOffset)
        {
            entry.fileSize = (int)FileSize;
            entry.nameOffset = nameOffset;
            entry.fileOffset = fileOffset;

            using (var bw = new BinaryWriterX(input, true))
            {
                FileData.CopyTo(bw.BaseStream);
                if (bw.BaseStream.Position % 4 != 0)
                    bw.WriteAlignment(4);
                else
                    bw.WritePadding(4);
            }

            return (nameOffset + Encoding.ASCII.GetByteCount(FileName) + 1,
                ((fileOffset + FileSize) % 4 != 0) ?
                    (int)((fileOffset + FileSize + 3) & ~3) :
                    (int)(fileOffset + FileSize + 4));
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int fileCount;
        public int headerSize;
        public int fileSize;

        public int fileEntryOffset;
        public int nameOffset;
        public int dataOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public int nameOffset; //relative
        public int fileOffset; //relative
        public int fileSize;
    }
}
