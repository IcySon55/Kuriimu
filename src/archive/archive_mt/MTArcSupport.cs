using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Kontract.Compression;
using Kontract.IO;
using Kontract.Interface;
using Kontract;

namespace archive_mt
{
    public class MTArcFileInfo : ArchiveFileInfo
    {
        public FileMetadata Metadata { get; set; }
        public FileMetadataSwitch SwitchMetadata { get; set; }
        public CompressionLevel CompressionLevel { get; set; }
        public Platform System { get; set; }

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived || CompressionLevel == CompressionLevel.NoCompression) return base.FileData;
                return new MemoryStream(ZLib.Decompress(base.FileData));
            }
        }

        public override long? FileSize => System == Platform.CTR ? Metadata.UncompressedSize & 0x00FFFFFF : System == Platform.Switch ? SwitchMetadata.UncompressedSize : Metadata.UncompressedSize >> 3;

        public void Write(Stream output, long offset, ByteOrder byteOrder)
        {
            using (var bw = new BinaryWriterX(output, true, byteOrder))
            {
                if (System == Platform.Switch)
                    SwitchMetadata.Offset = (int)offset;
                else
                    Metadata.Offset = (int)offset;

                if (State == ArchiveFileState.Archived)
                    base.FileData.CopyTo(bw.BaseStream);
                else
                {
                    if (CompressionLevel != CompressionLevel.NoCompression)
                    {
                        var bytes = ZLib.Compress(FileData, CompressionLevel, true);
                        bw.Write(bytes);
                        if (System == Platform.Switch)
                            SwitchMetadata.CompressedSize = bytes.Length;
                        else
                            Metadata.CompressedSize = bytes.Length;
                    }
                    else
                    {
                        FileData.CopyTo(bw.BaseStream);
                        if (System == Platform.Switch)
                            SwitchMetadata.CompressedSize = (int)FileData.Length;
                        else
                            Metadata.CompressedSize = (int)FileData.Length;
                    }

                    Metadata.UncompressedSize = System == Platform.CTR ? (int)(FileData.Length & 0x00FFFFFF) : System == Platform.Switch ? (int)FileData.Length : (int)(FileData.Length << 3);
                }
            }
        }
    }

    public enum Platform
    {
        CTR,
        PS3,
        Switch
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HFSHeader
    {
        public Magic Magic;
        public short Version;
        public short Type;
        public int FileSize;
        public int Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HFSFooter
    {
        public ulong Hash1;
        public ulong Hash2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic Magic;
        public short Version;
        public short EntryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileMetadata
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string FileName;
        public uint ExtensionHash;
        public int CompressedSize;
        public int UncompressedSize;
        public int Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileMetadataSwitch
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string FileName;
        public uint ExtensionHash;
        public int CompressedSize;
        public int UncompressedSize;
        public int Unknown1;
        public int Offset;
    }
}

public static class ArcShared
{
    public static uint GetHash(string s) => new BitArray(s.Select(x => (byte)x).ToArray()).Cast<bool>().Aggregate(~0u, (h, i) => h / 2 ^ (i ^ h % 2 != 0 ? 0xEDB88320 : 0)) * 2 / 2;

    public static Dictionary<uint, string> ExtensionMap = new Dictionary<uint, string>
    {
        [GetHash("rAIFSM")] = ".xfsa",
        [GetHash("rCameraList")] = ".lcm",
        [GetHash("rCharacter")] = ".xfsc",
        [GetHash("rCollision")] = ".sbc",
        [GetHash("rEffectAnim")] = ".ean",
        [GetHash("rEffectList")] = ".efl",
        [GetHash("rGUI")] = ".gui",
        [GetHash("rGUIFont")] = ".gfd",
        [GetHash("rGUIIconInfo")] = ".gii",
        [GetHash("rGUIMessage")] = ".gmd",
        [GetHash("rHit2D")] = ".xfsh",
        [GetHash("rLayoutParameter")] = ".xfsl",
        [GetHash("rMaterial")] = ".mrl",
        [GetHash("rModel")] = ".mod",
        [GetHash("rMotionList")] = ".lmt",
        [GetHash("rPropParam")] = ".prp",
        [GetHash("rScheduler")] = ".sdl",
        [GetHash("rSoundBank")] = ".sbkr",
        [GetHash("rSoundRequest")] = ".srqr",
        [GetHash("rSoundSourceADPCM")] = ".mca",
        [GetHash("rTexture")] = ".tex",

        [0x22FA09] = ".hpe",
        [0x26E7FF] = ".ccl",
        [0x86B80F] = ".plexp",
        [0xFDA99B] = ".ntr",
        [0x2358E1A] = ".spkg",
        [0x2373BA7] = ".spn",
        [0x2833703] = ".efs",
        [0x315E81F] = ".sds",
        [0x437BCF2] = ".grw",
        [0x4B4BE62] = ".tmd",
        [0x525AEE2] = ".wfp",
        [0x5A36D08] = ".qif",
        [0x69A1911] = ".olp",
        [0x737E28B] = ".rst",
        [0x7437CCE] = ".base",
        [0x79B5F3E] = ".pci",
        [0x7B8BCDE] = ".fca",
        [0x7F768AF] = ".gii",
        [0x89BEF2C] = ".sap",
        [0xA74682F] = ".rnp",
        [0xC4FCAE4] = ".PlDefendParam",
        [0xD06BE6B] = ".tmn",
        [0xECD7DF4] = ".scs",
        [0x11C35522] = ".gr2",
        [0x12191BA1] = ".epv",
        [0x12688D38] = ".pjp",
        [0x12C3BFA7] = ".cpl",
        [0x133917BA] = ".mss",
        [0x14428EAE] = ".gce",
        [0x15302EF4] = ".lot",
        [0x157388D3] = ".itl",
        [0x15773620] = ".nmr",
        [0x167DBBFF] = ".stq",
        [0x1823137D] = ".mlm",
        [0x19054795] = ".nnl",
        [0x199C56C0] = ".ocl",
        [0x1B520B68] = ".zon",
        [0x1BCC4966] = ".srq",
        [0x1C2B501F] = ".atr",
        [0x1EB3767C] = ".spr",
        [0x2052D67E] = ".sn2",
        [0x215896C2] = ".statusparam",
        [0x2282360D] = ".jex",
        [0x22948394] = ".gui",
        [0x22B2A2A2] = ".PlNeckPos",
        [0x232E228C] = ".rev",
        [0x241F5DEB] = ".tex",
        [0x242BB29A] = ".gmd",
        [0x257D2F7C] = ".swm",
        [0x2749C8A8] = ".mrl",
        [0x271D08FE] = ".ssq",
        [0x272B80EA] = ".prp",
        [0x276DE8B7] = ".e2d",
        [0x2A37242D] = ".gpl",
        [0x2A4F96A8] = ".rbd",
        [0x2B0670A5] = ".map",
        [0x2B303957] = ".gop",
        [0x2B40AE8F] = ".equ",
        [0x2CE309AB] = ".joblvl",
        [0x2D12E086] = ".srd",
        [0x2D462600] = ".gfd",
        [0x30FC745F] = ".smx",
        [0x312607A4] = ".bll",
        [0x31B81AA5] = ".qr",
        [0x325AACA5] = ".shl",
        [0x32E2B13B] = ".edp",
        [0x33B21191] = ".esp",
        [0x354284E7] = ".lvl",
        [0x358012E8] = ".vib",
        [0x36019854] = ".bed",
        [0x39A0D1D6] = ".sms",
        [0x39C52040] = ".lcm",
        [0x3A947AC1] = ".cql",
        [0x3B350990] = ".qsp",
        [0x3BBA4E33] = ".qct",
        [0x3D97AD80] = ".amr",
        [0x3E356F93] = ".stc",
        [0x3E363245] = ".chn",
        [0x3FB52996] = ".imx",
        [0x4046F1E1] = ".ajp",
        [0x437662FC] = ".oml",
        [0x4509FA80] = ".itemlv",
        [0x456B6180] = ".cnsshake",
        [0x472022DF] = ".AIPlActParam",
        [0x48538FFD] = ".ist",
        [0x48C0AF2D] = ".msl",
        [0x49B5A885] = ".ssc",
        [0x4B704CC0] = ".mia",
        [0x4C0DB839] = ".sdl",
        [0x4CA26828] = ".bmse",
        [0x4E397417] = ".ean",
        [0x4E44FB6D] = ".fpe",
        [0x4EF19843] = ".nav",
        [0x4FB35A95] = ".aor",
        [0x50F3D713] = ".skl",
        [0x5175C242] = ".geo2",
        [0x51FC779F] = ".sbc",
        [0x522F7A3D] = ".fcp",
        [0x52DBDCD6] = ".rdd",
        [0x535D969F] = ".ctc",
        [0x5802B3FF] = ".ahc",
        [0x58A15856] = ".mod",
        [0x59D80140] = ".ablparam",
        [0x5A61A7C8] = ".fed",
        [0x5A7FEA62] = ".ik",
        [0x5B334013] = ".bap",
        [0x5EA7A3E9] = ".sky",
        [0x5F36B659] = ".way",
        [0x5F88B715] = ".epd",
        [0x60BB6A09] = ".hed",
        [0x6186627D] = ".wep",
        [0x619D23DF] = ".shp",
        [0x628DFB41] = ".gr2s",
        [0x63747AA7] = ".rpi",
        [0x63B524A7] = ".ltg",
        [0x64387FF1] = ".qlv",
        [0x65B275E5] = ".sce",
        [0x66B45610] = ".fsm",
        [0x671F21DA] = ".stp",
        [0x69A5C538] = ".dwm",
        [0x6D0115ED] = ".prt",
        [0x6D5AE854] = ".efl",
        [0x6DB9FA5F] = ".cmc",
        [0x6EE70EFF] = ".pcf",
        [0x6F302481] = ".plw",
        [0x6FE1EA15] = ".spl",
        [0x72821C38] = ".stm",
        [0x73850D05] = ".arc",
        [0x754B82B4] = ".ahs",
        [0x76820D81] = ".lmt",
        [0x76DE35F6] = ".rpn",
        [0x7808EA10] = ".rtex",
        [0x7817FFA5] = ".fbik_human",
        [0x7AA81CAB] = ".eap",
        [0x7BEC319A] = ".sps",
        [0x7DA64808] = ".qmk",
        [0x7E1C8D43] = ".pcs",
        [0x7E33A16C] = ".spc",
        [0x7E4152FF] = ".stg",
        [0x17A550D] = ".lom",
        [0x253F147] = ".hit",
        [0x39D71F2] = ".rvt",
        [0xDADAB62] = ".oba",
        [0x10C460E6] = ".msg",
        [0x176C3F95] = ".los",
        [0x19A59A91] = ".lnk",
        [0x1BA81D3C] = ".nck",
        [0x1ED12F1B] = ".glp",
        [0x1EFB1B67] = ".adh",
        [0x2447D742] = ".idm",
        [0x266E8A91] = ".lku",
        [0x2C4666D1] = ".smh",
        [0x2DC54131] = ".cdf",
        [0x30ED4060] = ".pth",
        [0x36E29465] = ".hkx",
        [0x38F66FC3] = ".seg",
        [0x430B4FF4] = ".ptl",
        [0x46810940] = ".egv",
        [0x4D894D5D] = ".cmi",
        [0x4E2FEF36] = ".mtg",
        [0x4F16B7AB] = ".hri",
        [0x50F9DB3E] = ".bfx",
        [0x5204D557] = ".shp",
        [0x538120DE] = ".eng",
        [0x557ECC08] = ".aef",
        [0x585831AA] = ".pos",
        [0x5898749C] = ".bgm",
        [0x60524FBB] = ".shw",
        [0x60DD1B16] = ".lsp",
        [0x758B2EB7] = ".cef",
        [0x7D1530C2] = ".sngw",
        [0x46FB08BA] = ".bmt",
        [0x285A13D9] = ".vzo",
        [0x4323D83A] = ".stex",
        [0x6A5CDD23] = ".occ",
        [0x62440501] = ".lmd",
        [0x3516C3D2] = ".lfd",

        // E.X. Troopers - These are not working
        [GetHash("BCP")] = ".bcp", // 0x6EEAD597
        [GetHash("BBP")] = ".bbp", // 0xBFC8697B
        [GetHash("EVP")] = ".evp"  // 0x6AB3D572
    };
}
