using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Interface;
using Kontract;
using System.IO;
using Kontract.IO;

namespace archive_nintendo.CIA
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public int headerSize;
        public short type;
        public short version;
        public int certChainSize;
        public int ticketSize;
        public int tmdSize;
        public int metaSize;
        public long contentSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x2000)]
        public byte[] contentIndex;
    }

    public class CertChain
    {
        public CertChain(Stream instream)
        {
            CA = new Certificate(instream);
            TMDVerfier = new Certificate(instream);
            TicketVerifier = new Certificate(instream);
        }

        public void Write(Stream input)
        {
            CA.Write(input);
            TMDVerfier.Write(input);
            TicketVerifier.Write(input);
        }

        public Certificate CA;
        public Certificate TMDVerfier;
        public Certificate TicketVerifier;

        public class Certificate
        {
            public Certificate(Stream instream)
            {
                using (var br = new BinaryReaderX(instream, true, ByteOrder.BigEndian))
                {
                    sigType = br.ReadInt32();
                    var (sigSize, padSize) = Support.GetSignatureSizes(sigType);
                    signature = br.ReadBytes(sigSize);
                    br.BaseStream.Position += padSize;
                    issuer = br.ReadString(0x40);
                    keyType = br.ReadInt32();
                    name = br.ReadString(0x40);

                    var (pubKeySize, keyPadSize) = Support.GetPublicKeySizes(keyType);
                    publicKey = br.ReadBytes(pubKeySize);
                    unk1 = br.ReadInt32();
                    br.BaseStream.Position += keyPadSize;
                }
            }

            public void Write(Stream input)
            {
                using (var bw = new BinaryWriterX(input, true, ByteOrder.BigEndian))
                {
                    bw.Write(sigType);
                    bw.Write(signature);
                    bw.WritePadding(Support.GetSignatureSizes(sigType).Item2);

                    bw.Write(GetFixedStringBA(issuer, 0x40));

                    bw.Write(keyType);
                    bw.Write(GetFixedStringBA(name, 0x40));

                    bw.Write(new byte[publicKey.Length]);
                    bw.Write(unk1);
                    bw.WritePadding(Support.GetPublicKeySizes(keyType).Item2);
                }
            }

            private byte[] GetFixedStringBA(string input, int fixedLength)
            {
                var inputBA = System.Text.Encoding.ASCII.GetBytes(input);
                var final = new byte[0x40];
                inputBA.CopyTo(final, 0);
                return final;
            }

            public int sigType;
            public byte[] signature;
            public string issuer;
            public int keyType;
            public string name;
            public int unk1;
            public byte[] publicKey;
        }
    }

    public class Ticket
    {
        public Ticket(Stream instream)
        {
            using (var br = new BinaryReaderX(instream, true, ByteOrder.BigEndian))
            {
                sigType = br.ReadInt32();
                var (sigSize, padSize) = Support.GetSignatureSizes(sigType);
                signature = br.ReadBytes(sigSize);
                br.BaseStream.Position += padSize;

                ticketData = br.ReadStruct<TicketData>();
            }
        }

        public void Write(Stream input)
        {
            using (var bw = new BinaryWriterX(input, true, ByteOrder.BigEndian))
            {
                bw.Write(sigType);
                bw.Write(new byte[signature.Length]);
                bw.WritePadding(Support.GetSignatureSizes(sigType).Item2);

                bw.WriteStruct(ticketData);
            }
        }

        public int sigType;
        public byte[] signature;
        public TicketData ticketData;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class TicketData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string issuer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3C)]
            public byte[] eccPublicKey;
            public byte version;
            public byte caCrlVersion;
            public byte signerCrlVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
            public byte[] titleKey;
            public byte reserved1;
            public ulong ticketID;
            public uint consoleID;
            public ulong titleID;
            public short reserved2;
            public short ticketTitleVersion;
            public ulong reserved3;
            public byte licenseType;
            public byte keyYIndex;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x2A)]
            public byte[] reserved4;
            public uint eshopAccID;
            public byte reserved5;
            public byte audit;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x42)]
            public byte[] reserved6;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)]
            public byte[] limits;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xAC)]
            public byte[] contentIndex;
        }
    }

    public class TMD
    {
        public TMD(Stream instream)
        {
            using (var br = new BinaryReaderX(instream, true, ByteOrder.BigEndian))
            {
                sigType = br.ReadInt32();
                var (sigSize, padSize) = Support.GetSignatureSizes(sigType);
                signature = br.ReadBytes(sigSize);
                br.BaseStream.Position += padSize;

                header = br.ReadStruct<TMDHeader>();
                contentInfoRecord = br.ReadMultiple<ContentInfoRecord>(0x40);
                contentChunkRecord = br.ReadMultiple<ContentChunkRecord>(header.contentCount);
            }
        }

        public int Write(Stream input)
        {
            using (var bw = new BinaryWriterX(input, true, ByteOrder.BigEndian))
            {
                var inputOffset = bw.BaseStream.Position;

                bw.Write(sigType);
                bw.Write(new byte[signature.Length]);
                bw.WritePadding(Support.GetSignatureSizes(sigType).Item2);
                var TMDHeaderOffset = bw.BaseStream.Position;

                bw.BaseStream.Position += 0xC4;
                var CIROffset = bw.BaseStream.Position;

                bw.BaseStream.Position += 0x40 * 0x24;
                var startOffset = bw.BaseStream.Position;
                foreach (var ccr in contentChunkRecord)
                    bw.WriteStruct(ccr);
                var endOffset = bw.BaseStream.Position;

                contentChunkRecord[0].sha256 = Kontract.Hash.SHA256.Create(bw.BaseStream, startOffset, endOffset - startOffset);
                bw.BaseStream.Position = CIROffset;
                foreach (var cir in contentInfoRecord)
                    bw.WriteStruct(cir);

                bw.BaseStream.Position = TMDHeaderOffset;
                header.sha256 = Kontract.Hash.SHA256.Create(bw.BaseStream, CIROffset, 0x40 * 0x24);
                bw.WriteStruct(header);

                bw.BaseStream.Position = endOffset;

                return (int)(endOffset - inputOffset);
            }
        }

        public int sigType;
        public byte[] signature;
        public TMDHeader header;
        public List<ContentInfoRecord> contentInfoRecord;
        public List<ContentChunkRecord> contentChunkRecord;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class TMDHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string issuer;
            public byte version;
            public byte caCrlVersion;
            public byte signerCrlVersion;
            public byte reserved1;
            public long systemVersion;
            public ulong titleID;
            public int titleType;
            public short groupID;
            public int saveDataSize;
            public int srlPrivateSaveDataSize;
            public int reserved2;
            public byte srlFlag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x31)]
            public byte[] reserved3;
            public int accessRights;
            public short titleVersion;
            public short contentCount;
            public short bootContent;
            public short padding;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public byte[] sha256;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ContentInfoRecord
        {
            public short contentChunkOffset;
            public short contentChunkCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public byte[] sha256;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ContentChunkRecord
        {
            public int contentID;
            public short contentIndex;
            public short contentType;
            public long contentSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public byte[] sha256;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Meta
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x180)]
        public byte[] titleIDDependency;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x180)]
        public byte[] reserved1;
        public int coreVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xFC)]
        public byte[] reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x36C0)]
        public byte[] iconData;
    }

    public static class Support
    {
        public static (int, int) GetSignatureSizes(int sigType)
        {
            int sigSize = 0;
            int padSize = 0;
            switch (sigType)
            {
                case 0x010003:
                    sigSize = 0x200;
                    padSize = 0x3C;
                    break;
                case 0x010004:
                    sigSize = 0x100;
                    padSize = 0x3C;
                    break;
                case 0x010005:
                    sigSize = 0x3c;
                    padSize = 0x40;
                    break;
            }

            return (sigSize, padSize);
        }

        public static (int, int) GetPublicKeySizes(int pubKeyType)
        {
            int pubKeySize = 0;
            int padSize = 0;
            switch (pubKeyType)
            {
                case 0:
                    pubKeySize = 0x204;
                    padSize = 0x34;
                    break;
                case 1:
                    pubKeySize = 0x104;
                    padSize = 0x34;
                    break;
                case 2:
                    pubKeySize = 0x3C;
                    padSize = 0x3C;
                    break;
            }

            return (pubKeySize, padSize);
        }
    }
}
