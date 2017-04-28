using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;
using System.IO;
using Kuriimu.IO;

namespace archive_afs
{
    public class AFSFileInfo : ArchiveFileInfo
    {

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int fileCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public uint offset;
        public uint size;
    }

    public class NameEntry
    {
        public NameEntry(Stream input)
        {
            using (var br=new BinaryReaderX(input,true))
            {
                name = br.ReadString(0x20);
                unk1 = br.ReadBytes(0xc);
                offset = br.ReadUInt32();
            }
        }

        public string name;
        public byte[] unk1=new byte[0xc];
        public uint offset;
    }
}
