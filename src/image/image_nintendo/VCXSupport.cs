using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract;
using System.Diagnostics;

namespace image_nintendo.VCX
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VCLHeader
    {
        public Magic magic;
        public short colorCount;
        public short transparentId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VCGHeader
    {
        public Magic magic;
        public long unk1;
        public int dataSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VCEHeader
    {
        public Magic magic;
        public int count0;
        public int offset0;
        public int count1;
        public int offset1;
        public int count2;
        public int offset2;
        public int count3;
        public int offset3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VCEEntry0
    {
        public short t1Index;
        public short count;
        public int unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{unk1}")]
    public class VCEEntry1
    {
        public short t2Index;
        public short unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{unk1}")]
    public class VCEEntry2
    {
        public short t3Index;
        public short unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VCEEntry3
    {
        public short unk1;
        public short unk2;
        public short unk3;
        public short unk4;
    }

    [DebuggerDisplay("{unk1}")]
    public class MergedEntry
    {
        public int unk1;
        public List<MergeT1> t1Entries;

        [DebuggerDisplay("{unk1}")]
        public class MergeT1
        {
            public short unk1;
            public MergeT2 t2Entry;

            [DebuggerDisplay("{unk1}")]
            public class MergeT2
            {
                public short unk1;
                public VCEEntry3 t3Entry;
            }
        }
    }
}
