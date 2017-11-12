using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;

namespace archive_nintendo.SB
{
    public class SBFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public ushort magic;
        public ushort entryCount;
    }

    public class Entry
    {
        public Entry(int offset, int size)
        {
            this.offset = offset;
            this.size = size;
        }
        public int offset;
        public int size;
    }
}
