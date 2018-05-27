using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.Compression;
using System.IO;

namespace archive_pac_mario_party
{
    public class PacFileInfo : ArchiveFileInfo
    {
        public Entry entry;

        public override Stream FileData
        {
            get => new MemoryStream(ZLib.Decompress(base.FileData));
        }

        public override long? FileSize => (State == ArchiveFileState.Archived) ? entry.decompSize : base.FileData.Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic = "PAC\0";
        public int unk1;
        public int unk2;
        public int dataOffset;

        public Magic magic2;
        public int assetCount;
        public int entryCount;
        public int stringCount;
        public int fileDataCount;
        public long zero0;
        public long zero1;
        public int assetOffset;
        public int entryOffset;
        public int stringOffset;
        public int fileDataOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AssetEntry
    {
        public int stringOffset;
        public uint unk1;
        public int count;
        public int entryOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public int stringOffset;
        public uint unk1;
        public int extOffset;
        public int const1;

        public int dataOffset;
        public int decompSize;
        public int compSize;
        public int compSize2;

        public long zero1;
        public int unk2;
        public int zero2;
    }
}
