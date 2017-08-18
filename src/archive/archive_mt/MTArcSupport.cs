using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_mt
{
    public class MTArcFileInfo : ArchiveFileInfo
    {
        public FileMetadata meta { get; set; }
        public CompressionLevel compLvl { get; set; }

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived) return base.FileData;
                var ms = new MemoryStream();
                using (var ds = new DeflateStream(base.FileData, CompressionMode.Decompress, true))
                    ds.CopyTo(ms);
                return ms;
            }
        }

        public override long? FileSize => meta.uncompressedSize & 0x00ffffff;

        public void Write(Stream output, long offset, int compIdent)
        {
            using (var bw = new BinaryWriterX(output, true))
            {
                meta.offset = (int)offset;
                bw.Write((short)(compLvl == CompressionLevel.Optimal ? compIdent : 0x0178));

                if (State == ArchiveFileState.Archived)
                    base.FileData.CopyTo(bw.BaseStream);
                else
                {
                    byte[] bytes;
                    var startOffset = bw.BaseStream.Position;
                    using (var ds = new DeflateStream(bw.BaseStream, compLvl, true))
                    {
                        using (var br = new BinaryReaderX(FileData, true))
                            bytes = br.ReadBytes((int)br.BaseStream.Length);
                        ds.Write(bytes, 0, (int)FileData.Length);
                    }

                    meta.compressedSize = (int)(bw.BaseStream.Position - startOffset) + sizeof(short);
                    meta.uncompressedSize = (int)((meta.uncompressedSize & 0xff000000) | FileData.Length);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
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
