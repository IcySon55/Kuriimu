using System.Runtime.InteropServices;

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
        public int Index;
        public string Text;
    }
}
