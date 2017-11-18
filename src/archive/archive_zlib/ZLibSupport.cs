using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;
using System.IO;
using Komponent.IO;

namespace archive_zlib
{
    public class ZLIBFileInfo : ArchiveFileInfo
    {
        public uint decompSize;
        public Import imports;

        public override Stream FileData
        {
            get
            {
                return new MemoryStream(imports.zlib.Decompress(base.FileData, 0));
            }
        }

        public override long? FileSize => decompSize;

        public void Write(Stream input)
        {
            using (var bw = new BinaryWriterX(input, true, ByteOrder.BigEndian))
                if (State == ArchiveFileState.Archived)
                {
                    bw.Write(decompSize);
                    base.FileData.CopyTo(bw.BaseStream);
                }
                else
                {
                    bw.Write((uint)base.FileData.Length);
                    imports.zlib.SetMethod(0);
                    bw.Write(imports.zlib.Compress(base.FileData));
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
}
