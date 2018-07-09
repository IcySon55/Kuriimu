using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kontract.Interface;
using Kontract.IO;
using System.IO;
using Kontract;
using Kontract.Compression;

namespace archive_nintendo.NCCH
{
    public class ExeFSFileInfo : ArchiveFileInfo
    {
        public bool compressed;

        public override Stream FileData
        {
            get => (compressed) ?
                new MemoryStream(RevLZ77.Decompress(base.FileData)) :
                base.FileData;
            set => base.FileData = (compressed) ?
                new MemoryStream(RevLZ77.Compress(value)) :
                value;
        }

        public override long? FileSize => (compressed) ?
            RevLZ77.Decompress(base.FileData).Length :
            base.FileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
        public byte[] rsa2048;
        public Magic magic;
        public int mediaUnits;
        public ulong partitionID;
        public short makerCode;
        public short version;
        public uint seedHashVerifier;
        public ulong programID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] logoRegionHash;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] productCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] exHeaderHash;
        public int exHeaderSize;
        public int reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8)]
        public byte[] ncchFlags;
        public int plainRegionOffset;
        public int plainRegionSize;
        public int logoRegionOffset;
        public int logoRegionSize;
        public int exeFSOffset;
        public int exeFSSize;
        public int exeFSHashRegSize;
        public int reserved3;
        public int romFSOffset;
        public int romFSSize;
        public int romFSHashRegSize;
        public int reserved4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] exeFSSuperBlockHash;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] romFSSuperBlockHash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ExHeader
    {
        public SCI sci;
        public ACI aci;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
        public byte[] accessDescSig;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
        public byte[] ncchHdrRsaPubKey;
        public ACI aciLimits;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SCI
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x8)]
            public string appTitle;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x5)]
            public byte[] reserved1;
            public byte flag;
            public short remasterVersion;
            public CodeSetInfo textCodeSetInfo;
            public int stackSize;
            public CodeSetInfo readOnlyCodeSetInfo;
            public int reserved2;
            public CodeSetInfo dataCodeSetInfo;
            public int bssSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x180)]
            public byte[] dependencyModule;
            public SystemInfo systemInfo;

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public class CodeSetInfo
            {
                public int address;
                public int physRegionSize;
                public int size;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public class SystemInfo
            {
                public long saveDataSize;
                public long jumpID;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x30)]
                public byte[] reserved1;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ACI
        {
            public ARM11LocalSysCapabilities arm11localCap;
            public ARM11KernelCapabilities arm11kernelCap;
            public ARM9AccessControl arm9accControl;

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public class ARM11LocalSysCapabilities
            {
                public ulong programID;
                public int coreVersion;
                public short flag12;
                public byte flag0;
                public byte priority;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
                public byte[] resourceLimitDesc;
                public StorageInfo storageInfo;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
                public byte[] srvAccControl;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
                public byte[] extSrvAccControl;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xF)]
                public byte[] reserved1;
                public byte resourceLimitCategory;

                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                public class StorageInfo
                {
                    public ulong extDataID;
                    public ulong sysSaveDataID;
                    public ulong storageAccUniqueIDs;
                    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x7)]
                    public byte[] fsAccInfo;
                    public byte attr;
                }
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public class ARM11KernelCapabilities
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x70)]
                public byte[] descriptors;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
                public byte[] reserved1;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public class ARM9AccessControl
            {
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xF)]
                public byte[] descriptors;
                public byte arm9DescVersion;
            }
        }
    }
    
    public class ExeFS
    {
        public ExeFS(Stream instream)
        {
            using (var br = new BinaryReaderX(instream, true))
            {
                fileHeader = br.ReadMultiple<ExeFSHeader>(0xA);
                reserved1 = br.ReadBytes(0x20);
                fileHeaderHash = new List<byte[]>();
                for (int i = 0; i < 0xA; i++)
                    fileHeaderHash.Add(br.ReadBytes(0x20));
            }
        }

        public List<ExeFSHeader> fileHeader;
        public byte[] reserved1;
        public List<byte[]> fileHeaderHash;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ExeFSHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x8)]
            public string name;
            public int offset;
            public int size;
        }
    }
}
