using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.IO;

namespace text_bmd
{
    public class BMD
    {
        private const int SpeakerNameCount = 0x5D;
        private const int SpeakerNameLength = 0x20;
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
                DialogGroups = br.ReadMultiple<DialogGroup>(TableEntries.Sum(te => te.GroupCount));

                for (var i = 0; i < SpeakerNameCount; i++)
                    Speakers.Add(new Speaker
                    {
                        Name = (i + 1).ToString("00"),
                        Text = br.ReadString(SpeakerNameLength, Encoding.Unicode)
                    });


                Messages = br.ReadMultiple<Message>(TableEntries.Sum(te => te.GroupCount));
            }
        }

        public void Save(Stream output)
        {

        }
    }
}
