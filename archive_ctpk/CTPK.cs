using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using System.IO;
using Cetera.Image;

namespace archive_ctpk
{
    public sealed class CTPK : List<CTPK.Node>
    {
        public class Node
        {
            public String filename;
            public NodeEntry nodeEntry;
        }
        public class NodeEntry
        {
            public Entry entry;
            public uint info;
            public uint hash;
            public uint info2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public Magic magic;
            public ushort version;
            public ushort texCount;
            public uint texSecOffset;
            public uint texSecSize;
            public uint hashSecOffset;
            public uint texInfoOffset;
        }

        public class Entry
        {
            public Entry(Stream input)
            {
                using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(input, true))
                {
                    nameOffset = br.ReadUInt32();
                    texDataSize = br.ReadUInt32();
                    texOffset = br.ReadUInt32();
                    format = (Format)br.ReadUInt32();
                    width = br.ReadUInt16();
                    height = br.ReadUInt16();
                    mipLvl = br.ReadByte();
                    type = br.ReadByte();
                    unk1 = br.ReadUInt16();
                    bitmapSizeOffset = br.ReadUInt32();
                    timeStamp = br.ReadUInt32();
                }
            }
            public uint nameOffset;
            public uint texDataSize;
            public uint texOffset;
            public Format format;
            public ushort width;
            public ushort height;
            public byte mipLvl;
            public byte type;
            public ushort unk1;
            public uint bitmapSizeOffset;
            public uint timeStamp;
        }

        public Header header;

        public CTPK(String filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                header = br.ReadStruct<Header>();

                for (int i = 0; i < header.texCount; i++)
                {
                    Add(new Node()
                    {
                        filename = "",
                        nodeEntry = new NodeEntry()
                    });
                }

                for (int i = 0; i < header.texCount; i++)
                {
                    //Entry
                    br.BaseStream.Position = 0x20 + i * 0x20;
                    this[i].nodeEntry.entry = new Entry(br.BaseStream);
                }

                for (int i = 0; i < header.texCount; i++)
                {
                    //Info
                    this[i].nodeEntry.info = br.ReadUInt32();
                }

                for (int i = 0; i < header.texCount; i++)
                {
                    //NameList
                    br.BaseStream.Position = this[i].nodeEntry.entry.nameOffset;
                    this[i].filename = readASCII(br.BaseStream);
                }

                br.BaseStream.Position = header.hashSecOffset;
                for (int i = 0; i < header.texCount; i++)
                {
                    //HashList
                    uint hash = br.ReadUInt32();
                    int idx = br.ReadInt32();
                    this[idx].nodeEntry.hash = hash;
                }

                br.BaseStream.Position = header.texInfoOffset;
                for (int i = 0; i < header.texCount; i++)
                {
                    //TexInfo
                    this[i].nodeEntry.info2 = br.ReadUInt32();
                }
            }
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
