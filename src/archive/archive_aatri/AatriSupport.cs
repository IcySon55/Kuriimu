using System.IO;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract.Compression;
using Kontract.IO;

namespace archive_aatri.aatri
{
    public class AatriFileInfo : ArchiveFileInfo
    {
        public Entry Entry;

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived) return new MemoryStream(Nintendo.Compress(base.FileData, Nintendo.Method.LZ11));

                if (Entry.uncompSize == 0) return base.FileData;
                return new MemoryStream(Nintendo.Decompress(base.FileData));
            }
        }

        public override long? FileSize => Entry.uncompSize == 0 ? base.FileSize : Entry.uncompSize;

        public void Write(Stream input)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                if (State == ArchiveFileState.Archived || Entry.uncompSize == 0)
                {
                    Entry.offset = (uint)bw.BaseStream.Position;

                    base.FileData.CopyTo(bw.BaseStream);
                }
                else
                {
                    Entry.offset = (uint)bw.BaseStream.Position;
                    Entry.uncompSize = (uint)base.FileData.Length;

                    var compData = new MemoryStream(Nintendo.Compress(base.FileData,Nintendo.Method.LZ11));
                    Entry.compSize = (uint)compData.Length;
                    compData.CopyTo(bw.BaseStream);
                }
            }
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset;
        public uint flags;
        public uint uncompSize;
        public uint compSize;
        public uint hash;
    }
}
