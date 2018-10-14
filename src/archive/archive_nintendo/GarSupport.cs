using System.IO;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;
using Kontract.IO;

namespace archive_nintendo.GAR
{
    public class GARFileInfo : ArchiveFileInfo
    {
        public string ext;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint fileSize;
        public short fileChunks;
        public short fileCount;
        public int chunkEntryOffset;
        public int chunkInfOffset;
        public int offset3;
        public Magic8 hold0; //jenkins
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ChunkEntry
    {
        public int fileCount;
        public uint IDOffset;
        public uint chunkInfOffset;
        public uint hold0;
    }

    public class SystemChunkEntry
    {
        public SystemChunkEntry(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                fileCount = br.ReadInt32();
                unk1 = br.ReadUInt32();
                chunkInfoOffset = br.ReadUInt32();
                nameOffset = br.ReadUInt32();
                subTableOffset = br.ReadUInt32();

                var origPos = br.BaseStream.Position;
                br.BaseStream.Position = nameOffset;
                name = br.ReadCStringA();
                br.BaseStream.Position = origPos;
            }
        }

        public int fileCount;
        public uint unk1;
        public uint chunkInfoOffset;
        public uint nameOffset;
        public uint subTableOffset;

        public string name;
    }

    public class ChunkInfo
    {
        public ChunkInfo(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                fileSize = br.ReadUInt32();
                nameOffset = br.ReadInt32();
                fileNameOffset = br.ReadInt32();

                long bk = br.BaseStream.Position;
                br.BaseStream.Position = nameOffset;
                name = br.ReadCStringA();
                br.BaseStream.Position = fileNameOffset;
                fileName = br.ReadCStringA();
                br.BaseStream.Position = bk;
            }
        }

        public uint fileSize;
        public int nameOffset;
        public int fileNameOffset;

        public string name;
        public string fileName;
    }

    public class SystemChunkInfo
    {
        public SystemChunkInfo(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                fileSize = br.ReadUInt32();
                fileOffset = br.ReadInt32();
                nameOffset = br.ReadInt32();
                hold0 = br.ReadInt32();

                long bk = br.BaseStream.Position;
                br.BaseStream.Position = nameOffset;
                name = br.ReadCStringA();
                br.BaseStream.Position = bk;
            }
        }

        public uint fileSize;
        public int fileOffset;
        public int nameOffset;
        public int hold0;

        public string name;
        public string ext;
    }
}
