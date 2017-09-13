using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.IO;

namespace archive_obb
{
    public class OBBFileInfo : ArchiveFileInfo
    {
        public Entry entry;

        public uint Write(Stream input, uint offset)
        {
            entry.offset = offset;
            entry.size = (int)FileSize;

            input.Position = offset;
            FileData.CopyTo(input);

            return (uint)(offset + FileSize);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int unk1;
        public int fileCount;
        public uint unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint unk1;
        public uint offset;
        public int size;
        public uint unk2;
    }
}
