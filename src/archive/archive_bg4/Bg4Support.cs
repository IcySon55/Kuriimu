using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Interface;
using System.IO;
using Kontract.IO;
using Cetera.Hash;

namespace archive_bg4
{
    public class BG4FileInfo : ArchiveFileInfo
    {
        public FileEntry entry;
        public bool _compressed;
        const int hashSeed = 0x1f;

        public int Write(Stream input, int absDataOffset)
        {
            entry.fileSize = (FileSize == 0) ? 0x80000000 : (uint)(FileSize | ((_compressed) ? 0x80000000 : 0));
            entry.fileOffset = (entry.fileSize == 0x80000000) ? 0 : absDataOffset;
            entry.nameHash = (FileName == "(invalid)") ? 0xffffffff : SimpleHash.Create(FileName.Reverse().Aggregate("", (o, cl) => o + cl), hashSeed);

            FileData.CopyTo(input);

            return (int)(absDataOffset + FileSize);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public short version;
        public short fileEntryCount;
        public int metaSecSize;
        public short fileEntryCountDerived;
        public short fileEntryCountMultiplier;  //fileEntryCountDerived * fileEntryCountMultiplier = fileEntryCount
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public int fileOffset;
        public uint fileSize;    //MSB is set if file is compressed
        public uint nameHash;
        public short relNameOffset;
    }

    public class NameEntry
    {
        public string name;
        public int offset;
    }
}
