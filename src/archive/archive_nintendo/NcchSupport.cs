using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using Kontract.Interface;
using Kontract.IO;
using System.IO;
using System.Text;
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

    public class RomFS
    {
        public RomFS(Stream instream)
        {
            using (var br = new BinaryReaderX(instream, true))
            {
                header = br.ReadStruct<Header>();
                br.SeekAlignment(0x10);
                masterHash = br.ReadBytes(header.masterHashSize);

                //Level 3
                br.SeekAlignment(1 << header.lv3BlockSize);
                lv3Offset = br.BaseStream.Position;
                lv3Header = br.ReadStruct<HashLevelHeader>();

                br.BaseStream.Position = lv3Offset + lv3Header.dirMetaTableOffset;
                files = new List<FileInfo>();
                ResolveDirectories(br);
            }
        }

        public Header header;
        HashLevelHeader lv3Header;
        long lv3Offset;
        public byte[] masterHash;

        public List<FileInfo> files;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic magic;
            public int magicNumber;
            public int masterHashSize;
            public long lv1LogicalOffset;
            public long lv1HashDataSize;
            public int lv1BlockSize;
            public int reserved1;
            public long lv2LogicalOffset;
            public long lv2HashDataSize;
            public int lv2BlockSize;
            public int reserved2;
            public long lv3LogicalOffset;
            public long lv3HashDataSize;
            public int lv3BlockSize;
            public int reserved3;
            public int reserved4;
            public int infoSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class HashLevelHeader
        {
            public int headerLength;
            public int dirHashTableOffset;
            public int dirHashTableSize;
            public int dirMetaTableOffset;
            public int dirMetaTableSize;
            public int fileHashTableOffset;
            public int fileHashTableSize;
            public int fileMetaTableOffset;
            public int fileMetaTableSize;
            public int fileDataOffset;
        }

        public class DirectoryMetaData
        {
            public int parentDirOffset;
            public int nextSiblingDirOffset;
            public int firstChildDirOffset;
            public int firstFileOffset;
            public int nextDirInSameBucketOffset;
            public int nameLength;
            public string name;
        }

        public class FileMetaData
        {
            public int containingDirOffset;
            public int nextSiblingFileOffset;
            public long fileOffset;
            public long fileSize;
            public int nextFileInSameBucketOffset;
            public int nameLength;
            public string name;
        }

        [DebuggerDisplay("{fileName}")]
        public class FileInfo
        {
            public string fileName;
            public long fileOffset;
            public long fileSize;
        }

        void ResolveDirectories(BinaryReaderX br, string currentPath = "")
        {
            var currentDirEntry = new DirectoryMetaData
            {
                parentDirOffset = br.ReadInt32(),
                nextSiblingDirOffset = br.ReadInt32(),
                firstChildDirOffset = br.ReadInt32(),
                firstFileOffset = br.ReadInt32(),
                nextDirInSameBucketOffset = br.ReadInt32(),
                nameLength = br.ReadInt32()
            };
            currentDirEntry.name = Encoding.Unicode.GetString(br.ReadBytes(currentDirEntry.nameLength));

            //first go through all sub dirs
            if (currentDirEntry.firstChildDirOffset != -1)
            {
                br.BaseStream.Position = lv3Offset + lv3Header.dirMetaTableOffset + currentDirEntry.firstChildDirOffset;
                ResolveDirectories(br, currentPath + currentDirEntry.name + "/");
            }

            //then get all files of current dir
            if (currentDirEntry.firstFileOffset != -1)
            {
                br.BaseStream.Position = lv3Offset + lv3Header.fileMetaTableOffset + currentDirEntry.firstFileOffset;
                ResolveFiles(br, currentPath + currentDirEntry.name + "/");
            }

            //finally move to next sibling dir
            if (currentDirEntry.nextSiblingDirOffset != -1)
            {
                br.BaseStream.Position = lv3Offset + lv3Header.dirMetaTableOffset + currentDirEntry.nextSiblingDirOffset;
                ResolveDirectories(br, currentPath);
            }
        }

        void ResolveFiles(BinaryReaderX br, string currentPath = "")
        {
            var currentFileEntry = new FileMetaData
            {
                containingDirOffset = br.ReadInt32(),
                nextSiblingFileOffset = br.ReadInt32(),
                fileOffset = br.ReadInt64(),
                fileSize = br.ReadInt64(),
                nextFileInSameBucketOffset = br.ReadInt32(),
                nameLength = br.ReadInt32()
            };
            currentFileEntry.name = Encoding.Unicode.GetString(br.ReadBytes(currentFileEntry.nameLength));

            //Add current file
            files.Add(new FileInfo
            {
                fileName = currentPath + currentFileEntry.name,
                fileOffset = lv3Offset + lv3Header.fileDataOffset + currentFileEntry.fileOffset,
                fileSize = currentFileEntry.fileSize
            });

            //Move to next sibling
            if (currentFileEntry.nextSiblingFileOffset != -1)
            {
                br.BaseStream.Position = lv3Offset + lv3Header.fileMetaTableOffset + currentFileEntry.nextSiblingFileOffset;
                ResolveFiles(br, currentPath);
            }
        }
    }
}
