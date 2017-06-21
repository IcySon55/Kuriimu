using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace text_metal
{
    public sealed class NAM
    {
        public List<string> Entries = new List<string>();

        public NAM(Stream input)
        {
            using (var br = new BinaryReaderX(input, false))
            {   
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    br.BaseStream.Position = 0x2;

                    var temp = "";
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        var chars = br.ReadBytes(2);
                        if (chars[0] == 0x0 && chars[1] == 0x0)
                        {
                            Entries.Add(temp);
                            temp = "";
                            continue;
                        }
                        temp += Encoding.Unicode.GetString(chars);
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
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