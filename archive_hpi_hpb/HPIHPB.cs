using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using System.IO;
using Cetera.Image;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB : List<HPIHPB.Node>
    {
        public class Node
        {
            public String filename;
            public Entry entry;
            public MemoryStream fileData;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public Magic8 magic;
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

        public HPIHPB(String filename, String hpbFilename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
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
                using (BinaryReaderX br2 = new BinaryReaderX(File.Open(hpbFilename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    br.BaseStream.Position = header.entryListSize + header.infoSize + header.headerSize + 8;
                    for (int i = 0; i < entryCount; i++)
                    {
                        br2.BaseStream.Position = entries[i].offset;
                        Add(new Node()
                        {
                            filename = readASCII(br.BaseStream),
                            entry = entries[i],
                            fileData = new MemoryStream(br2.ReadBytes((int)entries[i].fileSize))
                        });
                    }
                }
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
            using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
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
