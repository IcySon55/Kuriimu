using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract.IO;
using System.IO;
using System.Text;

namespace archive_oto3_dat
{
    public class OTO3FileInfo : ArchiveFileInfo
    {
        public Entry entry;

        public void Write(Stream input)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                entry.fileOffset = (int)bw.BaseStream.Position;
                entry.fileSize = (int)FileSize;
                FileData.CopyTo(bw.BaseStream);
                bw.WriteAlignment();
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public int entryCount;
        public int nameBufSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public Entry(Stream input, int nameBufSize)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                fileOffset = br.ReadInt32();
                fileSize = br.ReadInt32();
                fileName = br.ReadString(nameBufSize);
            }
        }

        public int fileOffset;
        public int fileSize;
        public string fileName;

        public void Write(Stream input, int nameBufSize)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.Write(fileOffset);
                bw.Write(fileSize);
                bw.Write(Encoding.ASCII.GetBytes(fileName));
                bw.WritePadding(nameBufSize - fileName.Length);
            }
        }
    }
}
