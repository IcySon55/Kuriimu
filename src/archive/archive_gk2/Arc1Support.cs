using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Komponent.IO;

namespace archive_gk2.arc1
{
    public class Arc1FileInfo : ArchiveFileInfo
    {
        public Entry entry;
        public Import imports;

        public override Stream FileData
        {
            get
            {
                if ((entry.size & 0x80000000) == 0 || State != ArchiveFileState.Archived) return base.FileData;
                return new MemoryStream(imports.nintendo.Decompress(base.FileData, 0));
            }
        }

        public override long? FileSize => entry.size & 0x7fffffff;

        public void Write(Stream input, uint offset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                var startOffset = bw.BaseStream.Position;
                bw.BaseStream.Position = offset;

                if (State == ArchiveFileState.Archived)
                {
                    entry.offset = offset;
                    base.FileData.CopyTo(bw.BaseStream);
                }
                else
                {
                    if ((entry.size & 0x80000000) == 0)
                    {
                        entry.offset = offset;
                        base.FileData.CopyTo(bw.BaseStream);
                        entry.size = (uint)base.FileData.Length;
                    }
                    else
                    {
                        entry.offset = offset;
                        var comp = imports.lz11.Compress(base.FileData);
                        bw.Write(comp);
                        entry.size = (uint)(comp.Length | 0x80000000);
                    }
                }

                bw.WriteAlignment(0x4);
                bw.BaseStream.Position = startOffset;
            }
        }
    }

    public class Import
    {
        [Import("Nintendo")]
        public ICompressionCollection nintendo;
        [Import("LZ11")]
        public ICompression lz11;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset;
        public uint size;
    }
}
