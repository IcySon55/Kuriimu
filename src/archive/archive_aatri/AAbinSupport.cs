using System.IO;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.Compression;
using Kuriimu.IO;

namespace archive_aatri.aabin
{
    public class AAbinFileInfo : ArchiveFileInfo
    {
        public Entry Entry;

        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;
                using (var br = new BinaryReaderX(baseStream, true))
                {
                    var pre = new MemoryStream(Nintendo.Decompress(new MemoryStream(br.ReadBytes((int)Entry.compSize))));
                    if (pre == null) return base.FileData;
                    return pre;
                }
            }
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset;
        public uint compSize;
    }
}
