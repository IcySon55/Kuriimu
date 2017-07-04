using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.IO;

namespace archive_srtz.MTV
{
    public sealed class MtvArchiveFileInfo : ArchiveFileInfo
    {
        public int id { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VarHeader
    {
        public VarHeader(Stream input)
        {
            using (var br=new BinaryReaderX(input,true))
            {
                fileEntries = br.ReadInt32();
                headerSize = br.ReadInt32();

                entries = new Entry[fileEntries];
                var tmp = br.ReadUInt64();
                br.BaseStream.Position -= 8;
                if (tmp==0)
                {
                    for (int i = 0; i < fileEntries; i++) {
                        var offset = br.ReadInt32() + headerSize;
                        br.BaseStream.Position += 4;
                        var size = br.ReadInt32();
                        br.BaseStream.Position -= 4;
                        entries[i] = new Entry(offset, size);
                    }
                }
                else
                {
                    for (int i = 0; i < fileEntries; i++) entries[i] = new Entry(br.ReadInt32() + headerSize, br.ReadInt32());
                }
            }
        }

        public int fileEntries;
        public int headerSize;
        public Entry[] entries;

        public class Entry
        {
            public Entry(int offset, int size)
            {
                this.offset = offset;
                this.size = size;
            }

            public int offset;
            public int size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CompressionHeader2
    {
        public uint headerParts;
        public uint Length;
        public uint Padding1;
        public uint exHeaderSize;
        public uint exHeaderSize2;
        public uint dataSize;
        public uint dataSize2; //with exHeader
        public uint Padding4;
    }
}
