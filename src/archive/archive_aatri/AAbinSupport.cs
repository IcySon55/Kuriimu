using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Komponent.IO;

namespace archive_aatri.aabin
{
    public class AAbinFileInfo : ArchiveFileInfo
    {
        public Entry Entry;
        public Import imports;

        public override Stream FileData
        {
            get
            {
                var baseStream = base.FileData;
                using (var br = new BinaryReaderX(baseStream, true))
                {
                    var pre = new MemoryStream(imports.nintendo.Decompress(new MemoryStream(br.ReadBytes((int)Entry.compSize)), 0));
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

    public class Import
    {
        [Import("Nintendo")]
        public ICompressionCollection nintendo;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}
