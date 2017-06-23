using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.IO;

namespace text_metal
{
    public sealed class ARRNAM
    {
        public List<Entry> Entries = new List<Entry>();

        private List<ArrEntry> ArrEntries = new List<ArrEntry>();

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
                    var arrEntry = ArrEntries[i];
                    br.BaseStream.Position = arrEntry.Offset;

                    var chars = br.ReadBytes(2);
                    var text = Encoding.Unicode.GetString(chars);
                    while (!chars.SequenceEqual(new byte[] { 0, 0 }))
                    {
                        chars = br.ReadBytes(2);
                        text += Encoding.Unicode.GetString(chars);

                        if (chars.SequenceEqual(new byte[] { 0, 0 }))
                            break;
                    }

                    var entry = new Entry
                    {
                        ArrEntry = arrEntry,
                        Index = i + 1,
                        Text = text.TrimEnd('\0')
                    };

                    Entries.Add(entry);
                }
            }
        }

        public void Save(Stream namOutput, Stream arrOutput)
        {
            using (var bw = new BinaryWriterX(namOutput))
                foreach (var entry in Entries)
                {
                    var arrEntry = entry.ArrEntry;
                    arrEntry.Offset = (short)bw.BaseStream.Position;

                    bw.Write(Encoding.Unicode.GetBytes(entry.Text));
                    bw.Write(new byte[] { 0, 0 });
                }

            using (var bw = new BinaryWriterX(arrOutput))
                foreach (var entry in Entries)
                    bw.WriteStruct(entry.ArrEntry);
        }
    }
}