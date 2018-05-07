using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Kontract;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.SARC
{
    public class SarcArchiveFileInfo : ArchiveFileInfo
    {
        public SFATEntry Entry;
        public uint Hash;
    }

    public class Support
    {
        public static int Pad(int pos, string filename, System sys)
        {
            var dicPadding = (sys == System.CTR) ? DicPaddingCTR : DicPaddingWiiU;
            if (!dicPadding.TryGetValue(Path.GetExtension(filename), out int padding))
            {
                padding = dicPadding["default"];
            }
            return (pos + padding - 1) & -padding;
        }

        static Dictionary<string, int> DicPaddingCTR = new Dictionary<string, int>
        {
            ["default"] = 0x80,
            [".msbt"] = 0x4,
            [".msbf"] = 0x4,
            [".icn"] = 0x4,
            [".byaml"] = 0x4,
            [".bffnt"] = 0x2000,
            [".bcfnt"] = 0x2000,
        };

        static Dictionary<string, int> DicPaddingWiiU = new Dictionary<string, int>
        {
            ["default"] = 0x4,
            [".bflim"] = 0x100,
            [".ptcl"] = 0x80,
            [".shbin"] = 0x80,
            [".bsm"] = 0x80,
            [".bffnt"] = 0x80,
        };

        public static Dictionary<string, string> extensions = new Dictionary<string, string>
        {
            ["CTPK"] = ".ctpk",
            ["MsgF"] = ".msbf",
            ["MsgS"] = ".msbt",
            ["BCH\0"] = ".bch",
            ["SPBD"] = ".ptcl",
            ["DVLB"] = ".shbin",
            ["CLIM"] = ".bclim",
            ["FLIM"] = ".bflim",
            ["SMDH"] = ".icn",
            ["YB\x1\0"] = ".byaml"
        };
    }

    public enum System : byte
    {
        CTR,
        WiiU
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SARCHeader
    {
        Magic magic = "SARC";
        public short headerSize = 0x14;
        public ByteOrder byteOrder;
        public int fileSize;
        public int dataOffset;
        int unk1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SFATHeader
    {
        Magic magic = "SFAT";
        public short headerSize = 0xC;
        public short nodeCount;
        public int hashMultiplier; //default 0x65
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SFATEntry
    {
        public uint nameHash;
        public uint SFNTOffsetFlag; // 0x100 means it uses the SFNT table
        public int dataStart;
        public int dataEnd;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SFNTHeader
    {
        Magic magic = "SFNT";
        public int headerSize = 0x8;
    }
}
