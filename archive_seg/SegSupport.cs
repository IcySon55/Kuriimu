using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace archive_seg
{
    // TODO: Define your format's logical blocks here to keep the main file clean and straight forward
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SegFileEntry
    {
        public uint Offset = 0;
        public uint Size = 0;
    }
}
