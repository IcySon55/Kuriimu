using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_arcv
{
    public class ARCVFileInfo : ArchiveFileInfo
    {
        public uint hash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ARCVHeader
    {
        public Magic magic;
        public int fileCount;
        public uint fileSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ARCVFileEntry
    {
        public uint offset;
        public int size;
        public uint hash;
    }
}
