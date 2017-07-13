using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LibMobiclip.Utils;

namespace image_vxds
{
    public class VxDemuxer
    {
        private Stream Stream;

        public VxDemuxer(Stream Stream)
        {
            this.Stream = Stream;
            Header = new VxHeader(Stream);
            KeyFrames = new KeyFrameInfo[Header.KeyframeCount];
            Stream.Position = Header.KeyframeIndexOffset;
            byte[] tmp = new byte[8];
            for (int i = 0; i < Header.KeyframeCount; i++)
            {
                KeyFrames[i] = new KeyFrameInfo();
                Stream.Read(tmp, 0, 8);
                KeyFrames[i].FrameNumber = IOUtil.ReadU32LE(tmp, 0);
                KeyFrames[i].DataOffset = IOUtil.ReadU32LE(tmp, 4);
            }
            JumpToKeyFrame(0);
        }

        public VxHeader Header { get; private set; }
        public class VxHeader
        {
            public VxHeader(Stream Stream)
            {
                byte[] Data = new byte[0x30];
                Stream.Read(Data, 0, 0x30);
                VxString = Encoding.ASCII.GetString(Data, 0, 4);
                FrameCount = IOUtil.ReadU32LE(Data, 4);
                Width = IOUtil.ReadU32LE(Data, 0x8);
                Height = IOUtil.ReadU32LE(Data, 0xC);
                Fps = IOUtil.ReadU32LE(Data, 0x10);
                Unknown = IOUtil.ReadU32LE(Data, 0x14);
                Frequency = IOUtil.ReadU32LE(Data, 0x18);
                ChannelCount = IOUtil.ReadU32LE(Data, 0x1C);
                if (ChannelCount > 16)
                {
                    ChannelCount = 0;
                    BiggestFrame = IOUtil.ReadU32LE(Data, 0x1C);
                    AudioOffset = IOUtil.ReadU32LE(Data, 0x20);
                    KeyframeIndexOffset = IOUtil.ReadU32LE(Data, 0x24);
                    KeyframeCount = IOUtil.ReadU32LE(Data, 0x28);
                }
                else
                {
                    BiggestFrame = IOUtil.ReadU32LE(Data, 0x20);
                    AudioOffset = IOUtil.ReadU32LE(Data, 0x24);
                    KeyframeIndexOffset = IOUtil.ReadU32LE(Data, 0x28);
                    KeyframeCount = IOUtil.ReadU32LE(Data, 0x2C);
                }
            }
            public String VxString;
            public UInt32 FrameCount;
            public UInt32 Width;
            public UInt32 Height;
            public UInt32 Fps;
            public UInt32 Unknown;
            public UInt32 Frequency;
            public UInt32 ChannelCount;
            public UInt32 BiggestFrame;
            public UInt32 AudioOffset;
            public UInt32 KeyframeIndexOffset;
            public UInt32 KeyframeCount;
        }
        public KeyFrameInfo[] KeyFrames;
        public class KeyFrameInfo
        {
            public UInt32 FrameNumber;
            public UInt32 DataOffset;
        }

        private void JumpToKeyFrame(int KeyFrame)
        {
            if (KeyFrame >= Header.KeyframeCount) return;
            Stream.Position = KeyFrames[KeyFrame].DataOffset;
        }

        public byte[] ReadFrame(out uint NrAudioPackets)
        {
            byte[] tmp = new byte[4];
            Stream.Read(tmp, 0, 4);
            uint PacketInfo = IOUtil.ReadU32LE(tmp, 0);
            uint PacketSize = PacketInfo & 0xFFFF;
            NrAudioPackets = PacketInfo >> 16;
            byte[] completepacket = new byte[PacketSize];
            Stream.Read(completepacket, 0, (int)PacketSize);
            return completepacket;
        }
    }
}
