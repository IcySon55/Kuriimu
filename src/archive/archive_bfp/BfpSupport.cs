using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;
using Komponent.IO;
using System.IO;

namespace archive_bfp
{
    public class BFPFileInfo : ArchiveFileInfo
    {
        public Entry entry;
        public Entry2 entry2;
        public bool compressed;

        public Import imports;

        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;
                if (State != ArchiveFileState.Archived) return baseStream;
                if (!compressed)
                {
                    using (var br = new BinaryReaderX(baseStream, true))
                    {
                        var header = br.ReadStruct<CompHeader>();
                        br.BaseStream.Position = 0x20;
                        return new MemoryStream(br.ReadBytes((int)header.size));
                    }
                }
                using (var br = new BinaryReaderX(baseStream, true))
                {
                    var header = br.ReadStruct<CompHeader>();
                    br.BaseStream.Position = 0x20;
                    return new MemoryStream(imports.zlib.Decompress(new MemoryStream(br.ReadBytes((int)header.size)), 0));
                }
            }
        }

        public override long? FileSize => (entry == null) ? entry2.uncompSize : entry.uncompSize;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class CompHeader
        {
            public uint size;
            public uint padSize;
        }

        public uint Write(Stream stream)
        {
            using (var bw = new BinaryWriterX(stream, true))
            {
                if (State == ArchiveFileState.Archived)
                {
                    base.FileData.Position = 0;
                    base.FileData.CopyTo(bw.BaseStream);
                    return (uint)base.FileData.Length;
                }
                else
                {
                    if (compressed)
                    {
                        imports.zlib.SetMethod(0);
                        var comp = imports.zlib.Compress(FileData);
                        bw.Write(comp.Length);
                        bw.Write((comp.Length + 0xf) & ~0xf);
                        bw.WritePadding(0x18);
                        bw.Write(comp);
                        bw.WriteAlignment();
                        return (uint)((comp.Length + 0xf) & ~0xf) + 0x20;
                    }
                    else
                    {
                        bw.Write((uint)FileData.Length);
                        bw.Write((uint)((FileData.Length + 0xf) & ~0xf));
                        bw.WritePadding(0x18);
                        FileData.CopyTo(bw.BaseStream);
                        bw.WriteAlignment();
                        return (uint)((FileData.Length + 0xf) & ~0xf) + 0x20;
                    }
                }
            }
        }

        public void UpdateEntry(uint offset)
        {
            if (entry == null)
            {
                entry2.offset = offset;
                entry2.uncompSize = (uint)FileData.Length;
            }
            else
            {
                entry.offset = offset;
                entry.uncompSize = (uint)FileData.Length;
            }
        }
    }

    public class Import
    {
        [Import("ZLib")]
        public ICompressionCollection zlib;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint entryCount;
        public uint unk1;
        public uint unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint hash;
        public uint offset;
        public uint uncompSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry2
    {
        public uint offset;
        public uint uncompSize;
    }
}
