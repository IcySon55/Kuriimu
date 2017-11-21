using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Komponent.IO;
using Komponent.Compression;

namespace archive_aatri.aatri
{
    public class AatriFileInfo : ArchiveFileInfo
    {
        public Entry Entry;

        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;

                if (State != ArchiveFileState.Archived)
                {
                    var comp = new Nintendo();
                    comp.SetMethod(0x11);
                    return new MemoryStream(comp.Compress(baseStream));
                }

                if (Entry.uncompSize == 0) return baseStream;
                return new MemoryStream(new Nintendo().Decompress(baseStream, 0));
            }
        }

        public override long? FileSize => Entry.uncompSize == 0 ? base.FileSize : Entry.uncompSize;

        public void Write(Stream input)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                if (State == ArchiveFileState.Archived || Entry.uncompSize == 0)
                    base.FileData.CopyTo(bw.BaseStream);
                else
                {
                    var compData = new MemoryStream(new LZ11().Compress(base.FileData));
                    Entry.uncompSize = (uint)base.FileData.Length;
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
