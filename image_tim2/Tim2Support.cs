using System.Runtime.InteropServices;
using Kuriimu.Contract;

namespace image_tim2
{
    public enum gatterType : byte
    {
        TIM2_NONE,
        TIM2_RGB16,
        TIM2_RGB24,
        TIM2_RGB32,
        TIM2_IDTEX4,
        TIM2_IDTEX8
    }

    public enum PSM : byte
    {
        SCE_GS_PSMCT16,
        SCE_GS_PSMCT24,
        SCE_GS_PSMCT32,
        SCE_GS_PSMT4,
        SCE_GS_PSMT8
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public Magic magic; //TIM2
        public byte version;
        public byte id;
        public ushort picCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PicHeader
    {
        public uint totalSize;
        public uint clutSize;
        public uint imageSize;
        public ushort headerSize;
        public ushort clutColorCount;
        public byte picFormat;
        public byte mipMapTextureCount;
        public byte clutType;
        public byte imageType;
        public ushort width;
        public ushort height;

        public ulong gsTex0;
        public ulong gsTex1;
        public uint gsTexaFbaPabe;
        public uint gsTexClut;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ExtHeader
    {
        public Magic magic; //ext
        public ushort userSpaceSize;
        public ushort userDataSize;
        public ushort reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GsTex
    {
        public PSM psm;
    }
}
