using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kuriimu.IO
{
    public class BinaryWriterX : BinaryWriter
    {
        int nibble = -1;

        public ByteOrder ByteOrder { get; set; }

        public BinaryWriterX(Stream input, ByteOrder byteOrder = ByteOrder.LittleEndian) : this(input, false, byteOrder)
        {
        }

        public BinaryWriterX(Stream input, bool leaveOpen, ByteOrder byteOrder = ByteOrder.LittleEndian) : base(input, Encoding.Unicode, leaveOpen)
        {
            ByteOrder = byteOrder;
        }

        public override void Write(short value)
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                base.Write(value);
            else
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(int value)
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                base.Write(value);
            else
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(long value)
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                base.Write(value);
            else
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(ushort value)
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                base.Write(value);
            else
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(uint value)
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                base.Write(value);
            else
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        public override void Write(ulong value)
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                base.Write(value);
            else
                base.Write(BitConverter.GetBytes(value).Reverse().ToArray());
        }

        // Custom Methods
        public void WriteASCII(string value)
        {
            base.Write(Encoding.ASCII.GetBytes(value));
        }

        public void WriteString(Encoding encoding, string str)
        {
            var bytes = encoding.GetBytes(str);
            Write((byte)bytes.Length);
            Write(bytes);
        }

        public unsafe void WriteStruct<T>(T item)
        {
            var buffer = new byte[Marshal.SizeOf(typeof(T))];
            fixed (byte* pBuffer = buffer)
            {
                Marshal.StructureToPtr(item, (IntPtr)pBuffer, false);
            }
            Write(buffer);
        }

        public void WriteNibble(int val)
        {
            val &= 15;
            if (nibble == -1)
            {
                nibble = val;
            }
            else
            {
                Write((byte)(nibble + 16 * val));
                nibble = -1;
            }
        }

        public void WritePadding(int alignment = 16, byte paddingByte = 0x0)
        {
            long remainder = BaseStream.Position % alignment;
            if (remainder > 0)
                for (int i = 0; i < alignment - remainder; i++)
                    Write(paddingByte);
        }
    }
}