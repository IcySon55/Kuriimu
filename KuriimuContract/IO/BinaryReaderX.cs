using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kuriimu.IO
{
    public class BinaryReaderX : BinaryReader
    {
        int nibble = -1;

        public ByteOrder ByteOrder { get; set; }

        public BinaryReaderX(Stream input, ByteOrder byteOrder = ByteOrder.LittleEndian) : this(input, true, byteOrder)
        {
        }

        public BinaryReaderX(Stream input, bool leaveOpen, ByteOrder byteOrder = ByteOrder.LittleEndian) : base(input, Encoding.Unicode, leaveOpen)
        {
            ByteOrder = byteOrder;
        }

        public string ReadCStringA() => string.Concat(Enumerable.Range(0, 999).Select(_ => (char)ReadByte()).TakeWhile(c => c != 0));
        public string ReadCStringW() => string.Concat(Enumerable.Range(0, 999).Select(_ => (char)ReadInt16()).TakeWhile(c => c != 0));
        public T ReadStruct<T>() => ReadBytes(Marshal.SizeOf<T>()).ToStruct<T>();
        public List<T> ReadMultiple<T>(int count, Func<int, T> func) => Enumerable.Range(0, count).Select(func).ToList();
        public List<T> ReadMultiple<T>(int count) => Enumerable.Range(0, count).Select(_ => ReadStruct<T>()).ToList();

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

        public int ReadNibble()
        {
            if (nibble == -1)
            {
                nibble = ReadByte();
                return nibble % 16;
            }
            int val = nibble / 16;
            nibble = -1;
            return val;
        }
    }
}