using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibMobiclip.Utils;

namespace image_moflex
{
    public abstract class MoLiveStream : MoLiveChunk
    {
        public int StreamIndex;

        public sealed override bool IsStream() { return true; }
    }

    public abstract class MoLiveStreamCodec : MoLiveStream
    {
        public uint CodecId;
    }

    public class MoLiveStreamVideo : MoLiveStreamCodec
    {
        public MoLiveStreamVideo() { }

        public MoLiveStreamVideo(uint FpsRate, uint FpsScale, uint Width, uint Height, uint PelRatioRate, uint PelRatioScale)
        {
            Id = 1;
            Size = 12;
            this.FpsRate = FpsRate;
            this.FpsScale = FpsScale;
            this.Width = Width;
            this.Height = Height;
            this.PelRatioRate = PelRatioRate;
            this.PelRatioScale = PelRatioScale;
        }

        public uint FpsRate;
        public uint FpsScale;
        public uint Width;
        public uint Height;
        public uint PelRatioRate;
        public uint PelRatioScale;

        public override int Read(byte[] Data, int Offset)
        {
            if (Data == null || Data.Length == 0) return -1;
            int offset = Offset;
            StreamIndex = Data[offset++];
            if (offset >= Data.Length) return -1;
            CodecId = Data[offset++];
            if (Data.Length - offset < 0xA) return -1;
            FpsRate = IOUtil.ReadU16BE(Data, offset);
            FpsScale = IOUtil.ReadU16BE(Data, offset + 2);
            Width = IOUtil.ReadU16BE(Data, offset + 4);
            Height = IOUtil.ReadU16BE(Data, offset + 6);
            PelRatioRate = Data[offset + 8];
            PelRatioScale = Data[offset + 9];
            offset += 0xA;
            return offset;
        }

        public override void Write(Stream Destination)
        {
            byte[] result = new byte[12];
            result[0] = (byte)StreamIndex;
            result[1] = (byte)CodecId;
            IOUtil.WriteU16BE(result, 2, (ushort)FpsRate);
            IOUtil.WriteU16BE(result, 4, (ushort)FpsScale);
            IOUtil.WriteU16BE(result, 6, (ushort)Width);
            IOUtil.WriteU16BE(result, 8, (ushort)Height);
            result[10] = (byte)PelRatioRate;
            result[11] = (byte)PelRatioScale;
            Destination.Write(result, 0, result.Length);
        }
    }

    public class MoLiveStreamAudio : MoLiveStreamCodec
    {
        public uint Frequency;
        public uint Channel;

        public override int Read(byte[] Data, int Offset)
        {
            if (Data == null || Data.Length == 0) return -1;
            int offset = Offset;
            StreamIndex = Data[offset++];
            if (offset >= Data.Length) return -1;
            CodecId = Data[offset++];
            if (Data.Length - offset < 0x4) return -1;
            Frequency = IOUtil.ReadU24BE(Data, offset) + 1;
            Channel = (uint)(Data[offset + 3] + 1);
            offset += 4;
            return offset;
        }

        public override void Write(Stream Destination)
        {
            byte[] result = new byte[6];
            result[0] = (byte)StreamIndex;
            result[1] = (byte)CodecId;
            IOUtil.WriteU24BE(result, 2, Frequency - 1);
            result[5] = (byte)(Channel - 1);
            Destination.Write(result, 0, result.Length);
        }
    }

    public class MoLiveStreamVideoWithLayout : MoLiveStreamVideo
    {
        public enum VideoLayout : uint
        {
            Interleave3DLeftFirst,
            Interleave3DRightFirst,
            TopToBottom3DLeftFirst,
            TopToBottom3DRightFirst,
            SideBySide3DLeftFirst,
            SideBySide3DRightFirst,
            Simple2D
        }

        public VideoLayout ImageLayout;
        public uint ImageRotation;

        public override int Read(byte[] Data, int Offset)
        {
            if (Data == null || Data.Length == 0) return -1;
            int offset = Offset;
            StreamIndex = Data[offset++];
            if (offset >= Data.Length) return -1;
            CodecId = Data[offset++];
            if (Data.Length - offset < 0xA) return -1;
            FpsRate = IOUtil.ReadU16BE(Data, offset);
            FpsScale = IOUtil.ReadU16BE(Data, offset + 2);
            Width = IOUtil.ReadU16BE(Data, offset + 4);
            Height = IOUtil.ReadU16BE(Data, offset + 6);
            PelRatioRate = Data[offset + 8];
            PelRatioRate = Data[offset + 9];
            offset += 0xA;
            if (offset >= Data.Length) return -1;
            ImageLayout = (VideoLayout)(Data[offset] & 0xF);
            ImageRotation = (uint)(Data[offset] >> 4);
            offset++;
            return offset;
        }

        public override void Write(Stream Destination)
        {
            base.Write(Destination);
            Destination.WriteByte((byte)(((uint)ImageLayout & 0xF) | ((ImageRotation & 0xF) << 4)));
        }
    }

    public class MoLiveStreamTimeline : MoLiveStream
    {
        public uint AssociatedStreamIndex;

        public override int Read(byte[] Data, int Offset)
        {
            if (Data == null || Data.Length == 0) return -1;
            int offset = Offset;
            StreamIndex = Data[offset++];
            if (offset >= Data.Length) return -1;
            AssociatedStreamIndex = Data[offset++];
            return offset;
        }

        public override void Write(Stream Destination)
        {
            Destination.WriteByte((byte)StreamIndex);
            Destination.WriteByte((byte)AssociatedStreamIndex);
        }
    }

    public class MoLiveChunkFoo : MoLiveChunk
    {
        public byte[] Bar;

        public override int Read(byte[] Data, int Offset)
        {
            throw new NotImplementedException();
        }

        public override void Write(Stream Destination)
        {
            throw new NotImplementedException();
        }
    }
}
