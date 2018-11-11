using Kontract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace archive_xc2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int filenameTableSizeUnpadded;
        public int nodeCount;
        public int filenameTableOffset;
        public int filenameTableSize;
        public int trieTableOffset;
        public int trieTableSize;
        public int fileEntryTableOffset;
        public int fileEntryCount;
        public int unk1;
    }

    public class StringEntry
    {
        public int offset;
        public string name;
        public int id;
    }

    public class NodeEntry
    {
        public int id1;
        public int id2;
        public bool usedNode;
    }

    public class FileEntry
    {
        public long fileOffset;
        public int fileSize;
        public int decompFileSize;
        public int flags;       //0x02 for compressed file
        public int fileId;
    }
}
