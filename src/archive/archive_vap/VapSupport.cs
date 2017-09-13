using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.IO;

namespace archive_vap
{
    public class VAPFileInfo : ArchiveFileInfo
    {
        public Entry entry;

        public int Write(Stream input, int offset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.BaseStream.Position = offset;

                FileData.CopyTo(bw.BaseStream);

                entry.offset = offset;
                entry.size = (int)FileSize;

                return offset + (int)FileSize;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public int fileCount;
        public int unk1;
        public int unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public int offset;
        public int size;
    }
}
