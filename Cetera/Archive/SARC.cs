using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Kuriimu.IO;

namespace Cetera.Archive
{
    public sealed class SARC : List<SARC.Node>
    {
        public SARCHeader sarcHeader;
        public SimplerSARCHeader ssarcHeader;
        public SFATHeader sfatHeader;
        public SFNTHeader sfntHeader;

        [DebuggerDisplay("{fileName}")]
        public class Node
        {
            public State state;
            public SFATNode nodeEntry;
            public SimplerSFATNode sNodeEntry;
            public String fileName;
        }
        public enum State : byte
        {
            Normal = 0,
            Simpler = 1
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SimplerSARCHeader
        {
            String4 magic;
            public uint nodeCount;
            uint unk1;
            uint unk2;
        }
        public class SimplerSFATNode
        {
            public SimplerSFATNode(Stream input)
            {
                using (BinaryReaderX br = new BinaryReaderX(input, true))
                {
                    hash = br.ReadUInt32();
                    dataStart = br.ReadUInt32();
                    dataLength = br.ReadUInt32();
                    unk1 = br.ReadUInt32();
                }
            }
            public uint hash;
            public uint dataStart;
            public uint dataLength;
            public uint unk1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SARCHeader
        {
            String4 magic;
            ushort headerSize;
            ByteOrder byteOrder;
            uint fileSize;
            public uint dataOffset;
            uint unk1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SFATHeader
        {
            String4 magic;
            ushort headerSize;
            public ushort nodeCount;
            uint hashMultiplier;
        }

        public class SFATNode
        {
            public SFATNode(Stream input)
            {
                using (BinaryReaderX br = new BinaryReaderX(input, true))
                {
                    nameHash = br.ReadUInt32();
                    SFNTOffset = br.ReadUInt16();
                    unk1 = br.ReadUInt16();
                    dataStart = br.ReadUInt32();
                    dataEnd = br.ReadUInt32();
                }
            }
            public uint nameHash;
            public ushort SFNTOffset;
            public ushort unk1;
            public uint dataStart;
            public uint dataEnd;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SFNTHeader
        {
            String4 magic;
            ushort headerSize;
            ushort unk1;
        }

        public unsafe uint CalcNodeHash(String name, int hashMultiplier)
        {
            uint result = 0;

            for (int i = 0; i < name.Length; i++)
            {
                result = (uint)(name[i] + (result * hashMultiplier));
            }

            return result;
        }

        public String readASCII(BinaryReaderX br)
        {
            String result = "";
            Encoding ascii = Encoding.GetEncoding("ascii");

            byte[] character = br.ReadBytes(1);
            while (character[0] != 0x00)
            {
                result += ascii.GetString(character);
                character = br.ReadBytes(1);
            }
            br.BaseStream.Position -= 1;

            return result;
        }

        public SARC(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                br.BaseStream.Position = 6;
                ushort ind = br.ReadUInt16();
                if (ind != 0xfeff && ind != 0xfffe)
                {
                    br.BaseStream.Position = 0;
                    SimplerSARC(br.BaseStream);
                }
                else
                {
                    br.BaseStream.Position = 0;

                    sarcHeader = br.ReadStruct<SARCHeader>();
                    sfatHeader = br.ReadStruct<SFATHeader>();

                    for (int i = 0; i < sfatHeader.nodeCount; i++)
                    {
                        Add(new Node());
                        this[i].state = State.Normal;
                        this[i].nodeEntry = new SFATNode(br.BaseStream);
                    }

                    sfntHeader = br.ReadStruct<SFNTHeader>();

                    for (int i = 0; i < sfatHeader.nodeCount; i++)
                    {
                        this[i].fileName = readASCII(br);

                        byte tmp;
                        do
                        {
                            tmp = br.ReadByte();
                        } while (tmp == 0x00);
                        br.BaseStream.Position -= 1;
                    }
                }
            }
        }

        public void SimplerSARC(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                ssarcHeader = br.ReadStruct<SimplerSARCHeader>();

                for (int i = 0; i < ssarcHeader.nodeCount; i++)
                {
                    Add(new Node());
                    this[i].state = State.Simpler;
                    this[i].sNodeEntry = new SimplerSFATNode(br.BaseStream);
                }

                for (int i = 0; i < ssarcHeader.nodeCount; i++)
                {
                    br.BaseStream.Position = this[i].sNodeEntry.dataStart;
                    this[i].fileName = "File" + i.ToString();
                }
            }
        }

        public void Save(Stream input)
        {
            int t = 0;
        }
    }
}
