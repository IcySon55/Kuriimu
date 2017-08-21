using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Compression;

namespace archive_cdar
{
    public class CDARFileInfo : ArchiveFileInfo
    {
        public uint hash;
        public Entry entry;

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived || entry.decompSize == 0) return base.FileData;
                return new MemoryStream(ZLib.Decompress(base.FileData));
            }
        }

        public override long? FileSize => (entry.decompSize == 0) ? entry.compSize : entry.decompSize;

        public void Write(Stream input, uint offset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.BaseStream.Position = offset;

                if (State == ArchiveFileState.Archived)
                {
                    base.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment();
                }
                else
                {
                    if (entry.decompSize == 0)
                    {
                        entry.offset = offset;
                        entry.compSize = (uint)base.FileData.Length;
                        base.FileData.CopyTo(bw.BaseStream);

                        bw.WriteAlignment();
                    }
                    else
                    {
                        entry.offset = offset;
                        entry.decompSize = (uint)base.FileData.Length;
                        var comp = ZLib.Compress(base.FileData);
                        entry.compSize = (uint)comp.Length;
                        bw.Write(comp);

                        bw.WriteAlignment();
                    }
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint unk1;
        public uint entryCount;
        public uint unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset;
        public uint decompSize;
        public uint compSize;
    }
}
