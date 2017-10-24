using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;
using System.IO;
using Kontract.IO;

namespace archive_obb
{
    public class OBBFileInfo : ArchiveFileInfo
    {
        public Entry entry;

        public uint Write(Stream input, uint offset)
        {
            entry.offset = offset;
            entry.size = (int)FileSize;

            input.Position = offset;
            FileData.CopyTo(input);

            return (uint)(offset + FileSize);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public int unk1;
        public int fileCount;
        public uint unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint unk1;
        public uint offset;
        public int size;
        public uint unk2;
    }

    public class ObbSupport
    {
        public static Dictionary<String, String> extensions = new Dictionary<string, string>()
        {
            ["OggS"] = ".ogg",
            ["ARCC"] = ".arc",
            ["GUI\0"] = ".gui",
            ["SBKR"] = ".sbkr",
            ["TEX "] = ".tex",
            ["FWSE"] = ".fwse",
            ["SDL\0"] = ".sdl",
            ["MOD\0"] = ".mod"
        };
    }
}
