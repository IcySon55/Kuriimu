using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.IO;

namespace text_srtz
{
    public class SRVC
    {
        public const int HashTableLength = 0x138;

        public Header Header = null;
        private byte[] HashTable = null;
        private byte[] Block = null;
        private List<EntryData> Metas = new List<EntryData>();
        public List<Entry> Entries = new List<Entry>();

        private Encoding shift = Encoding.GetEncoding("shift-jis");

        public SRVC(Stream input)
        {
            using (var br = new BinaryReaderX(input, false))
            {
                Header = br.ReadStruct<Header>();
                HashTable = br.ReadBytes(HashTableLength);
                int blockSize = Header.AugmentA * 8 + Header.AugmentB * 4 + Header.AugmentC * 8 + 4;
                Block = br.ReadBytes(blockSize);

                Metas = br.ReadMultiple<EntryData>(Header.StringCount);

                for (int i = 0; i < Header.StringCount; i++)
                {
                    var bytes = new List<byte>();
                    byte current = 0xFF;
                    while (current != 0x0)
                    {
                        current = br.ReadByte();
                        if (current != 0x0)
                            bytes.Add(current);
                    }
                    Entries.Add(new Entry
                    {
                        ID = Metas[i].ID,
                        Text = shift.GetString(bytes.ToArray())
                    });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                ;
            }
        }
    }
}
