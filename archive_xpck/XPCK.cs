using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using Cetera.Compression;
using System.IO;

namespace archive_xpck
{
    public sealed class XPCK : List<XPCK.Node>
    {
        public class Node
        {
            public String filename;
            public Entry entry;
            public byte[] fileData;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public Magic magic;
            public byte fileCount;
            public byte unk1;
            ushort tmp1;
            ushort tmp2;
            ushort tmp3;
            ushort tmp4;
            ushort tmp5;
            uint tmp6;

            public ushort fileInfoOffset => (ushort)(tmp1 * 4);
            public ushort filenameTableOffset => (ushort)(tmp2 * 4);
            public ushort dataOffset => (ushort)(tmp3 * 4);
            public ushort fileInfoSize => (ushort)(tmp4 * 4);
            public ushort filenameTableSize => (ushort)(tmp5 * 4);
            public uint dataSize => tmp6 * 4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            public uint crc32;
            ushort unk1;
            ushort tmp1;
            public uint fileSize;

            public ushort fileOffset => (ushort)(tmp1 * 4);
            /*public uint crc32;
            uint mer1;
            uint mer2;

            public uint offset => (mer1 >> 16) * 4;
            public uint fileSize => (mer2 & 0xFFFF) | (((mer2 >> 16) % 8) << 8 | ((mer2 >> 16) / 8));*/
        }

        public Header header;
        public List<Entry> entries;
        public List<String> nameList;

        public XPCK(String filename)
        {
            using (BinaryReaderX tmpbr = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                Stream input;
                if (tmpbr.ReadString(4) == "XPCK")
                {
                    tmpbr.BaseStream.Position = 0;
                    input = tmpbr.BaseStream;
                }
                else
                {
                    tmpbr.BaseStream.Position = 0;
                    byte[] decomp = CriWare.GetDecompressedBytes(tmpbr.BaseStream);
                    input = new MemoryStream(decomp);
                }

                using (BinaryReaderX br = new BinaryReaderX(input))
                {
                    //Header
                    header = br.ReadStruct<Header>();

                    //fileInfo
                    br.BaseStream.Position = header.fileInfoOffset;
                    entries = new List<Entry>();
                    for (int i = 0; i < header.fileCount; i++)
                    {
                        entries.Add(br.ReadStruct<Entry>());
                    }

                    SortEntries(entries);

                    //nameList
                    br.BaseStream.Position = header.filenameTableOffset;
                    byte[] nl = CriWare.GetDecompressedBytes(new MemoryStream(br.ReadBytes(header.filenameTableSize)));
                    nameList = new List<String>();
                    using (BinaryReaderX br2 = new BinaryReaderX(new MemoryStream(nl)))
                    {
                        while (br2.BaseStream.Position < nl.Length)
                        {
                            nameList.Add(readASCII(br2.BaseStream));
                        }
                    }

                    for (int i = 0; i < header.fileCount; i++)
                    {
                        br.BaseStream.Position = entries[i].fileOffset + header.dataOffset;
                        Add(new Node()
                        {
                            filename = nameList[i],
                            entry = entries[i],
                            fileData = br.ReadBytes((int)entries[i].fileSize)
                        });
                    }

                    int t = 0;
                }
            }
        }
        public static void SortEntries(List<Entry> entries)
        {
            //BubbleSort
            bool sorted;
            Entry help;
            do
            {
                sorted = true;
                for (int i = 0; i < entries.Count - 1; i++)
                {
                    if (entries[i].fileOffset > entries[i + 1].fileOffset)
                    {
                        sorted = false;
                        help = entries[i];
                        entries[i] = entries[i + 1];
                        entries[i + 1] = help;
                    }
                }
            } while (sorted == false);
        }
        public static String readASCII(Stream input)
        {
            Encoding encode = Encoding.GetEncoding("ascii");
            String result = "";

            using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
            {
                var letters = br.ReadBytes(1);
                bool nul;
                do
                {
                    nul = true;
                    result += encode.GetString(letters);
                    letters = br.ReadBytes(1);
                    for (int i = 0; i < 1; i++) if (letters[i] != 0) nul = false;
                } while (nul == false);

                return result;
            }
        }
    }
}