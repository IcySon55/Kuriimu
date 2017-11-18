using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;
using System.IO;
using Komponent.IO;

namespace archive_level5.XPCK
{
    public class XPCKFileInfo : ArchiveFileInfo
    {
        public FileInfoEntry Entry;

        public int Write(Stream input, int absDataOffset, int baseDataOffset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.BaseStream.Position = absDataOffset;
                FileData.CopyTo(bw.BaseStream);
                if (bw.BaseStream.Position % 4 > 0) bw.WriteAlignment(4);
                else bw.WritePadding(4);

                var relOffset = absDataOffset - baseDataOffset;
                Entry.tmp = (ushort)((relOffset >> 2) & 0xffff);
                Entry.tmpZ = (byte)(((relOffset >> 2) & 0xff0000) >> 16);
                Entry.tmp2 = (ushort)(FileSize & 0xffff);
                Entry.tmp2Z = (byte)((FileSize & 0xff0000) >> 16);

                return (bw.BaseStream.Position % 4 > 0) ? (int)(absDataOffset + FileSize + 0x3) & ~0x3 : (int)(absDataOffset + FileSize + 4);
            }
        }
    }

    public class Import
    {
        [Import("Level5")]
        public ICompressionCollection level5;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public byte fc1;
        public byte fc2;
        ushort tmp1;
        ushort tmp2;
        ushort tmp3;
        ushort tmp4;
        ushort tmp5;
        public uint tmp6;

        public ushort fileCount => (ushort)((fc2 & 0xf) << 8 | fc1);
        public ushort fileInfoOffset => (ushort)(tmp1 << 2);
        public ushort filenameTableOffset => (ushort)(tmp2 << 2);
        public ushort dataOffset => (ushort)(tmp3 << 2);
        public ushort fileInfoSize => (ushort)(tmp4 << 2);
        public ushort filenameTableSize => (ushort)(tmp5 << 2);
        public uint dataSize => tmp6 << 2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileInfoEntry
    {
        public uint crc32;
        public ushort nameOffset;
        public ushort tmp;
        public ushort tmp2;
        public byte tmpZ;
        public byte tmp2Z;

        public uint fileOffset => (((uint)tmpZ << 16) | tmp) << 2;
        public uint fileSize => ((uint)tmp2Z << 16) | tmp2;
    }
}
