using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Kontract;

namespace image_mods
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ModsHeader
    {
        public Magic ModsString;
        public ushort TagId;
        public ushort TagIdSizeDword;
        public uint FrameCount;
        public uint Width;
        public uint Height;
        public uint Fps;
        public ushort AudioCodec;
        public ushort NbChannel;
        public uint Frequency;
        public uint BiggestFrame;
        public uint AudioOffset;
        public uint KeyframeIndexOffset;
        public uint KeyframeCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class KeyFrameInfo
    {
        public uint FrameNumber;
        public uint DataOffset;
    }
}
