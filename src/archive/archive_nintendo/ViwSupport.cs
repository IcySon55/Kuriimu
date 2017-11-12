using System.IO;
using System.Runtime.InteropServices;
using Kontract.Compression;
using Kontract.IO;
using Kontract.Interface;

namespace archive_nintendo.VIW
{
    public class ViwFileInfo : ArchiveFileInfo
    {
        public override Stream FileData => State != ArchiveFileState.Archived ? base.FileData : new MemoryStream(Nintendo.Decompress(base.FileData));
        public override long? FileSize => base.FileData.Length;

        public int Write(Stream output)
        {
            var compressedSize = (int)base.FileData.Length;

            using (var bw = new BinaryWriterX(output, true))
            {
                if (State == ArchiveFileState.Archived)
                    base.FileData.CopyTo(bw.BaseStream);
                else
                {
                    var bytes = Nintendo.Compress(base.FileData, Nintendo.Method.LZ10);
                    compressedSize = bytes.Length;
                    bw.Write(bytes);
                }
            }

            return compressedSize;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfHeader
    {
        public int FileCount;
        public int MetaEntryCount;
        public int Table0Offset;
        public int Table1Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfEntry
    {
        public int Offset;
        public int CompressedSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfMetaEntry
    {
        public short Unk1;
        public short Unk2;
        public short Unk3;
        public short Unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ViwEntry
    {
        public int ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x14)]
        public string Name;
    }
}
