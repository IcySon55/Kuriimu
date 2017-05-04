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

                var tableEntryTotal = TableEntries.Sum(te => te.GroupCount);

                DialogGroups = br.ReadMultiple<DialogGroup>(tableEntryTotal);

                for (var i = 0; i < SpeakerCount; i++)
                    Speakers.Add(new Speaker
                    {
                        Name = (i + 1).ToString("00"),
                        Text = br.ReadString(SpeakerLength, Encoding.Unicode)
                    });

                for (var i = 0; i < tableEntryTotal; i++)
                    Messages.Add(new Message
                    {
                        Name = (i + 1).ToString("00"),
                        Text = br.ReadString(MessageLength, Encoding.Unicode)
                    });
            }
        }

        public void Save(Stream output)
        {

        }
    }
}
