using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using LibMobiclip.Utils;

namespace image_moflex
{
    public class MoLiveDemux
    {
        public delegate void CompleteFrameReceivedEventHandler(MoLiveChunk Chunk, byte[] Data);

        public event CompleteFrameReceivedEventHandler OnCompleteFrameReceived;

        public MoLiveDemux(Stream Stream)
        {
            Reader = Stream;
        }

        public ulong Gts;
        public ulong DeltaGts;
        public Stream Reader;
        public uint PacketSize = 0;//?
        public uint SynchroCounter = 64;
        public uint LastCounter;
        public bool VariablePacketSize = true;
        public bool HasReferenceTs = false;
        public bool Synchronized;
        public bool ReaderIsDatagramBased;

        private class Endpoint
        {
            public Endpoint(MoLiveChunk Chunk)
            {
                this.Chunk = Chunk;
            }
            public MoLiveChunk Chunk { get; private set; }
            private List<byte> Data = new List<byte>();
            public void AddData(byte[] Data)
            {
                this.Data.AddRange(Data);
            }
            public void ClearData()
            {
                this.Data.Clear();
            }
            public byte[] GetData()
            {
                return Data.ToArray();
            }
        }

        private Dictionary<int, Endpoint> Streams = new Dictionary<int, Endpoint>();

        public void Desynchronize()
        {
            Gts = 0;
            DeltaGts = 0;
            SynchroCounter = 64;
            LastCounter = 65536;
            Synchronized = false;
            Streams.Clear();
        }

        public uint ReadPacket()
        {
            ulong ts;
            ushort packetsize;
            byte[] packet = new byte[PacketSize == 0 ? 0x1000 : PacketSize];//?
            int length = Reader.Read(packet, 0, packet.Length);
            Reader.Position -= length;
            if (!Synchronized)
            {
                if (length < 0xE) return 1;
                int offset = 0;
                while (!ReadSynchroHeader(packet, offset, out ts, out packetsize))
                {
                    offset++;
                    if (offset == length - 0xE) return 0x80;//synchronisation pattern not found
                }
                if ((long)ts - 1 < 0)
                {
                    HasReferenceTs = true;
                    ts &= 0x7FFFFFFFFFFFFFFFul;
                }
                else HasReferenceTs = false;
                if (packetsize < 0x10)
                {
                    //error
                    return 73;
                }
                Synchronized = true;
                Reader.Position += offset;
                return 0;
            }
            if (ReaderIsDatagramBased)
            {

            }
            else
            {
                if (PacketSize != 0 && PacketSize != length)
                {
                    //error
                    return 73;
                }
            }
            uint offset2 = 0;
            if (length > 0xE && ReadSynchroHeader(packet, 0, out ts, out packetsize))
            {
                if ((long)ts - 1 < 0)
                {
                    HasReferenceTs = true;
                    ts &= 0x7FFFFFFFFFFFFFFFul;
                }
                else HasReferenceTs = false;
                if (packetsize < 0x10)
                {
                    //error
                    return 73;
                }
                if (ts != 0)
                {
                    if (Gts != 0 && DeltaGts == 0)
                        DeltaGts = ts - Gts;
                    Gts = ts;
                    Streams.Clear();
                }
                if (PacketSize != packetsize)
                {
                    bool retry = (PacketSize == 0 ? 0x1000 : PacketSize) < packetsize;
                    PacketSize = packetsize;
                    if (retry) return 0;
                }
                offset2 = 0xE;
                uint size = (PacketSize > length ? (uint)length : PacketSize);
                while (true)
                {
                    uint result = ReadSynchroChunk(packet, ref offset2, size);
                    if (result == 0x100) break;
                    if (result != 0) return result;
                }
                if (offset2 > length) return 0x43;
            }
            uint result2 = ReadDataBlock(packet, ref offset2, (uint)length);
            if (!Synchronized) return 0;
            if (result2 == 0)
            {
                while (true)
                {
                    result2 = ReadEp(packet, ref offset2, (uint)length);
                    if (result2 == 0x101) break;
                    if (result2 != 0)
                        return result2;
                }
                if (offset2 > length)
                    return 0x43;
                Reader.Position += offset2;
                return 0;
            }
            return result2;
        }

        //int videostreamid = -1;

