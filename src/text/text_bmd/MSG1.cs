using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace text_bmd.msg1
{
    public sealed class MSG1
    {
        public List<Label> Labels = new List<Label>();

        Header header;
        TextHeader textHeader;

        public MSG1(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Entries
                var entries = br.ReadMultiple<EntryTableEntry>(header.entryCount);

                //TextHeader
                textHeader = br.ReadStruct<TextHeader>();

                //Labels

            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {

            }
        }
    }
}
