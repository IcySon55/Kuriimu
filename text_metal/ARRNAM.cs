using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace text_metal
{
    public sealed class ARRNAM
    {
        public List<Entry> Entries = new List<Entry>();
        private List<ArrEntry> ArrEntries;

        public ARRNAM(Stream namInput, Stream arrInput)
        {
            using (var br = new BinaryReaderX(arrInput, false))
            {
                ArrEntries = br.ReadMultiple<ArrEntry>((int)br.BaseStream.Length / 12);
            }

            using (var br = new BinaryReaderX(namInput, false))
            {
                for (int i = 0; i < ArrEntries.Count; i++)
                {

                }

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    br.BaseStream.Position = 0x2;

                    var temp = "";
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        var chars = br.ReadBytes(2);
                        if (chars[0] == 0x0 && chars[1] == 0x0)
                        {
                            Entries.Add(new Entry());
                            temp = "";
                            continue;
                        }
                        temp += Encoding.Unicode.GetString(chars);
                    }
                }
            }
        }

        public void Save(Stream namOutput, Stream arrOutput)
        {
            using (var bw = new BinaryWriterX(namOutput))
                foreach (var name in Entries)
                {
                    //var delta = MessageLength - shift.GetBytes(name.Text).Length;

                    //if (delta > 0)
                    //{
                    //    bw.Write(shift.GetBytes(name.Text));
                    //    for (var i = 0; i < delta; i++)
                    //        bw.Write((byte)0x0);
                    //}
                    //else
                    //{
                    //    bw.Write(shift.GetBytes(name.Text).Take(MessageLength - 1).ToArray());
                    //    bw.Write((byte)0x0);
                    //}
                }
        }
    }
}