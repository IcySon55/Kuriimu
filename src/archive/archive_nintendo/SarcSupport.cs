using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using Komponent.IO;
using Kontract.Interface;

namespace archive_nintendo.SARC
{
    public class SarcArchiveFileInfo : ArchiveFileInfo
    {
        public uint hash;
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
            ["default"] = 0x80,
            [".bflim"] = 0x80,
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

    public class Import
    {
        [Import("SimpleHash_3DS")]
        public IHash simplehash;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SARCHeader
    {
        Magic magic = "SARC";
        public short headerSize = 0x14;
        public ByteOrder byteOrder;
        public int fileSize;
        public int dataOffset;
        int unk1 = 0x100;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SFATHeader
    {
        Magic magic = "SFAT";
        public short headerSize = 0xc;
        public short nodeCount;
        public int hashMultiplier;  //default 0x65
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SFATEntry
    {
        public uint nameHash;
        public uint SFNTOffsetFlag;// 0x100 means it uses the SFNT table
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
