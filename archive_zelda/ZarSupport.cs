using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_zar
{
    public class ZARFileInfo : ArchiveFileInfo
    {
        public string ext;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ZARHeader
    {
        public Magic magic;
        public uint fileSize;
        public short fileChunks;
        public short fileCount;
        public int chunkEntryOffset;
        public int chunkInfOffset;
        public int offsetList;
        public Magic8 hold0; //queen
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZARChunkEntry
    {
        public int fileCount;
        public uint IDOffset;
        public uint chunkInfOffset;
        public uint hold0;
    }

    public class ZARChunkInfo
    {
        public ZARChunkInfo(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                fileSize = br.ReadUInt32();
                fileNameOffset = br.ReadInt32();

                long bk = br.BaseStream.Position;
                br.BaseStream.Position = fileNameOffset;
                fileName = br.ReadCStringA();
                br.BaseStream.Position = bk;
            }
        }

        public uint fileSize;
        public int fileNameOffset;

        public string fileName;
    }
}
