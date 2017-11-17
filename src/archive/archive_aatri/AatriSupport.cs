using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Komponent.IO;

namespace archive_aatri.aatri
{
    public class AatriFileInfo : ArchiveFileInfo
    {
        public Entry Entry;
        public Import imports;

        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;

                if (State != ArchiveFileState.Archived)
                {
                    imports.nintendo.SetMethod(0x11);
                    return new MemoryStream(imports.nintendo.Compress(baseStream));
                }

                if (Entry.uncompSize == 0) return baseStream;
                return new MemoryStream(imports.nintendo.Decompress(baseStream, 0));
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
                    var compData = new MemoryStream(imports.lz11.Compress(base.FileData));
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
}
