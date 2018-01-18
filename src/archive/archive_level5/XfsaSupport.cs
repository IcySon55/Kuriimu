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
        public FileEntryIndex entry;

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
            entry.entry.comb1 = (entry.entry.comb1 & 0xfe000000) | (uint)(offset >> 4);
            entry.entry.comb2 = (entry.entry.comb2 & 0xfff00000) | ((uint)FileSize);

            return ((offset + (int)FileSize) % 0x10 != 0) ? (((offset + (int)FileSize) + 0xf) & ~0xf) : offset + (int)FileSize + 0x10;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint offset1;
        public uint offset2;
        public uint fileEntryTableOffset;
        public uint nameTableOffset;
        public uint dataOffset;
        public ushort table1EntryCount;
        public ushort table2EntryCount;
        public uint fileEntryCount;
        public uint unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Table1Entry
    {
        public uint hash;
        public uint unk1;
        public uint unk2;
        public uint unk3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Table2Entry
    {
        public uint hash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileEntry
    {
        public uint crc32;  //lower case filename hash
        public uint comb1; //offset combined with an unknown value, offset is last 24 bits with 4bit left-shift
        public uint comb2;   //size combined with an unknown value, size is last 20 bits
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
