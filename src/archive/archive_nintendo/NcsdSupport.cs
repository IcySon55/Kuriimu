using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract;
using Kontract.IO;

namespace archive_nintendo.DDSFS
{
    public class NCSDFileInfo : ArchiveFileInfo
    {
        public int PartitionID;
    }

    public class NCSDHeader
    {
        public NCSDHeader(Stream input)
        {
            const int mediaUnitSize = 0x200;

            using (var br = new BinaryReaderX(input, true))
            {
                rsa = br.ReadBytes(0x100);
                magic = br.ReadStruct<Magic>();
                ncsdSize = br.ReadUInt32() * mediaUnitSize;
                mediaID = br.ReadUInt64();
                partFSType = br.ReadUInt64();
                partCryptType = br.ReadUInt64();

                partEntries = new List<PartEntry>();
                for (int i = 0; i < 8; i++)
                    partEntries.Add(new PartEntry(br.ReadUInt32() * mediaUnitSize, br.ReadUInt32() * mediaUnitSize));

                shaHash = br.ReadBytes(0x20);
                addHeaderSize = br.ReadUInt32();
                secZeroOffset = br.ReadUInt32();
                partFlags = br.ReadBytes(8);

                partIDTable = new List<ulong>();
                for (int i = 0; i < 8; i++)
                    partIDTable.Add(br.ReadUInt64());

                reserved = br.ReadBytes(0x2E);
                ncsdVerification = br.ReadByte();
                saveCrypto = br.ReadByte();

                cardInfoHeader = br.ReadStruct<CardInfoHeader>();
            }
        }

        public void Write(Stream instream)
        {
            using (var bw = new BinaryWriterX(instream, true))
            {
                bw.Write(rsa);
                bw.WriteStruct(magic);
                bw.Write(ncsdSize);
                bw.Write(mediaID);
                bw.Write(partFSType);
                bw.Write(partCryptType);

                foreach (var part in partEntries)
                {
                    bw.Write(part.offset);
                    bw.Write(part.size);
                }

                bw.Write(shaHash);
                bw.Write(addHeaderSize);
                bw.Write(secZeroOffset);
                bw.Write(partFlags);
                foreach (var id in partIDTable)
                    bw.Write(id);

                bw.Write(reserved);
                bw.Write(ncsdVerification);
                bw.Write(saveCrypto);

                bw.WriteStruct(cardInfoHeader);
            }
        }

        public byte[] rsa;
        public Magic magic;     //NCSD
        public uint ncsdSize;
        public ulong mediaID;
        public ulong partFSType;
        public ulong partCryptType;
        public List<PartEntry> partEntries;

        public byte[] shaHash;
        public uint addHeaderSize;
        public uint secZeroOffset;
        public byte[] partFlags;
        public List<ulong> partIDTable;
        public byte[] reserved;
        public byte ncsdVerification;
        public byte saveCrypto;

        public CardInfoHeader cardInfoHeader;

        public class PartEntry
        {
            public PartEntry(uint offset, uint size)
            {
                this.offset = offset;
                this.size = size;
            }

            public uint offset;
            public uint size;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CardInfoHeader
    {
        public int card2WAddr;
        public int cardInfoBitmask;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x108)]
        public byte[] reserved1;
        public short titleVersion;
        public short cardRev;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xCEC)]
        public byte[] reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] cardSeedKeyY;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] encCardSeed;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] cardSeedAESMAC;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xC)]
        public byte[] cardSeedNonce;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xC4)]
        public byte[] reserved3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
        public byte[] copyFirstNCCHHeader;
    }
}
