using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;
using Kontract.Compression;
using System.IO;
using Kontract.IO;
using System.Collections.Generic;
using System;

namespace archive_level5.XFSA
{
    public static class Extensions
    {
        public static IEnumerable<FileEntryIndex> ReadMultipleEntriesInc(this BinaryReaderX br, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new FileEntryIndex
                {
                    entry = br.ReadStruct<FileEntry>(),
                    ID = i
                };
            }
        }

        public static void WriteMultiple<T>(this BinaryWriterX bw, IEnumerable<T> list)
        {
            foreach (var entry in list)
                bw.WriteStruct(entry);
        }
    }

    public class Support
    {
        public static Stream DecompressToStream(byte[] input) => new MemoryStream(Level5.Decompress(new MemoryStream(input)));
    }

    public class XFSAFileInfo : ArchiveFileInfo
    {
        public FileEntry fileEntry;
        public Table0Entry dirEntry;
        public int fileCountInDir;

        public int UpdateEntry(int offset)
        {
            //catch file limits
            if (FileSize >= 0x100000)
            {
                throw new Exception("File " + FileName + " is too big to pack into this archive type!");
            }
            else if (offset >= 0x20000000)
            {
                throw new Exception("The archive can't be bigger than 0x20000000 Bytes.");
            }

            //Edit entry
            //entry.entry.comb1 = (entry.entry.comb1 & 0xfe000000) | (uint)(offset >> 4);
            //entry.entry.comb2 = (entry.entry.comb2 & 0xfff00000) | ((uint)FileSize);

            return ((offset + (int)FileSize) % 0x10 != 0) ? (((offset + (int)FileSize) + 0xf) & ~0xf) : offset + (int)FileSize + 0x10;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int table0Offset;
        public int table1Offset;
        public int fileEntryTableOffset;
        public int nameTableOffset;
        public int dataOffset;
        public short table0EntryCount;
        public short table1EntryCount;
        public int fileEntryCount;
        public int infoSecDecompSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Table0Entry
    {
        public uint hash;
        public uint comb1;
        public short fileEntryOffset;
        public short unk1;
        public uint comb3;

        public uint firstFileNameInDir { get { return comb1 >> 14; } set { comb1 = (comb1 & 0x3FFF) + (value << 14); } }
        public uint fileCountInDir { get { return comb1 & 0x3FFF; } set { comb1 = (uint)((comb1 & ~0x3FFF) + (value & 0x3FFF)); } }

        public uint dirNameOffset { get { return comb3 >> 14; } set { comb3 = (comb3 & 0x3FFF) + (value << 14); } }
        public uint dirCountInDir { get { return comb3 & 0x3FFF; } set { comb3 = (uint)((comb3 & ~0x3FFF) + (value & 0x3FFF)); } }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Table1Entry
    {
        public uint hash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint crc32;  //lower case filename hash
        public uint comb1; //offset combined with an unknown value, offset is last 24 bits with 4bit left-shift
        public uint comb2;   //size combined with an unknown value, size is last 20 bits

        public int offset { get { return (int)comb1 & 0x03FFFFFF; } set { comb1 = (uint)((comb1 & ~0x03FFFFFF) + (value & 0x03FFFFFF)); } }
        public int size { get { return (int)comb2 & 0x007FFFFF; } set { comb2 = (uint)((comb2 & ~0x007FFFFF) + (value & 0x007FFFFF)); } }
        public int nameOffset => (int)((comb1 >> 26) * 512 + (comb2 >> 23));
    }

    public class FileEntryIndex
    {
        public FileEntry entry;
        public int ID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NameEntry
    {
        public uint offset; //relative to nameTableOffset
        public uint name;
    }
}
