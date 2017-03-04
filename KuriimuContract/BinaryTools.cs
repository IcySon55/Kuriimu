using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KuriimuContract
{
    public class BinaryReaderX : BinaryReader
    {
        int nibbles = -1;

        public ByteOrder ByteOrder { get; set; }

        public string ReadCStringA() => string.Concat(Enumerable.Range(0, 999).Select(_ => (char)ReadByte()).TakeWhile(c => c != 0));
        public string ReadCStringW() => string.Concat(Enumerable.Range(0, 999).Select(_ => (char)ReadInt16()).TakeWhile(c => c != 0));
        public List<T> ReadMultiple<T>(int count, Func<int, T> func) => Enumerable.Range(0, count).Select(func).ToList();

        public BinaryReaderX(Stream input, ByteOrder byteOrder = ByteOrder.LittleEndian)
            : base(input)
        {
            ByteOrder = byteOrder;
        }

        public BinaryReaderX(Stream input, Encoding encoding, ByteOrder byteOrder = ByteOrder.LittleEndian)
            : base(input, encoding)
        {
            ByteOrder = byteOrder;
        }

        public override short ReadInt16()
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                return base.ReadInt16();
            else
                return BitConverter.ToInt16(base.ReadBytes(2).Reverse().ToArray(), 0);
        }

        public override int ReadInt32()
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                return base.ReadInt32();
            else
                return BitConverter.ToInt32(base.ReadBytes(4).Reverse().ToArray(), 0);
        }

        public override long ReadInt64()
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                return base.ReadInt64();
            else
                return BitConverter.ToInt64(base.ReadBytes(8).Reverse().ToArray(), 0);
        }

        public override ushort ReadUInt16()
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                return base.ReadUInt16();
            else
                return BitConverter.ToUInt16(base.ReadBytes(2).Reverse().ToArray(), 0);
        }

        public override uint ReadUInt32()
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                return base.ReadUInt32();
            else
                return BitConverter.ToUInt32(base.ReadBytes(4).Reverse().ToArray(), 0);
        }

        public override ulong ReadUInt64()
        {
            if (ByteOrder == ByteOrder.LittleEndian)
                return base.ReadUInt64();
            else
                return BitConverter.ToUInt64(base.ReadBytes(8).Reverse().ToArray(), 0);
        }

        // Custom Methods
        public byte[] ReadBytesUntil(byte stop)
        {
            List<byte> result = new List<byte>();

            byte b = ReadByte();
            while (b != stop && BaseStream.Position < BaseStream.Length)
            {
                result.Add(b);
                b = ReadByte();
            }

            return result.ToArray();
        }

        public string ReadASCIIStringUntil(byte stop)
        {
            string result = string.Empty;

            byte b = ReadByte();
            while (b != stop && BaseStream.Position < BaseStream.Length)
            {
                result += (char)b;
                b = ReadByte();
            }

            return result;
        }

        public string ReadString(int length)
        {
            return Encoding.ASCII.GetString(ReadBytes(length)).TrimEnd('\0');
        }

        public string ReadString(int length, Encoding encoding)
        {
            return encoding.GetString(ReadBytes(length)).TrimEnd('\0');
        }

        public string PeekString(int length = 4)
        {
            List<byte> bytes = new List<byte>();
            long startOffset = BaseStream.Position;

            for (int i = 0; i < length; i++)
                bytes.Add(ReadByte());

            BaseStream.Seek(startOffset, SeekOrigin.Begin);

            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        public string PeekString(int length, Encoding encoding)
        {
            List<byte> bytes = new List<byte>();
            long startOffset = BaseStream.Position;

            for (int i = 0; i < length; i++)
                bytes.Add(ReadByte());

            BaseStream.Seek(startOffset, SeekOrigin.Begin);

            return encoding.GetString(bytes.ToArray());
        }

        public unsafe T ReadStruct<T>()
        {
            fixed (byte* pBuffer = ReadBytes(Marshal.SizeOf<T>()))
                return Marshal.PtrToStructure<T>((IntPtr)pBuffer);
        }

        public int ReadNibble()
        {
            if (nibbles == -1)
            {
                nibbles = ReadByte();
                return nibbles % 16;
            }
            else
            {
                int val = nibbles / 16;
                nibbles = -1;
                return val;
            }
        }
    }

    public class BinaryWriterX : BinaryWriter
    {
        int nibble = -1;

        public ByteOrder ByteOrder { get; set; }

        public BinaryWriterX(Stream input, ByteOrder byteOrder = ByteOrder.LittleEndian)
            : base(input)
        {
            ByteOrder = byteOrder;
        }

        public BinaryWriterX(Stream input, Encoding encoding, ByteOrder byteOrder = ByteOrder.LittleEndian)
            : base(input, encoding)
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

        public void WriteASCII(string value)
        {
            base.Write(Encoding.ASCII.GetBytes(value));
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
    }
}