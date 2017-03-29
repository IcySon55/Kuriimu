using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Cetera.IO
{
    public class BinaryReaderX : BinaryReader
    {
        int nibbles = -1;

        public BinaryReaderX(Stream input, bool leaveOpen = false) : base(input, Encoding.Unicode, leaveOpen)
        {
        }

        public string ReadCStringA() => string.Concat(Enumerable.Range(0, 999).Select(_ => (char)ReadByte()).TakeWhile(c => c != 0));
        public string ReadCStringW() => string.Concat(Enumerable.Range(0, 999).Select(_ => (char)ReadInt16()).TakeWhile(c => c != 0));
        public T ReadStruct<T>() => ReadBytes(Marshal.SizeOf<T>()).ToStruct<T>();
        public List<T> ReadMultiple<T>(int count, Func<int, T> func) => Enumerable.Range(0, count).Select(func).ToList();

        public int ReadNibble()
        {
            if (nibbles == -1)
            {
                nibbles = ReadByte();
                return nibbles % 16;
            }
            int val = nibbles / 16;
            nibbles = -1;
            return val;
        }

        public NW4CSectionList ReadSections()
        {
            var lst = new NW4CSectionList { Header = ReadStruct<NW4CHeader>() };
            lst.AddRange(from _ in Enumerable.Range(0, lst.Header.section_count)
                         let magic1 = ReadStruct<String4>()
                         let data = ReadBytes(ReadInt32() - 8)
                         select new NW4CSection(magic1, data));
            return lst;
        }
    }
}
