using Kuriimu.IO;
using Kuriimu.Kontract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archive_hunex
{
    public class HEDFileEntry
    {
        public string Name;
        public string Extension;
        public string Suffix;
        public int Offset;
        public long Size;

        public HEDFileEntry(BinaryReaderX blob, string name, string suffix)
        {
            Name = Path.GetFileNameWithoutExtension(name);
            Extension = Path.GetExtension(name) == "" ? ".mxz" : Path.GetExtension(name);
            Suffix = suffix;

            if (blob.BaseStream.Length == 8)
            {
                var offsetLow = blob.ReadUInt16();
                var offsetHigh = blob.ReadUInt16();
                var sizeSect = blob.ReadUInt16();
                var sizeLow = blob.ReadUInt16();

                Offset = 0x800 * (((offsetHigh & 0xF000) << 4) | offsetLow);
                Size = 0x800 * sizeSect;

                if (sizeLow != 0)
                    Size = (0x800 * (sizeSect - 1) & 0xFFFF0000) | sizeLow;
            }
            else if (blob.BaseStream.Length == 4)
            {
                var offsetLow = blob.ReadUInt16();
                // Contains 4 extra offset bits, as well as 12 bits for the Sect Size
                var offsetSizeHigh = blob.ReadUInt16();

                Offset = 0x800 * (offsetLow | ((offsetSizeHigh & 0xF000) << 4));
                Size = 0x800 * (offsetSizeHigh & 0x0FFF);
            }
            else
            {
                throw new Exception("HED Entry is of invalid size.");
            }
        }
    }

    public class HEDFileInfo : ArchiveFileInfo
    {
        public HEDFileEntry Entry;
    }

    public class NAM
    {
        private BinaryReaderX br = null;

        public string BaseTerm;
        public uint Length;
        public uint Total;
        public Dictionary<int, uint> Index;
        public long Size;

        public bool Indexed = false;

        public NAM(Stream namStream, string baseTerm)
        {
            Size = namStream.Length;
            BaseTerm = baseTerm;

            br = new BinaryReaderX(namStream, true);

            getInfo();
        }

        private void getInfo()
        {
            checkIndex();
            if (!Indexed)
            {
                checkNamLength();
                return;
            }
            checkNamTotal();
            makeNamIndex();
        }

        private void checkIndex()
        {
            byte[] bstr = br.ReadBytes(0x7);
            string str = Encoding.GetEncoding(932).GetString(bstr);

            if (str == "MRG.NAM")
            {
                Indexed = true;
                Index = new Dictionary<int, uint>();
            }
        }

        private void checkNamLength()
        {
            Length = 0x20;

            if (BaseTerm.Contains("voice"))
            {
                Length = 0x08;
            }
        }

        private void checkNamTotal()
        {
            br.BaseStream.Position = 0x10;
            Total = br.ReadUInt32();
        }

        private void makeNamIndex()
        {
            br.BaseStream.Position = 0x20;
            for (int i = 0; i < Total; ++i)
            {
                Index[i] = br.ReadUInt32();
            }

            int index = Convert.ToInt32(Total);
            uint value = Convert.ToUInt32(Size);

            Index[index] = value;
        }

        private string readString(byte[] bstr)
        {
            string str = "";

            // If CP-932 fails, fallback to CP-1252
            try
            {
                str = Encoding.GetEncoding(932).GetString(bstr).Replace("\r\n", "").Trim('\x00');
            }
            catch
            {
                str = Encoding.GetEncoding(1252).GetString(bstr).Replace("\r\n", "").Trim('\x00');
            }

            return str;
        }

        private byte[] getNameWithIndex(int index)
        {
            int len = Convert.ToInt32(Index[index + 1] - Index[index] - 4);
            br.BaseStream.Position = Index[index];

            uint count = br.ReadUInt32();
            if (index == count)
            {
                return br.ReadBytes(len);
            }
            else
            {
                return new byte[] { 0x0 };
            }
        }

        public string GetName(int index)
        {
            byte[] name;

            if (Indexed)
            {
                name = getNameWithIndex(index);
            }
            else
            {
                br.BaseStream.Position = Length * index;
                name = br.ReadBytes(Convert.ToInt32(Length));
            }

            return readString(name);
        }

        public void Close()
        {
            br?.Dispose();
            br = null;
        }
    }
}
