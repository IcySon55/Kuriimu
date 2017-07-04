using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kuriimu.IO;

namespace text_bmd
{
    public class BMD
    {
        private const int SpeakerCount = 0x5D;
        public const int SpeakerLength = 0x20;
        public const int MessageLength = 0x30;

        public Header Header;
        public List<TableEntry> TableEntries;
        public List<DialogGroup> DialogGroups;

        public List<Speaker> Speakers = new List<Speaker>();
        public List<Message> Messages = new List<Message>();

        public BMD(Stream input)
        {
            using (var br = new BinaryReaderX(input, false))
            {
                Header = br.ReadStruct<Header>();
                TableEntries = br.ReadMultiple<TableEntry>(Header.NumberOfTableEntries);

                var totalGroups = TableEntries.Sum(te => te.GroupCount);

                DialogGroups = br.ReadMultiple<DialogGroup>(totalGroups);

                for (var i = 0; i < SpeakerCount; i++)
                    Speakers.Add(new Speaker
                    {
                        Name = (i + 1).ToString("0000"),
                        Text = br.ReadString(SpeakerLength, Encoding.Unicode)
                    });

                for (var i = 0; i < totalGroups; i++)
                    Messages.Add(new Message
                    {
                        Name = (i + 1).ToString("0000"),
                        Text = br.ReadString(MessageLength, Encoding.Unicode)
                    });
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                bw.WriteStruct(Header);

                foreach (var tableEntry in TableEntries)
                    bw.WriteStruct(tableEntry);

                foreach (var dialogGroup in DialogGroups)
                    bw.WriteStruct(dialogGroup);

                for (var i = 0; i < SpeakerCount; i++)
                {
                    var speakerName = Encoding.Unicode.GetBytes(Speakers[i].Text).Take(SpeakerLength - 2).ToArray();
                    bw.Write(speakerName);
                    for (var j = 0; j < SpeakerLength - speakerName.Length; j++)
                        bw.Write((byte)0x0);
                }

                for (var i = 0; i < DialogGroups.Count; i++)
                {
                    var message = Encoding.Unicode.GetBytes(Messages[i].Text).Take(MessageLength - 2).ToArray();
                    bw.Write(message);
                    for (var j = 0; j < MessageLength - message.Length; j++)
                        bw.Write((byte)0x0);
                }
            }
        }
    }
}
