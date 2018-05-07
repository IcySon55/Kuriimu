using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract.IO;
using Kontract;
using System.IO;

namespace archive_rdp
{
    public class RdpFileInfo : ArchiveFileInfo
    {
        public override Stream FileData { get => Decompress(base.FileData); set => base.FileData = value; }

        Stream Decompress(Stream input)
        {
            if (State == ArchiveFileState.Archived)
                using (var br = new BinaryReaderX(input, true))
                {
                    switch (br.ReadString(4))
                    {
                        case "blz4":
                            //zlib
                            var decompSize = br.ReadInt32();
                            br.BaseStream.Position = (br.BaseStream.Position + 0xf) & ~0xf;
                            br.BaseStream.Position = 0;
                            return input;
                        case "blz2":
                            //deflate
                            decompSize = br.ReadInt32();
                            br.BaseStream.Position = (br.BaseStream.Position + 0xf) & ~0xf;
                            br.BaseStream.Position = 0;
                            return input;
                        default:
                            br.BaseStream.Position = 0;
                            return input;
                    }
                }
            else
                return input;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ResHeader
    {
        public Magic magic;
        public int entryOffset;
        public int unk1;
        public int entryCount;
        public int dataOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ResEntry
    {
        public int offset;
        public int entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ResSubEntry
    {
        public int dataOffsetFlag;  //flag 0xc0=internal File, 0x30=?, 0x40=?,0x00=?
        public int compDataSize;
        public int stringOffset;
        public int stringParts;
        public int zero1;
        public int zero2;
        public int zero3;
        public int decompSize;

        public int internalOffset => dataOffsetFlag & 0x00FFFFFF;
        public byte flag => (byte)(dataOffsetFlag >> 24);
    }

    public class RES
    {
        public ResSubEntry entry;
        public string name;
        public byte[] fileContent;
    }
}
