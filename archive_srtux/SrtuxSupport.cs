using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Compression;
using System;
using System.Linq;

namespace archive_srtux
{
    public class SrtuxFileInfo : ArchiveFileInfo
    {
        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;
                using (var br = new BinaryReaderX(baseStream, true))
                {
                    var header = new ECDHeader(br.BaseStream);
                    if (header.magic != "ECD\u0001") return br.BaseStream;
                    br.BaseStream.Position -= 0x10;
                    return new MemoryStream(LZECD.Decompress(new MemoryStream(br.ReadBytes(header.compSize + 0x10))));
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset = 0;
        public uint size = 0;
    }

    public class ECDHeader
    {
        public ECDHeader(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                magic = br.ReadStruct<Magic>();
                init = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
                compSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
                uncompSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
            }
        }
        public Magic magic;
        public int init;
        public int compSize;
        public int uncompSize;
    }
}
