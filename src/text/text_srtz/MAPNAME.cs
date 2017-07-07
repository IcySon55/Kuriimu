using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.IO;

namespace text_srtz
{
    public class MAPNAME
    {
        public const int MessageLength = 0x100;

        public List<MapName> MapNames = new List<MapName>();

        public MAPNAME(Stream input)
        {
            using (var br = new BinaryReaderX(input, false))
            {
                var index = 0;
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    MapNames.Add(new MapName
                    {
                        Index = index,
                        Text = br.ReadString(MessageLength, Encoding.GetEncoding("shift-jis"))
                    });
                    index++;
                }
            }
        }

        public void Save(Stream output)
        {
            var shift = Encoding.GetEncoding("shift-jis");

            using (var bw = new BinaryWriterX(output))
                foreach (var name in MapNames)
                {
                    var delta = MessageLength - shift.GetBytes(name.Text).Length;

                    if (delta > 0)
                    {
                        bw.Write(shift.GetBytes(name.Text));
                        for (var i = 0; i < delta; i++)
                            bw.Write((byte)0x0);
                    }
                    else
                    {
                        bw.Write(shift.GetBytes(name.Text).Take(MessageLength - 1).ToArray());
                        bw.Write((byte)0x0);
                    }
                }
        }
    }
}
