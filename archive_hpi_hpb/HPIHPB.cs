using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cetera.Compression;
using Cetera.IO;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB : List<HPIHPB.Node>
    {
        public class Node
        {
            public String filename;
            public Entry entry;
            public Stream fileData;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public KuriimuContract.Magic8 magic;
            public int headerSize;  //without magic
            int unk1;
            short unk2;
            short tmp1;
            int tmp2;

            public int infoSize => tmp1 << 2;
            public int entryListSize => tmp2 << 4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Entry
        {
            uint unk1;
            public uint offset;
            public uint fileSize;
            uint unk2;
        }

        public Header header;
        public List<Entry> entries;

        Stream stream, stream2;

        public HPIHPB(String filename, String hpbFilename)
        {
            stream = File.OpenRead(filename);

            using (BinaryReaderX br = new BinaryReaderX(stream))
            {
                //Header
                header = br.ReadStruct<Header>();

                //infoList??? - not mapped

                //Entries
                br.BaseStream.Position = header.infoSize + header.headerSize + 8;
                int entryCount = header.entryListSize / 0x10;
                entries = new List<Entry>();
                for (int i = 0; i < entryCount; i++)
                {
                    entries.Add(br.ReadStruct<Entry>());
                }
                SortEntries(entries);

                //Names
                stream2 = File.OpenRead(hpbFilename);
                using (BinaryReaderX br2 = new BinaryReaderX(stream2, true))
                {
                    br.BaseStream.Position = header.entryListSize + header.infoSize + header.headerSize + 8;
                    for (int i = 0; i < entryCount; i++)
                    {
                        br2.BaseStream.Position = entries[i].offset;
                        try
                        {
                            Add(new Node()
                            {
                                filename = readASCII(br.BaseStream),
                                entry = entries[i],
                                fileData = new MemoryStream(DecompressACMP(br2.ReadBytes((int)entries[i].fileSize)))
                                //fileData = new SubStream(stream2, entries[i].offset, entries[i].fileSize)
                            });
                        }
                        catch { throw new Exception(i.ToString()); }
                    }
                }
            }
        }

        public byte[] DecompressACMP(byte[] acmp)
        {
            using (BinaryReaderX br = new BinaryReaderX(new MemoryStream(acmp)))
            {
                KuriimuContract.Magic magic = br.ReadStruct<KuriimuContract.Magic>();
                if (magic != "ACMP") return acmp;

                int compSize = br.ReadInt32();
                int dataOffset = br.ReadInt32();
                br.ReadInt32();
                int decompSize = br.ReadInt32();
                br.BaseStream.Position = dataOffset;
                return RevLZ77.Decompress(br.ReadBytes(compSize), (uint)decompSize);
            }
        }

        public void SortEntries(List<Entry> entries)
        {
            //BubbleSort
            Entry help;
            bool sort;
            do
            {
                sort = true;
                for (int i = 0; i < entries.Count - 1; i++)
                {
                    if (entries[i].offset > entries[i + 1].offset)
                    {
                        sort = false;
                        help = entries[i];
                        entries[i] = entries[i + 1];
                        entries[i + 1] = help;
                    }
                }
            } while (sort == false);
        }

        public String readASCII(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                String result = "";
                Encoding ascii = Encoding.GetEncoding("ascii");

                byte[] character = br.ReadBytes(1);
                while (character[0] != 0x00)
                {
                    result += ascii.GetString(character);
                    character = br.ReadBytes(1);
                }

                return result;
            }
        }
    }
}
