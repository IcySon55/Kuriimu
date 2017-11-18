using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.DDSFS
{
    public class DDSFSFileInfo : ArchiveFileInfo
    {

    }

    public class NCSDHeader
    {
        public NCSDHeader(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                rsa = br.ReadBytes(0x100);
                magic = br.ReadStruct<Magic>();
                ncsdSize = br.ReadUInt32() * 0x200;
                mediaID = br.ReadUInt64();
                partFSType = br.ReadUInt64();
                partCryptType = br.ReadUInt64();

                partEntries = new List<PartEntry>();
                for (int i = 0; i < 8; i++) partEntries.Add(new PartEntry(br.ReadUInt32(), br.ReadUInt32()));

                shaHash = br.ReadBytes(0x20);
                addHeaderSize = br.ReadUInt32();
                secZeroOffset = br.ReadUInt32();
                partFlags = br.ReadUInt64();

                partIDTable = new List<ulong>();
                for (int i = 0; i < 8; i++) partIDTable.Add(br.ReadUInt64());

                reserved = br.ReadBytes(0x2E);
                ncsdVerification = br.ReadByte();
                saveCrypto = br.ReadByte();

                var tmpAddr = br.ReadUInt32();
                card2WAddr = (tmpAddr != 0xffffffff) ? tmpAddr * 0x200 : tmpAddr;
                cardInfoBitmask = br.ReadUInt32();
                res1 = br.ReadBytes(0x106);
                titleVersion = br.ReadUInt16();
                cardRev = br.ReadUInt16();
                res2 = br.ReadBytes(0xcee);
                cardSeedKeyY = br.ReadBytes(0x10);
                encCardSeed = br.ReadBytes(0x10);
                cardSeedAESMAC = br.ReadBytes(0x10);
                cardSeedNonce = br.ReadBytes(0xc);
                res3 = br.ReadBytes(0xc4);
                copyFirstNCCHHeader = br.ReadBytes(0x100);
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
        public ulong partFlags;
        public List<ulong> partIDTable;
        public byte[] reserved;
        public byte ncsdVerification;
        public byte saveCrypto;

        public uint card2WAddr;
        public uint cardInfoBitmask;
        public byte[] res1;
        public ushort titleVersion;
        public ushort cardRev;
        public byte[] res2;
        public byte[] cardSeedKeyY;
        public byte[] encCardSeed;
        public byte[] cardSeedAESMAC;
        public byte[] cardSeedNonce;
        public byte[] res3;
        public byte[] copyFirstNCCHHeader; //without rsa sigature

        public class PartEntry
        {
            public PartEntry(uint offset, uint size)
            {
                this.offset = offset * 0x200;
                this.size = size * 0x200;
            }

            public uint offset;
            public uint size;
        }
    }

    public class NCCHHeader
    {
        public NCCHHeader(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                rsa = br.ReadBytes(0x100);
                magic = br.ReadStruct<Magic>();
                contSize = br.ReadUInt32() * 0x200;
                partID = br.ReadUInt64();
                makerCode = br.ReadUInt16();
                version = br.ReadUInt16();
                comp = br.ReadUInt32();
                progID = br.ReadUInt64();
                res1 = br.ReadBytes(0x10);
                logoRegHash = br.ReadBytes(0x20);
                prodCode = br.ReadBytes(0x10);

                exHeaderHash = br.ReadBytes(0x20);
                exHeaderSize = br.ReadUInt32();

                res2 = br.ReadUInt32();
                flags = br.ReadUInt64();

                plainRegOffset = br.ReadUInt32() * 0x200;
                plainRegSize = br.ReadUInt32() * 0x200;

                logoRegOffset = br.ReadUInt32() * 0x200;
                logoRegSize = br.ReadUInt32() * 0x200;

                exeFsOffset = br.ReadUInt32() * 0x200;
                exeFsSize = br.ReadUInt32() * 0x200;
                exeFsHashRegSize = br.ReadUInt32() * 0x200;
                res3 = br.ReadUInt32();

                romFsOffset = br.ReadUInt32() * 0x200;
                romFsSize = br.ReadUInt32() * 0x200;
                romFsHashRegSize = br.ReadUInt32() * 0x200;
                res4 = br.ReadUInt32();

                exeFsSuperblHash = br.ReadBytes(0x20);
                romFsSuperblHash = br.ReadBytes(0x20);
            }
        }

        public byte[] rsa;
        public Magic magic;     //NCCH
        public uint contSize;
        public ulong partID;
        public ushort makerCode;
        public ushort version;
        public uint comp;
        public ulong progID;
        public byte[] res1;
        public byte[] logoRegHash;
        public byte[] prodCode;

        public byte[] exHeaderHash;
        public uint exHeaderSize;

        public uint res2;
        public ulong flags;

        public uint plainRegOffset;
        public uint plainRegSize;

        public uint logoRegOffset;
        public uint logoRegSize;

        public uint exeFsOffset;
        public uint exeFsSize;
        public uint exeFsHashRegSize;
        public uint res3;

        public uint romFsOffset;
        public uint romFsSize;
        public uint romFsHashRegSize;
        public uint res4;

        public byte[] exeFsSuperblHash;
        public byte[] romFsSuperblHash;
    }

    public class ExeFsFileEntry
    {
        public ExeFsFileEntry(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                filename = br.ReadString(8);
                offset = br.ReadUInt32();
                size = br.ReadUInt32();
            }
        }

        public string filename;
        public uint offset;
        public uint size;
    }

    public class IVFCHeader
    {
        public IVFCHeader(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                magic = br.ReadStruct<Magic>();
                magicNr = br.ReadUInt32();
                masterHashSize = br.ReadUInt32();

                lv1LogOffset = br.ReadUInt64();
                lv1HashDataSize = br.ReadUInt64();
                lv1BlockSize = br.ReadUInt32();
                res1 = br.ReadUInt32();

                lv2LogOffset = br.ReadUInt64();
                lv2HashDataSize = br.ReadUInt64();
                lv2BlockSize = br.ReadUInt32();
                res2 = br.ReadUInt32();

                lv3LogOffset = br.ReadUInt64();
                lv3HashDataSize = br.ReadUInt64();
                lv3BlockSize = br.ReadUInt32();
                res3 = br.ReadUInt32();

                headerSize = br.ReadUInt32();
                res4 = br.ReadUInt32();
            }
        }

        public Magic magic;     //IVC
        public uint magicNr;    //0x10000
        public uint masterHashSize;

        public ulong lv1LogOffset;
        public ulong lv1HashDataSize;
        public uint lv1BlockSize; //in log2
        public uint res1;

        public ulong lv2LogOffset;
        public ulong lv2HashDataSize;
        public uint lv2BlockSize; //in log2
        public uint res2;

        public ulong lv3LogOffset;
        public ulong lv3HashDataSize;
        public uint lv3BlockSize; //in log2
        public uint res3;

        public uint headerSize;
        public uint res4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Level3Header
    {
        public int headerLength;

        public int dirHashTabOffset;
        public int dirHashTabLength;

        public int dirMetaTabOffset;
        public int dirMetaTabLength;

        public int fileHashTabOffset;
        public int fileHashTabLength;

        public int fileMetaTabOffset;
        public int fileMetaTabLength;

        public int fileDataOffset;
    }
}
