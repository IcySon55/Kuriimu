using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Cetera.Compression;
using KuriimuContract;

namespace archive_hpi_hpb
{
    public sealed class HPIHPB
    {
        public class Node
        {
            public String filename;
            public Entry entry;
            public Cetera.IO.SubStream fileData;
        }
        public List<HPIHPB.Node> nodes = new List<HPIHPB.Node>();

        private FileStream hpi = null;
        private FileStream hpb = null;

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
            public uint unk1;
            public uint offset;
            public uint fileSize;
            public uint unk2;
        }

        public Header header;
        public List<Entry> entries;

        public HPIHPB(String filename, String otherFilename)
        {
            //which file is what?
            String hpiFilename;
            String hpbFilename;
            if (filename.EndsWith(".HPI"))
            {
                hpiFilename = filename;
                hpbFilename = otherFilename;
            }
            else
            {
                hpiFilename = otherFilename;
                hpbFilename = filename;
            }

            hpi = File.OpenRead(hpiFilename);
            BinaryReaderX hpiBr = new BinaryReaderX(hpi);
            //Header
            header = hpiBr.ReadStruct<Header>();

            //infoList??? - not mapped

            //Entries
            hpiBr.BaseStream.Position = header.infoSize + header.headerSize + 8;
            int entryCount = header.entryListSize / 0x10;
            entries = new List<Entry>();
            for (int i = 0; i < entryCount; i++)
            {
                entries.Add(hpiBr.ReadStruct<Entry>());
            }
            SortEntries(entries);

            //Names
            hpb = File.OpenRead(hpbFilename);
            BinaryReaderX hpbBr = new BinaryReaderX(hpb);
            hpiBr.BaseStream.Position = header.entryListSize + header.infoSize + header.headerSize + 8;
            for (int i = 0; i < entryCount; i++)
            {
                hpbBr.BaseStream.Position = entries[i].offset;
                nodes.Add(new Node()
                {
                    filename = readASCII(hpiBr.BaseStream),
                    entry = entries[i],
                    fileData = new Cetera.IO.SubStream(hpbBr.BaseStream, entries[i].offset, entries[i].fileSize)
                });
            }
        }

        public void Save()
        {

        }

        //--------------HAS TO BE REPLACED!!---------------------START
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
        //--------------HAS TO BE REPLACED!!---------------------END

        public String readASCII(Stream input)
        {
            BinaryReaderX br = new BinaryReaderX(input);

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

        public void Close()
        {
            hpi.Dispose(); hpb.Dispose();
            hpi = null; hpb = null;
        }
    }
}
