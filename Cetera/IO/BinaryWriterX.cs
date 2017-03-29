using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Cetera.IO
{
    public class BinaryWriterX : BinaryWriter
    {
        int nibble = -1;

        public BinaryWriterX(Stream output, bool leaveOpen = false) : base(output, Encoding.Unicode, leaveOpen)
        {
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

        public void WriteSections(NW4CSectionList sections)
        {
            WriteStruct(sections.Header);
            foreach (var sec in sections)
            {
                Write(Encoding.ASCII.GetBytes(sec.Magic)); // will need a magic->byte[] converter eventually
                Write(sec.Data.Length + 8);
                Write(sec.Data);
            }
        }
    }
}
