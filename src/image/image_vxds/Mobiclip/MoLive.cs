using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace image_moflex
{
    public abstract class MoLiveChunk
    {
        public uint Id;
        public uint Size;

        public virtual bool IsStream() { return false; }
        public abstract int Read(byte[] Data, int Offset);
        public abstract void Write(Stream Destination);
    }

    public class MoLive
    {
        public static bool ReadVariableByte(Stream src, out uint value, ref uint pos, uint psize)
        {
            value = 0;
            if (pos == psize) return false;
            byte data = (byte)src.ReadByte();
            pos++;
            if ((data & 0x80) == 0) { value = data; return true; }
            if (pos == psize) return false;
            value = (uint)(data & 0x7F) << 7;
            data = (byte)src.ReadByte();
            pos++;
            if ((data & 0x80) == 0) { value |= data; return true; }
            if (pos == psize) return false;
            value = ((uint)(data & 0x7F) | value) << 7;
            data = (byte)src.ReadByte();
            pos++;
            if ((data & 0x80) == 0) { value |= data; return true; }
            if (pos == psize) return false;
            value = (((uint)(data & 0x7F) | value) << 7) | ((byte)src.ReadByte());
            pos++;
            return true;
        }

        public static bool ReadVariableByte(byte[] src, out uint value, ref uint pos, uint psize)
        {
            value = 0;
            if (pos == psize) return false;
            byte data = src[pos++];
            if ((data & 0x80) == 0) { value = data; return true; }
            if (pos == psize) return false;
            value = (uint)(data & 0x7F) << 7;
            data = src[pos++];
            if ((data & 0x80) == 0) { value |= data; return true; }
            if (pos == psize) return false;
            value = ((uint)(data & 0x7F) | value) << 7;
            data = src[pos++];
            if ((data & 0x80) == 0) { value |= data; return true; }
            if (pos == psize) return false;
            value = (((uint)(data & 0x7F) | value) << 7) | src[pos++];
            return true;
        }

        public static void WriteVariableByte(Stream dst, uint value)
        {
            if ((value & 0xF0000000) != 0) throw new Exception();
            if ((value & ~0x7F) == 0)
            {
                dst.WriteByte((byte)value);
                return;
            }
            if ((value & ~0x1FFF) == 0)
            {
                dst.WriteByte((byte)((value >> 7) | 0x80));
                dst.WriteByte((byte)(value & 0x7F));
                return;
            }
            if ((value & ~0x1FFFFF) == 0)
            {
                dst.WriteByte((byte)((value >> 14) | 0x80));
                dst.WriteByte((byte)(((value >> 7) & 0x7F) | 0x80));
                dst.WriteByte((byte)(value & 0x7F));
                return;
            }
            dst.WriteByte((byte)((value >> 21) | 0x80));
            dst.WriteByte((byte)((value >> 14) | 0x80));
            dst.WriteByte((byte)(((value >> 7) & 0x7F) | 0x80));
            dst.WriteByte((byte)(value & 0x7F));
        }
    }
}
