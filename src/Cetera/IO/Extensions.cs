using System.Linq;
using System.Text;
using Kuriimu.IO;

namespace Cetera.IO
{
    public static class Extensions
    {
        public static NW4CSectionList ReadSections(this BinaryReaderX br)
        {
            br.BaseStream.Position += 4;
            br.ByteOrder = (ByteOrder)br.ReadUInt16();
            br.BaseStream.Position -= 6;
            var lst = new NW4CSectionList { Header = br.ReadStruct<NW4CHeader>() };
            lst.AddRange(from _ in Enumerable.Range(0, lst.Header.section_count)
                         let magic1 = br.ReadStruct<String4>()
                         let data = br.ReadBytes(br.ReadInt32() - 4)
                         select new NW4CSection(magic1, data));
            return lst;
        }

        public static void WriteSections(this BinaryWriterX bw, NW4CSectionList sections)
        {
            bw.WriteStruct(sections.Header);
            foreach (var sec in sections)
            {
                bw.Write(Encoding.ASCII.GetBytes(sec.Magic)); // will need a magic->byte[] converter eventually
                bw.Write(sec.Data.Length + 8);
                bw.Write(sec.Data);
            }
        }
    }
}
