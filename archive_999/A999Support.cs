using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kuriimu.Contract;

namespace archive_999
{
    public class A999FileInfo : ArchiveFileInfo
    {
        public Entry Entry;
        public uint XORpad;

        public override Stream FileData {
            get
            {
                if (State==ArchiveFileState.Archived)
                {
                    base.FileData.Position = 0;
                    var result = A999Support.deXOR(base.FileData, XORpad, (long)FileSize);
                    return new MemoryStream(result);
                } else
                {
                    return base.FileData;
                }
            }
        }

        public override long? FileSize => Entry.fileSize;
    }

    public class A999Support
    {
        public static byte[] deXOR(Stream input, uint XORpad, long length)
        {
            var result = new MemoryStream();

            var b = BitConverter.GetBytes(XORpad);
            for (int i = 0; i < length; i++)
                result.WriteByte((byte)(input.ReadByte() ^ i ^ b[i % 4]));

            return result.ToArray();
        }

        public static byte[] reXOR(Stream input, uint XORpad, long length) => deXOR(input, XORpad, length);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic;
        public int hashTableOffset;
        public int fileEntryOffset;
        public long dataOffset;
        public long infoSecSize;
        public int hold0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TableHeader
    {
        public int tableSize;
        public int entryCount;
        public long hold0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DirectoryEntry
    {
        public uint directoryHash;
        public int fileCount;
        public int unk1;
        public uint hold0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public long fileOffset;
        public uint XORpad;
        public long fileSize;
        public uint XORID;
        public short directoryHashID;
        public short const0;
        public uint hold0;
    }
}
