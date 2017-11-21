using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;
using Komponent.IO;

namespace archive_xbb
{
    public class XBBFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XBBHeader
    {
        public Magic magic;
        public int entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XBBFileEntry
    {
        public uint offset;
        public uint size;
        public uint nameOffset;
        public uint hash;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XBBHashEntry
    {
        public uint hash;
        public uint id;
    }
}
