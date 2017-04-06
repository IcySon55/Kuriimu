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
			entries = Enumerable.Range(0, entryCount).Select(_ => hpiBr.ReadStruct<Entry>()).OrderBy(e => e.offset).ToList();

            //Names
            hpb = File.OpenRead(hpbFilename);
            BinaryReaderX hpbBr = new BinaryReaderX(hpb);
            hpiBr.BaseStream.Position = header.entryListSize + header.infoSize + header.headerSize + 8;
            for (int i = 0; i < entryCount; i++)
            {
                hpbBr.BaseStream.Position = entries[i].offset;
                nodes.Add(new Node()
                {
                    filename = hpiBr.ReadCStringA(),
                    entry = entries[i],
                    fileData = new Cetera.IO.SubStream(hpbBr.BaseStream, entries[i].offset, entries[i].fileSize)
                });
            }
        }

        public void Save()
        {

        }

        public void Close()
        {
            hpi.Dispose(); hpb.Dispose();
            hpi = null; hpb = null;
        }
    }
}
