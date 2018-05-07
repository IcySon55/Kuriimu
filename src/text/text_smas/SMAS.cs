using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.IO;

namespace text_smas
{
    public class SMAS
    {
        public Header Header = new Header();
        public List<Entry> Entries = new List<Entry>();
        public List<String> Strings = new List<String>();

        public SMAS(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                Header = br.ReadStruct<Header>();
                Entries = br.ReadMultiple<Entry>(Header.EntryCount);

                var strEntries = Entries.Where(e => e.Offset > 0).ToList();
                for (var i = 0; i < strEntries.Count; i++)
                {
                    var entry = strEntries[i];

                    if (entry.Offset > 0)
                    {
                        br.BaseStream.Position = entry.Offset;
                        string str;

                        var length = (i == strEntries.Count - 1 ? (int)(br.BaseStream.Length - br.BaseStream.Length % 2) : strEntries[i + 1].Offset) - entry.Offset;
                        if (entry.Label == "<label>")
                            str = br.ReadString(length);
                        else
                            str = br.ReadString(length, Encoding.Unicode);

                        Strings.Add(new String
                        {
                            Name = entry.Label,
                            Text = str,
                            Index = i
                        });
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            var headerSize = 0x10;
            var entrySize = 0x28;

            using (var bw = new BinaryWriterX(output))
            {
                bw.WriteStruct(Header);

                // Strings
                bw.BaseStream.Position = headerSize + Header.EntryCount * entrySize;
                var strEntries = Entries.Where(e => e.Offset > 0).ToList();

                var offset = (int)bw.BaseStream.Position;
                for (var i = 0; i < strEntries.Count; i++)
                {
                    var entry = strEntries[i];
                    if (entry.Label == "<label>")
                    {
                        bw.WriteASCII(Strings[i].Text);
                        bw.Write((byte)0x0);
                    }
                    else
                    {
                        bw.Write(Encoding.Unicode.GetBytes(Strings[i].Text));
                        bw.Write((byte)0x0);
                        bw.Write((byte)0x0);
                    }
                    bw.WriteAlignment(4);
                    entry.Offset = offset;
                    offset = (int)bw.BaseStream.Position;
                }

                // Odd extra bytes
                bw.Write(0);
                bw.Write((byte)0x0);

                // Entries
                bw.BaseStream.Position = headerSize;
                foreach (var entry in Entries)
                    bw.WriteStruct(entry);
            }
        }
    }
}
