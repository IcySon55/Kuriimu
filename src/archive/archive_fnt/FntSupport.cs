using System.Runtime.InteropServices;
using Kontract.Interface;
using System.IO;
using Komponent.IO;

namespace archive_fnt
{
    public class FntFileInfo : ArchiveFileInfo
    {
        
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public Entry(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                offset = br.ReadInt32();
                size = br.ReadInt32() - offset;
            }
        }

        public int offset;
        public int size;
    }
}