        public uint ReadSynchroChunk(byte[] packet, ref uint pos, uint psize)
        {
            //long curpos = packet.Position;
            //packet.Position += pos;
            uint type;
            uint size;
            if (!MoLive.ReadVariableByte(packet, out type, ref pos, psize)
                || !MoLive.ReadVariableByte(packet, out size, ref pos, psize))
                goto LABEL_119;
            MoLiveChunk chunk = null;
            switch (type)
            {
                case 0:
                    pos += size;
                    //packet.Position = curpos;
                    return 0x100;
                case 1://MoLiveStreamVideo 
                    chunk = new MoLiveStreamVideo() { Id = 1, Size = 12, StreamIndex = -1 };
                    break;
                case 2://MoLiveStreamAudio
                    chunk = new MoLiveStreamAudio() { Id = 2, Size = 6, StreamIndex = -1 };
                    break;
                case 3://MoLiveStreamVideoWithLayout
                    chunk = new MoLiveStreamVideoWithLayout() { Id = 3, Size = 13, StreamIndex = -1, ImageRotation = 0 };
                    break;
                case 4://MoLiveStreamTimeline
                    chunk = new MoLiveStreamTimeline() { Id = 4, Size = 2, StreamIndex = -1 };
                    break;
                case 0x100000://MoLiveChunkFoo
                    chunk = new MoLiveChunkFoo() { Id = 0x100000, Size = 20 };
                    break;
            }
            if (chunk == null) return 0x44;
            if (chunk.Size != size || chunk.Read(packet, (int)pos) == 0)
            {
                //error
                return 0x45;
            }
            //if (chunk is MoLiveStreamVideo || chunk is MoLiveStreamVideoWithLayout)
            //    videostreamid = ((MoLiveStream)chunk).StreamIndex;
            Streams.Add(((MoLiveStream)chunk).StreamIndex, new Endpoint(chunk));
            pos += size;
            if (pos <= psize) return 0;
            LABEL_119:
            Desynchronize();
            //packet.Position = curpos;
            return 0x43u;
        }

        public uint ReadDataBlock(byte[] packet, ref uint pos, uint psize)
        {
            if (pos >= psize)
            {
                Desynchronize();
                return 67;
            }
            byte flags = packet[pos++];
            VariablePacketSize = (flags & 1) == 1;
            bool PacketCounting = ((flags >> 1) & 1) == 1;
            uint synchrocounter = (uint)(flags >> 2);
            if (SynchroCounter == 64) SynchroCounter = synchrocounter;
            else if (SynchroCounter != synchrocounter)
            {
                if (DeltaGts == 0)
                {
                    Desynchronize();
                    return 70;
                }
                Gts += (synchrocounter - SynchroCounter) * DeltaGts;
                SynchroCounter = synchrocounter;
                //BreakStreams?
                foreach (Endpoint p in Streams.Values)
                    p.ClearData();
            }
            if (PacketCounting)
            {
                uint val = IOUtil.ReadU16BE(packet, (int)pos);
                pos += 2;
                if (pos > psize)
                {
                    Desynchronize();
                    return 67;
                }
                uint expectedval;
                if (LastCounter == 65536) expectedval = val;
                else expectedval = LastCounter + 1;
                if (expectedval != val)
                {
                    LastCounter = 65536;
                    //ClearPartialStreams?
                    return 0x50;
                }
                LastCounter = val;
            }
            return 0;
        }

        //public FileStream s2 = File.Create(@"d:\Old\Temp\3DS Files\MK7\Mobi\CourseSelectRace\video.bin");
        //bool startedframe = false;
        //int framesize = 0;
        //long framestartpos = 0;

