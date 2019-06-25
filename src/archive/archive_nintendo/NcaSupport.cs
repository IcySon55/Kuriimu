using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Kontract.Interface;
using Kontract.IO;
using Kontract;
using System.IO;
using System.Text;
using System;

namespace archive_nintendo.NCA
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Section
    {
        public byte unk1;
        public byte unk2;
        public byte partitionType;
        public byte fsType;
        public byte cryptType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3)]
        public byte[] padding1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x138)]
        public byte[] superBlock;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8)]
        public byte[] sectionCtr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xB8)]
        public byte[] padding2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RomFSSuperBlock
    {
        public Magic magic;
        public int magicNum;    //0x20000
        public int masterHashSize;
        public int levelCount;
        public long level1Offset;
        public long level1Size;
        public int level1BlockSize;        //log2
        public int reserved1;
        public long level2Offset;
        public long level2Size;
        public int level2BlockSize;        //log2
        public int reserved2;
        public long level3Offset;
        public long level3Size;
        public int level3BlockSize;        //log2
        public int reserved3;
        public long level4Offset;
        public long level4Size;
        public int level4BlockSize;        //log2
        public int reserved4;
        public long level5Offset;
        public long level5Size;
        public int level5BlockSize;        //log2
        public int reserved5;
        public long level6Offset;
        public long level6Size;
        public int level6BlockSize;        //log2
        public int reserved6;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] reserved7;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] masterHash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PFS0SuperBlock
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] hashTableHash;    //SHA256 over hashtable in PFS0
        public int blockSize;
        public int unk1;        //must be 0x2
        public long hashTableOffset;
        public long hashTableSize;
        public long pfs0Offset;
        public long pfs0Size;
    }

    public class NCAHeader
    {
        public Header meta;
        public List<SectionEntry> sectionEntries;
        public List<byte[]> sectionHeaderHashes = new List<byte[]>();
        public byte[] keyArea;

        public NCAHeader(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                meta = br.ReadStruct<Header>();
                sectionEntries = br.ReadMultiple<SectionEntry>(0x4);
                for (int i = 0; i < 4; i++)
                    sectionHeaderHashes.Add(br.ReadBytes(0x20));
                keyArea = br.ReadBytes(0x40);

                input.Position = 0x400;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
            public byte[] rsaUsingFixedKey;     //RSA-2048 over 0x200-0x400 with fixed key
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
            public byte[] rsaNPDMKey;           //RSA-2048 over 0x200-0x400 with NPDM key
            public Magic magic;
            public byte ncaType;
            public byte contentType;
            public byte cryptoType;
            public byte keyIndex;
            public long fileSize;
            public long titleID;
            public int zero1;
            public int sdk_version;
            public byte cryptoType2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xF)]
            public byte[] zero2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
            public byte[] rightsID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SectionEntry
        {
            public int mediaStartOffset;
            public int mediaEndOffset;
            public int unk1;
            public int unk2;
        }
    }

    public class RomFS
    {
        HashLevelHeader lv6header;
        long lv6Offset;
        public List<FinalFileInfo> files;

        public RomFS(Stream input, RomFSSuperBlock romFsBlock, long offset, string currentPath = "")
        {
            var bkOffset = input.Position;

            using (var br = new BinaryReaderX(input, true))
            {
                if (offset + romFsBlock.level6Offset > input.Length)
                    throw new InvalidOperationException("Invalid romfs data.");

                lv6Offset = br.BaseStream.Position = offset + romFsBlock.level6Offset;

                lv6header = br.ReadStruct<HashLevelHeader>();

                //Level6
                br.BaseStream.Position = lv6Offset + lv6header.dirMetaTableOffset;
                files = new List<FinalFileInfo>();
                ResolveDirectories(br, currentPath);
            }

            input.Position = bkOffset;
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
            currentDirEntry.name = Encoding.UTF8.GetString(br.ReadBytes(currentDirEntry.nameLength));

            //first go through all sub dirs
            if (currentDirEntry.firstChildDirOffset != -1)
            {
                br.BaseStream.Position = lv6Offset + lv6header.dirMetaTableOffset + currentDirEntry.firstChildDirOffset;
                ResolveDirectories(br, currentPath + currentDirEntry.name + "\\");
            }

            //then get all files of current dir
            if (currentDirEntry.firstFileOffset != -1)
            {
                br.BaseStream.Position = lv6Offset + lv6header.fileMetaTableOffset + currentDirEntry.firstFileOffset;
                ResolveFiles(br, currentPath + currentDirEntry.name + "\\");
            }

            //finally move to next sibling dir
            if (currentDirEntry.nextSiblingDirOffset != -1)
            {
                br.BaseStream.Position = lv6Offset + lv6header.dirMetaTableOffset + currentDirEntry.nextSiblingDirOffset;
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
            currentFileEntry.name = Encoding.UTF8.GetString(br.ReadBytes(currentFileEntry.nameLength));

            //Add current file
            files.Add(new FinalFileInfo
            {
                fileName = currentPath + currentFileEntry.name,
                fileOffset = lv6Offset + lv6header.fileDataOffset + currentFileEntry.fileOffset,
                fileSize = currentFileEntry.fileSize
            });

            //Move to next sibling
            if (currentFileEntry.nextSiblingFileOffset != -1)
            {
                br.BaseStream.Position = lv6Offset + lv6header.fileMetaTableOffset + currentFileEntry.nextSiblingFileOffset;
                ResolveFiles(br, currentPath);
            }
        }

        [DebuggerDisplay("{fileName}")]
        public class FinalFileInfo
        {
            public string fileName;
            public long fileOffset;
            public long fileSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class HashLevelHeader
        {
            public long headerLength;
            public long dirHashTableOffset;
            public long dirHashTableSize;
            public long dirMetaTableOffset;
            public long dirMetaTableSize;
            public long fileHashTableOffset;
            public long fileHashTableSize;
            public long fileMetaTableOffset;
            public long fileMetaTableSize;
            public long fileDataOffset;
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
    }

    public class PFS0
    {
        public List<FinalFileEntry> files = new List<FinalFileEntry>();

        public PFS0(Stream input, PFS0SuperBlock superBlock, long offset)
        {
            var bkOffset = input.Position;

            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = offset + superBlock.pfs0Offset;

                var header = br.ReadStruct<PFS0Header>();
                if (header.magic != "PFS0")
                    throw new InvalidOperationException("No valid PFS0");
                var fileEntries = br.ReadMultiple<FileEntry>(header.fileCount);

                var stringTable = br.ReadBytes(header.stringTableSize);
                using (var nameBr = new BinaryReaderX(new MemoryStream(stringTable)))
                {
                    foreach (var entry in fileEntries)
                    {
                        entry.offset += offset + superBlock.pfs0Offset + (0x10 + 0x18 * header.fileCount + header.stringTableSize);

                        nameBr.BaseStream.Position = entry.stringOffset;
                        files.Add(new FinalFileEntry
                        {
                            fileEntry = entry,
                            name = nameBr.ReadCStringA()
                        });
                    }
                }
            }

            input.Position = bkOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class PFS0Header
        {
            public Magic magic;
            public int fileCount;
            public int stringTableSize;
            public int reserved1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class FileEntry
        {
            public long offset;
            public long size;
            public int stringOffset;
            public int reserved1;
        }

        public class FinalFileEntry
        {
            public FileEntry fileEntry;
            public string name;
        }
    }

    //public static class RomFSBuilder
    //{
    //    const int UnusedEntry = -1;

    //    public static int SuperBlockSize = 0;
    //    public static byte[] SuperBlockHash = new byte[0x20];

    //    class MetaData
    //    {
    //        public int DirMetaOffset = 0;
    //        public int FileMetaOffset = 0;
    //        public int FileOffset = 0;

    //        public List<DirEntry> Dirs = new List<DirEntry>();
    //        public uint[] DirHashTable;
    //        public List<FileEntry> Files = new List<FileEntry>();
    //        public uint[] FileHashTable;

    //        [DebuggerDisplay("{name}")]
    //        public class DirEntry
    //        {
    //            public const int Size = 6 * 0x4;

    //            public int MetaOffset;
    //            public uint Hash;

    //            public int ParentOffset;
    //            public int NextSiblingOffset;
    //            public int FirstChildOffset;
    //            public int FirstFileOffset;

    //            public int? NextDirInSameBucket = null;

    //            public string name;
    //        }
    //        [DebuggerDisplay("{name}")]
    //        public class FileEntry
    //        {
    //            public const int Size = 4 * 0x4 + 2 * 0x8;

    //            public Stream FileData;

    //            public int MetaOffset;
    //            public uint Hash;

    //            public int ParentDirOffset;
    //            public int NextSiblingOffset;
    //            public long DataOffset;
    //            public long DataSize;

    //            public int? NextFileInSameBucket = null;

    //            public string name;
    //        }
    //    }

    //    [DebuggerDisplay("{dirName}")]
    //    public class IntDirectory
    //    {
    //        public string dirName;
    //        public string fullDirName;
    //        List<IntDirectory> dirs = new List<IntDirectory>();
    //        List<ArchiveFileInfo> files = new List<ArchiveFileInfo>();

    //        public void AddFile(ArchiveFileInfo file)
    //        {
    //            files.Add(file);
    //        }
    //        public void AddFiles(List<ArchiveFileInfo> file)
    //        {
    //            files.AddRange(file);
    //        }

    //        public void AddDirectory(IntDirectory dir)
    //        {
    //            dirs.Add(dir);
    //        }

    //        public ArchiveFileInfo[] GetFiles()
    //        {
    //            return files.ToArray();
    //        }

    //        public IntDirectory[] GetDirectories()
    //        {
    //            return dirs.ToArray();
    //        }
    //    }
    //    static IntDirectory ParseFileTree(List<ArchiveFileInfo> files, string inPath = "")
    //    {
    //        var dir = new IntDirectory();
    //        var sp = inPath.Split('\\');
    //        dir.dirName = (sp.Count() == 1) ? sp[0] : sp[sp.Count() - 1];
    //        dir.fullDirName = inPath;

    //        var pf = files.Where(f => f.FileName.Replace(inPath + "\\", "").Count(c => c == '\\') <= 0).ToList();
    //        var nf = files.Except(pf).ToList();

    //        dir.AddFiles(pf);

    //        var distinctDirNames = nf.Select(f => f.FileName.Replace(inPath + "\\", "").Split('\\')[0]).Distinct().ToList();
    //        foreach (var newInPathPart in distinctDirNames)
    //        {
    //            var nl = nf.Where(s => s.FileName.StartsWith(Path.Combine(inPath, newInPathPart))).ToList();
    //            dir.AddDirectory(ParseFileTree(nl, Path.Combine(inPath, newInPathPart)));
    //        }

    //        return dir;
    //    }

    //    public static long Rebuild(Stream input, long offset, List<ArchiveFileInfo> files, string root)
    //    {
    //        root = (root.Last() == '\\') ? root.Remove(root.Length - 1, 1) : root;
    //        var ROOT = ParseFileTree(files, root);

    //        //Create MetaData Tree
    //        var metaData = new MetaData();
    //        var rootDir = new MetaData.DirEntry
    //        {
    //            MetaOffset = 0,
    //            ParentOffset = 0,
    //            NextSiblingOffset = UnusedEntry,
    //            FirstChildOffset = 0x18,
    //            FirstFileOffset = 0,
    //            NextDirInSameBucket = UnusedEntry,
    //            name = ""
    //        };
    //        metaData.DirMetaOffset = 0x18;
    //        metaData.Dirs.Add(rootDir);

    //        PopulateMetaData(metaData, ROOT, rootDir);

    //        //Creating hashbuckets
    //        metaData.DirHashTable = new uint[GetHashTableEntryCount((uint)metaData.Dirs.Count)];
    //        for (int i = 0; i < metaData.DirHashTable.Length; i++)
    //            metaData.DirHashTable[i] = 0xFFFFFFFF;
    //        PopulateDirHashTable(metaData.Dirs, metaData.DirHashTable);

    //        metaData.FileHashTable = new uint[GetHashTableEntryCount((uint)metaData.Files.Count)];
    //        for (int i = 0; i < metaData.FileHashTable.Length; i++)
    //            metaData.FileHashTable[i] = 0xFFFFFFFF;
    //        PopulateFileHashTable(metaData.Files, metaData.FileHashTable);

    //        //Write MetaData
    //        var romMeta = File.Create("romMeta_tmp.bin");
    //        var rawRomFSSize = WriteMetaData(romMeta, metaData, 0x1000);

    //        //Creating IVFC Hash Levels
    //        var ivfcLvls = CreateIVFCLevels(romMeta, 3, 0x1000);

    //        //Write all levels and IVFC Header
    //        var start = input.Position;
    //        WriteRomFSData(input, romMeta, rawRomFSSize, ivfcLvls, 0x1000);
    //        var fullFomFSSize = input.Position - start;

    //        romMeta.Close();
    //        if (File.Exists("romMeta_tmp.bin")) File.Delete("romMeta_tmp.bin");

    //        return fullFomFSSize;
    //    }

    //    static void WriteRomFSData(Stream input, Stream romMeta, long rawRomFSSize, List<(Stream, int)> ivfcLvls, int blockSize)
    //    {
    //        int Align(int pos, int align) => pos += align - (pos % align);

    //        //Create Header
    //        var header = new RomFS.Header
    //        {
    //            masterHashSize = ivfcLvls[2].Item2,
    //            lv1LogicalOffset = 0,
    //            lv1HashDataSize = ivfcLvls[1].Item2,
    //            lv2LogicalOffset = ivfcLvls[1].Item1.Length,
    //            lv2HashDataSize = ivfcLvls[0].Item2,
    //            lv3LogicalOffset = ivfcLvls[1].Item1.Length + ivfcLvls[0].Item1.Length,
    //            lv3HashDataSize = rawRomFSSize
    //        };

    //        //Write data
    //        using (var bw = new BinaryWriterX(input, true))
    //        {
    //            var startOffset = input.Position;

    //            //Header
    //            bw.WriteStruct(header);
    //            bw.WriteAlignment(0x10);

    //            //MasterHash
    //            ivfcLvls[2].Item1.CopyTo(bw.BaseStream);
    //            bw.WriteAlignment(blockSize);

    //            //SuperBlock
    //            SuperBlockSize = Align(0x60 + ivfcLvls[2].Item2, 0x200);
    //            SuperBlockHash = Kontract.Hash.SHA256.Create(input, startOffset, SuperBlockSize);

    //            //Level3 Data Layer
    //            romMeta.CopyTo(bw.BaseStream);
    //            bw.WriteAlignment(blockSize);

    //            //Level2 Hash Layer
    //            ivfcLvls[1].Item1.CopyTo(bw.BaseStream);
    //            bw.WriteAlignment(blockSize);

    //            //Level1 Hash Layer
    //            ivfcLvls[0].Item1.CopyTo(bw.BaseStream);
    //            bw.WriteAlignment(blockSize);
    //        }
    //    }

    //    static List<(Stream, int)> CreateIVFCLevels(Stream romFS, int levels, int blockSize = 0x1000)
    //    {
    //        ulong Align(ulong pos, ulong align) => pos += align - (pos % align);

    //        List<(Stream, int)> result = new List<(Stream, int)>();

    //        for (int i = 0; i < levels; i++)
    //        {
    //            result.Add((new MemoryStream(), 0));

    //            if (i == 0)
    //            {
    //                //Hash Top level RomFS
    //                while (romFS.Position < romFS.Length)
    //                {
    //                    var hash = Kontract.Hash.SHA256.Create(romFS, romFS.Position, blockSize);
    //                    romFS.Position += blockSize;
    //                    result.Last().Item1.Write(hash, 0, hash.Length);
    //                }
    //                romFS.Position = 0;
    //            }
    //            else
    //            {
    //                //Hash last hash Level
    //                var latestHashLvl = result[result.Count - 2].Item1;
    //                while (latestHashLvl.Position < latestHashLvl.Length)
    //                {
    //                    var hash = Kontract.Hash.SHA256.Create(latestHashLvl, latestHashLvl.Position, blockSize);
    //                    latestHashLvl.Position += blockSize;
    //                    result.Last().Item1.Write(hash, 0, hash.Length);
    //                }
    //                latestHashLvl.Position = 0;
    //            }

    //            result[result.Count - 1] = (result.Last().Item1, (int)result.Last().Item1.Length);

    //            if (i != levels - 1)
    //            {
    //                long len = result.Last().Item1.Position;
    //                if (len % blockSize != 0)
    //                {
    //                    len = (long)Align((ulong)len, (ulong)blockSize);
    //                    byte[] buf = new byte[len - result.Last().Item1.Position];
    //                    result.Last().Item1.Write(buf, 0, buf.Length);
    //                }
    //            }
    //            result.Last().Item1.Position = 0;
    //        }

    //        return result;
    //    }
    //    static long WriteMetaData(Stream romFS, MetaData metaData, int blockSize)
    //    {
    //        int Align(int input, int align) => input = input + (align - 1) & ~(align - 1);

    //        using (var bw = new BinaryWriterX(romFS, true))
    //        {
    //            var header = new RomFS.HashLevelHeader
    //            {
    //                headerLength = 0x28,
    //                dirHashTableOffset = 0x28,
    //                dirHashTableSize = metaData.DirHashTable.Length * sizeof(uint),
    //                dirMetaTableSize = metaData.Dirs.Sum(m => Align(MetaData.DirEntry.Size + Encoding.Unicode.GetByteCount(m.name), 4)),
    //                fileHashTableSize = metaData.FileHashTable.Length * sizeof(uint),
    //                fileMetaTableSize = metaData.Files.Sum(m => Align(MetaData.FileEntry.Size + Encoding.Unicode.GetByteCount(m.name), 4)),
    //            };
    //            bw.BaseStream.Position = 0x28;

    //            //DirHashTable
    //            foreach (var hash in metaData.DirHashTable)
    //                bw.Write(hash);
    //            header.dirMetaTableOffset = (int)bw.BaseStream.Position;

    //            //DirMetaTable
    //            foreach (var dir in metaData.Dirs)
    //            {
    //                bw.Write(dir.ParentOffset);
    //                bw.Write(dir.NextSiblingOffset);
    //                bw.Write(dir.FirstChildOffset);
    //                bw.Write(dir.FirstFileOffset);
    //                bw.Write(dir.NextDirInSameBucket ?? UnusedEntry);
    //                bw.Write(Encoding.Unicode.GetByteCount(dir.name));
    //                bw.Write(Encoding.Unicode.GetBytes(dir.name));
    //                bw.WriteAlignment(4);
    //            }
    //            header.fileHashTableOffset = (int)bw.BaseStream.Position;

    //            //FileHashTable
    //            foreach (var hash in metaData.FileHashTable)
    //                bw.Write(hash);
    //            header.fileMetaTableOffset = (int)bw.BaseStream.Position;

    //            //FileMetaTable
    //            foreach (var file in metaData.Files)
    //            {
    //                bw.Write(file.ParentDirOffset);
    //                bw.Write(file.NextSiblingOffset);
    //                bw.Write(file.DataOffset);
    //                bw.Write(file.DataSize);
    //                bw.Write(file.NextFileInSameBucket ?? UnusedEntry);
    //                bw.Write(Encoding.Unicode.GetByteCount(file.name));
    //                bw.Write(Encoding.Unicode.GetBytes(file.name));
    //                bw.WriteAlignment(4);
    //            }
    //            header.fileDataOffset = (int)bw.BaseStream.Position;

    //            //Files
    //            foreach (var file in metaData.Files)
    //                file.FileData.CopyTo(bw.BaseStream);

    //            var rawSize = bw.BaseStream.Position;

    //            bw.WriteAlignment(blockSize);

    //            bw.BaseStream.Position = 0;
    //            bw.WriteStruct(header);
    //            bw.BaseStream.Position = 0;

    //            return rawSize;
    //        }
    //    }

    //    static void PopulateDirHashTable(List<MetaData.DirEntry> entries, uint[] buckets)
    //    {
    //        for (int i = 0; i < entries.Count; i++)
    //        {
    //            if (entries[i].NextDirInSameBucket == null)
    //            {
    //                //get all entries with same bucket
    //                var buckID = GetBucketID(entries[i].Hash, buckets.Length);
    //                var siblings = entries.Where(e => GetBucketID(e.Hash, buckets.Length) == buckID);

    //                //Set head entry offset (the latest entry in the list)
    //                buckets[buckID] = (uint)siblings.Last().MetaOffset;

    //                //set NextDirInSameBucket in each of those entries
    //                var ind = 0;
    //                foreach (var sib in siblings)
    //                {
    //                    if (ind == 0)
    //                        sib.NextDirInSameBucket = UnusedEntry;
    //                    else
    //                        sib.NextDirInSameBucket = siblings.ElementAt(ind - 1).MetaOffset;
    //                    ind++;
    //                }
    //            }
    //        }
    //    }
    //    static void PopulateFileHashTable(List<MetaData.FileEntry> entries, uint[] buckets)
    //    {
    //        for (int i = 0; i < entries.Count; i++)
    //        {
    //            if (entries[i].NextFileInSameBucket == null)
    //            {
    //                //get all entries with same bucket
    //                var buckID = GetBucketID(entries[i].Hash, buckets.Length);
    //                var siblings = entries.Where(e => GetBucketID(e.Hash, buckets.Length) == buckID);

    //                //Set head entry offset (the latest entry in the list)
    //                buckets[buckID] = (uint)siblings.Last().MetaOffset;

    //                //set NextDirInSameBucket in each of those entries
    //                var ind = 0;
    //                foreach (var sib in siblings)
    //                {
    //                    if (ind == 0)
    //                        sib.NextFileInSameBucket = UnusedEntry;
    //                    else
    //                        sib.NextFileInSameBucket = siblings.ElementAt(ind - 1).MetaOffset;
    //                    ind++;
    //                }
    //            }
    //        }
    //    }

    //    static void PopulateMetaData(MetaData metaData, IntDirectory dir, MetaData.DirEntry parentDir)
    //    {
    //        //Adding files
    //        var files = dir.GetFiles();
    //        for (int i = 0; i < files.Length; i++)
    //        {
    //            var newFileEntry = new MetaData.FileEntry
    //            {
    //                //Parent = parentDir,

    //                MetaOffset = metaData.FileMetaOffset,
    //                Hash = CalcPathHash((uint)parentDir.MetaOffset, Encoding.Unicode.GetBytes(Path.GetFileName(files[i].FileName))),

    //                FileData = files[i].FileData,

    //                ParentDirOffset = parentDir.MetaOffset,
    //                DataOffset = metaData.FileOffset,
    //                DataSize = (int)files[i].FileSize,
    //                //NextFileInSameBucket =,
    //                name = Path.GetFileName(files[i].FileName)
    //            };
    //            metaData.FileOffset += (int)files[i].FileSize;

    //            metaData.FileMetaOffset += MetaData.FileEntry.Size + Path.GetFileName(files[i].FileName).Length * 2;
    //            if (metaData.FileMetaOffset % 4 != 0)
    //                metaData.FileMetaOffset += 2;

    //            newFileEntry.NextSiblingOffset = (i + 1 == files.Length) ? UnusedEntry : metaData.FileMetaOffset;

    //            metaData.Files.Add(newFileEntry);
    //        }

    //        //Adding subdirectories
    //        var dirs = dir.GetDirectories();
    //        List<int> metaDirIndeces = new List<int>();
    //        for (int i = 0; i < dirs.Length; i++)
    //        {
    //            var newDirEntry = new MetaData.DirEntry
    //            {
    //                //Parent = parentDir,

    //                MetaOffset = metaData.DirMetaOffset,
    //                Hash = CalcPathHash((uint)parentDir.MetaOffset, Encoding.Unicode.GetBytes(dirs[i].dirName)),

    //                ParentOffset = parentDir.MetaOffset,
    //                //FirstChildOffset
    //                //FirstFileOffset =,
    //                //NextDirInSameBucket =,

    //                name = dirs[i].dirName
    //            };

    //            metaData.DirMetaOffset += MetaData.DirEntry.Size + dirs[i].dirName.Length * 2;
    //            if (metaData.DirMetaOffset % 4 != 0)
    //                metaData.DirMetaOffset += 2;

    //            newDirEntry.NextSiblingOffset = (i + 1 < dirs.Length) ? metaData.DirMetaOffset : UnusedEntry;

    //            metaData.Dirs.Add(newDirEntry);
    //            metaDirIndeces.Add(metaData.Dirs.Count - 1);
    //        }

    //        //Adding childs of subdirectories
    //        for (int i = 0; i < dirs.Length; i++)
    //        {
    //            metaData.Dirs[metaDirIndeces[i]].FirstChildOffset = dirs[i].GetDirectories().Length > 0 ? metaData.DirMetaOffset : UnusedEntry;
    //            metaData.Dirs[metaDirIndeces[i]].FirstFileOffset = (dirs[i].GetFiles().Length > 0) ? metaData.FileMetaOffset : UnusedEntry;
    //            PopulateMetaData(metaData, dirs[i], metaData.Dirs[metaDirIndeces[i]]);
    //        }
    //    }

    //    static uint CalcPathHash(uint ParentOffset, byte[] NameArray, int start = 0, int len = -1)
    //    {
    //        uint hash = ParentOffset ^ 123456789;
    //        for (int i = 0; i < NameArray.Length; i += 2)
    //        {
    //            hash = (hash >> 5) | (hash << 27);
    //            hash ^= (ushort)(NameArray[start + i] | (NameArray[start + i + 1] << 8));
    //        }
    //        return hash;
    //    }
    //    static uint GetHashTableEntryCount(uint Entries)
    //    {
    //        uint count = Entries;
    //        if (Entries < 3)
    //            count = 3;
    //        else if (count < 19)
    //            count |= 1;
    //        else
    //        {
    //            while (count % 2 == 0 || count % 3 == 0 || count % 5 == 0 || count % 7 == 0 || count % 11 == 0 || count % 13 == 0 || count % 17 == 0)
    //            {
    //                count++;
    //            }
    //        }
    //        return count;
    //    }
    //    static long GetBucketID(uint Hash, int EntryCount) => Hash % EntryCount;
    //}
}
