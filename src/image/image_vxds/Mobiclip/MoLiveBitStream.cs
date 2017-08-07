using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace image_moflex
{
    public class MoLiveInBitStream
    {
        public UInt64 Value;
        public UInt32 Remaining;
        public Byte[] Stream;
        public uint Pos;

        public ulong Pop(int NrBits)
        {
            if (NrBits > 64) throw new ArgumentException();
            uint v5 = 64 - ((64 - Remaining) & 7);
            if (NrBits > v5)
            {
                uint v12 = (uint)NrBits - v5;
                if (Remaining < v5)
                {
                    do
                    {
                        Value |= ((ulong)Stream[Pos++]/*Stream.ReadByte()*/) << (int)(56 - Remaining);
                        Remaining += 8;
                    }
                    while (Remaining < v5);
                }
                byte data = Stream[Pos++];//(byte)Stream.ReadByte();
                ulong res1 = ((ulong)(Value >> (int)(64 - v5)) << (int)v12);
                Value = ((ulong)data) << (int)(v12 + 56);
                Remaining = 8 - v12;
                return res1 | (((ulong)data) >> (int)(8 - v12));
            }
            else
            {
                if (Remaining < NrBits)
                {
                    do
                    {
                        Value |= ((ulong)Stream[Pos++]/*Stream.ReadByte()*/) << (int)(56 - Remaining);
                        Remaining += 8;
                    }
                    while (Remaining < NrBits);
                }
                ulong v10 = Value >> (int)(64 - NrBits);
                Value = Value << (int)NrBits;
                Remaining -= (uint)NrBits;
                return v10;
            }
        }

    }
}
