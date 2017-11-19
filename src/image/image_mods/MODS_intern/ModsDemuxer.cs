using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LibMobiclip.Utils;
using Komponent.IO;

namespace image_mods
{
    public class ModsDemuxer
    {
        private Stream Stream;
        private uint CurFrame;
        private int NextKeyFrame;

        public List<byte[]> AudioCodebooks;
        public List<KeyFrameInfo> KeyFrames;

        public ModsHeader Header;

        public ModsDemuxer(Stream input)
        {
            Stream = input;
            using (var br = new BinaryReaderX(Stream, true))
            {
                //Header
                Header = br.ReadStruct<ModsHeader>();

                //Audio
                if (Header.AudioOffset != 0)
                {
                    br.BaseStream.Position = Header.AudioOffset;
                    for (int i = 0; i < Header.NbChannel; i++) AudioCodebooks.Add(br.ReadBytes(0xc34));
                }

                //KeyFrames
                br.BaseStream.Position = Header.KeyframeIndexOffset;
                KeyFrames = br.ReadMultiple<KeyFrameInfo>((int)Header.KeyframeCount);

                JumpToKeyFrame(0);
            }
        }

        private void JumpToKeyFrame(int KeyFrame)
        {
            using (var br = new BinaryReaderX(Stream, true))
            {
                if (KeyFrame >= Header.KeyframeCount) return;

                br.BaseStream.Position = KeyFrames[KeyFrame].DataOffset;
                CurFrame = KeyFrames[KeyFrame].FrameNumber;

                if (KeyFrame + 1 < KeyFrames.Count) NextKeyFrame = KeyFrame + 1;
                else NextKeyFrame = -1;
            }
        }

        public byte[] ReadFrame(out uint NrAudioPackets, out bool IsKeyFrame)
        {
            NrAudioPackets = 0;
            IsKeyFrame = false;
            if (CurFrame >= Header.FrameCount) return null;
            if (NextKeyFrame >= 0 && NextKeyFrame < KeyFrames.Count && CurFrame == KeyFrames[NextKeyFrame].FrameNumber)
            {
                IsKeyFrame = true;
                if (NextKeyFrame + 1 < KeyFrames.Count) NextKeyFrame++;
                else NextKeyFrame = -1;
            }
            CurFrame++;
            byte[] tmp = new byte[4];
            Stream.Read(tmp, 0, 4);
            uint PacketInfo = IOUtil.ReadU32LE(tmp, 0);
            uint PacketSize = PacketInfo >> 14;
            NrAudioPackets = PacketInfo & 0x3FFF;
            byte[] completepacket = new byte[PacketSize];
            Stream.Read(completepacket, 0, (int)PacketSize);
            return completepacket;
        }
    }
}
