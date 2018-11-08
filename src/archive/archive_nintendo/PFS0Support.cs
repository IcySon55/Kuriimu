using Kontract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;

namespace archive_nintendo.PFS0
{
    public class PFS0FileInfo : ArchiveFileInfo
    {
        public FileEntry entry;
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
