using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;

namespace image_bnr
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public short version;
        public ushort crc16;
    }

    public class Import
    {
        [Import("CRC16")]
        public IHash crc16;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
}
