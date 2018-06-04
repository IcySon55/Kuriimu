using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.Compression;
using System.IO;

using System.Diagnostics;

namespace archive_pac_mario_party
{
    [DebuggerDisplay("{FileName}")]
    public class PacFileInfo : ArchiveFileInfo
    {
        public Entry entry;
        public int connectedID = -1;

        public override Stream FileData
        {
            get => new MemoryStream(ZLib.Decompress(base.FileData));
        }

        public Stream GetBaseData()
        {
            return base.FileData;
        }

        public override long? FileSize => (State == ArchiveFileState.Archived) ? entry.decompSize : base.FileData.Length;

        public bool Equals(PacFileInfo file, int ID)
        {
            if (State != file.State)
            {
                if (State == ArchiveFileState.Archived)
                {
                    if (Support.CompareFileData(FileData, file.GetBaseData()))
                    {
                        connectedID = file.connectedID = ID;
                        return true;
                    }
                    else
                    {
                        connectedID = ID;
                        return false;
                    }
                }
                else
                {
                    if (Support.CompareFileData(base.FileData, file.FileData))
                    {
                        connectedID = file.connectedID = ID;
                        return true;
                    }
                    else
                    {
                        connectedID = ID;
                        return false;
                    }
                }
            }
            else
            {
                if (Support.CompareFileData(base.FileData, file.GetBaseData()))
                {
                    connectedID = file.connectedID = ID;
                    return true;
                }
                else
                {
                    connectedID = ID;
                    return false;
                }
            }
        }
    }

    public class Support
    {
        public static bool CompareFileData(Stream file1, Stream file2)
        {
            if (file1.Length != file2.Length)
                return false;

            var ms1 = new MemoryStream();
            file1.CopyTo(ms1);
            var ba1 = ms1.ToArray();

            var ms2 = new MemoryStream();
            file2.CopyTo(ms2);
            var ba2 = ms2.ToArray();

            for (var i = 0; i < ba1.Length; i++)
                if (ba1[i] != ba2[i])
                    return false;

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic = "PAC\0";
        public int unk1;
        public int unk2;
        public int dataOffset;

        public Magic magic2 = "\x1A$€";
        public int assetCount;
        public int entryCount;
        public int stringCount;
        public int fileDataCount;
        public long zero0;
        public long zero1;
        public int assetOffset = 0x80;
        public int entryOffset;
        public int stringOffset;
        public int fileDataOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AssetEntry
    {
        public int stringOffset;
        public uint unk1;
        public int count;
        public int entryOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public int stringOffset;
        public uint unk1;
        public int extOffset;
        public int const1;

        public int dataOffset;
        public int decompSize;
        public int compSize;
        public int compSize2;

        public long zero1;
        public int unk2;
        public int zero2;
    }
}
