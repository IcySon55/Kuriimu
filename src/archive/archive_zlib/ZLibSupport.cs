using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;
using System.IO;
using Komponent.IO;
using Komponent.Compression;

namespace archive_zlib
{
    public class ZLIBFileInfo : ArchiveFileInfo
    {
        public uint decompSize;

        public override Stream FileData
        {
            get
            {
                return new MemoryStream(new ZLib().Decompress(base.FileData, 0));
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
                    var comp = new ZLib();
                    comp.SetMethod(0);
                    bw.Write(comp.Compress(base.FileData));
                }
        }
    }
}
