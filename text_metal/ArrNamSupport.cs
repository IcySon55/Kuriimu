using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace text_metal
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class ArrEntry
    {
        public short Offset;
        public short Unk1;
        public int Unk2;
        public int Unk3;
    }

    public sealed class Entry
    {
        public ArrEntry ArrEntry;
        public string Text;
        public long Offset;
        public int Length;
    }
}