        public uint ReadEp(byte[] packet, ref uint pos, uint psize)
        {
            if (pos == psize) return 0x101;
            if (pos > psize)
            {
                Desynchronize();
                return 0x43;
            }
            //long curpos = packet.Position;
            //packet.Position += pos;
            byte tmp = packet[pos];//(byte)packet.ReadByte();
            //packet.Position -= 1;
            if (tmp == 0)
            {
                pos++;
                uint EpPadding;
                if (VariablePacketSize) EpPadding = 0;
                else
                {
                    EpPadding = PacketSize - pos;
                    pos = PacketSize;
                }
                //if (!VariablePacketSize) pos = PacketSize;
                return 0x101;
            }

            int NrStreamIdxBits = 1;
            MoLiveInBitStream bs = new MoLiveInBitStream() { Value = 0, Stream = packet, Pos = pos, Remaining = 0 };
            while (bs.Pop(1) == 0) NrStreamIdxBits++;
            int StreamIdx = (int)bs.Pop(NrStreamIdxBits);

            bool EndFrame = bs.Pop(1) == 1;

            if (EndFrame)
            {
                int FrameTypeNrBits = 1;
                while (bs.Pop(1) == 0) FrameTypeNrBits++;
                int FrameType = (int)bs.Pop(FrameTypeNrBits);
                int v23 = 28;
                bool v35 = (int)bs.Pop(1) == 1;
                while (bs.Pop(1) == 0) v23 += 2;
                long v24 = (long)bs.Pop(v23);
                int v26 = (int)(v24 >> 32);
                int v25 = (int)v24;
                if (v35)
                {
                    v25 = (int)-v24;
                    v26 = -((~((v24 <= 0) ? 1 : 0)) + (int)(v24 >> 32));
                }
            }
            int EPSize = (int)bs.Pop(0xD) + 1;
            pos = bs.Pos;//(uint)bs.Stream.Position - (uint)curpos;

            if (pos + EPSize > psize)
            {
                Desynchronize();
                return 0x43;
            }
            byte[] epdata = new byte[EPSize];
            Array.Copy(packet, pos, epdata, 0, EPSize);
            if (Streams.ContainsKey(StreamIdx))
                Streams[StreamIdx].AddData(epdata);
            /*if (StreamIdx == videostreamid)
            {
                if (!startedframe)
                {
                    //s2.WriteByte((byte)(EPSize & 0xFF));
                    //s2.WriteByte((byte)((EPSize >> 8) & 0xFF));
                    framestartpos = s2.Position;
                    s2.WriteByte(0);
                    s2.WriteByte(0);
                    framesize = 0;
                    startedframe = true;
                }
                framesize += EPSize;
                s2.Write(epdata, 0, epdata.Length);
            }*/
            pos += (uint)EPSize;

            if (EndFrame)// && StreamIdx == videostreamid)
            {
                if (Streams.ContainsKey(StreamIdx))
                {
                    Streams[StreamIdx].AddData(new byte[2]);
                    if (OnCompleteFrameReceived != null)
                    {
                        OnCompleteFrameReceived(Streams[StreamIdx].Chunk, Streams[StreamIdx].GetData());
                    }
                    Streams[StreamIdx].ClearData();
                }
                /*if (startedframe)
                {
                    startedframe = false;
                    long curpos = s2.Position;
                    s2.Position = framestartpos;
                    s2.WriteByte((byte)(framesize & 0xFF));
                    s2.WriteByte((byte)((framesize >> 8) & 0xFF));
                    s2.Position = curpos;
                }*/
            }

            if (pos < psize) return 0;
            return 0x101;
        }

        public static bool ReadSynchroHeader(/*Stream packet*/byte[] packet, int offset, out ulong ts, out ushort packetSize)
        {
            ts = 0;
            packetSize = 0;
            //long curpos = packet.Position;
            if (!(/*packet.ReadByte()*/packet[offset] == 0x4C && /*packet.ReadByte()*/packet[offset + 1] == 0x32))
            {
                //packet.Position = curpos;
                return false;
            }
            offset += 2;

            uint v10 = IOUtil.ReadU16BE(packet, offset);// (uint)((packet.ReadByte() << 8) | packet.ReadByte());
            offset += 2;
            uint v13 = (IOUtil.ReadU32BE(packet, offset) & 0xFFFFFF00) | packet[offset + 3];//(uint)((packet.ReadByte() << 24) | (packet.ReadByte() << 16) | (packet.ReadByte() << 8) | packet.ReadByte());
            offset += 4;
            //(packet[offset + 0] << 24) | (packet[offset + 1] << 16) | (packet[offset + 2] << 8) | packet[offset + 3] | (v13 << 32)
            uint v12 = (uint)packet[offset++]/*packet.ReadByte()*/ << 24;
            uint v14 = (uint)packet[offset++];//packet.ReadByte();
            uint v15 = v12 | (v14 << 16);
            uint v16 = v13 | (v14 >> 16);
            uint v17 = (uint)packet[offset++];//packet.ReadByte();
            ts = v15 | (v17 << 8) | ((uint)packet[offset++]/*packet.ReadByte()*/) | (((ulong)(v16 | (v17 >> 24))) << 32);

            uint v19 = (uint)((ts >> 32) & 0xFFFFFFFF);

            if ((int)((ts >> 32) - 1) < 0) v19 &= 0x7FFFFFFF;

            packetSize = (ushort)(IOUtil.ReadU16BE(packet, offset) + 1);//(ushort)(((packet.ReadByte() << 8) | packet.ReadByte()) + 1);
            offset += 2;

            if (v10 == (((ts >> 16) & 0xFFFF) ^ (v19 >> 16) ^ 0xAAAA ^ (v19 & 0xFFFF) ^ (ts & 0xFFFF)))
            {
                //packet.Position = curpos;
                return true;
            }

            //packet.Position = curpos;
            return false;
        }
    }
}
