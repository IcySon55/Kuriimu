using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;

namespace archive_mt
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ArcHeader
    {
        public Magic magic;
        public short version;
        public short entryCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class FileMetadata
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string filename;
        public uint extensionHash;
        public int compressedSize;
        public int uncompressedSize;
        public int offset;
    }

    public class MTArcFileInfo : ArchiveFileInfo
    {
        public FileMetadata Metadata { get; set; }
        public CompressionLevel CompressionLevel { get; set; }
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

            // E.X. Troopers - These are not working
            [GetHash("BCP")] = ".bcp", // 0x6EEAD597
            [GetHash("BBP")] = ".bbp", // 0xBFC8697B
            [GetHash("EVP")] = ".evp"  // 0x6AB3D572
        };
    }
}
