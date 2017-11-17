using System.Runtime.InteropServices;
using Kontract.Interface;
using System.IO;
using Komponent.IO;
using System.Text;

namespace archive_dqxi
{
    public class PACKFileInfo : ArchiveFileInfo
    {
        public int Write(Stream input, int offset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.BaseStream.Position = offset;

                bw.Write((int)FileSize);
                bw.Write(Encoding.ASCII.GetBytes(FileName));
                bw.WritePadding(0x20 - Encoding.ASCII.GetByteCount(FileName));
                FileData.CopyTo(bw.BaseStream);
                bw.WriteAlignment(4);

                return (int)(offset + 4 + 0x20 + FileSize + 0x3) & ~0x3;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int size;
        public short unk1;
        public ushort byteOrder;
        public short count;
        public short tableSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TableEntry
    {
        public short unk1;
        public short unk2;
        public uint hash;
    }
}
