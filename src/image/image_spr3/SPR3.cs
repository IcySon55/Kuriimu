using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kontract.IO;
using System;
using image_nintendo.CTPK;

namespace image_spr3
{
    public sealed class SPR3
    {
        public Header header;
        public List<SPR3Entry> sSpr3 = new List<SPR3Entry>();

        public CTPK ctpk;

        public SPR3(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                header = br.ReadStruct<Header>();

                //dataOffset
                br.ReadUInt32();
                var dataOffset = br.ReadUInt32();

                //Entries
                var entries = br.ReadMultiple<Entry>(header.entryCount);

                //Entry data
                for (int i = 0; i < header.entryCount; i++)
                    sSpr3.Add(new SPR3Entry
                    {
                        entry = entries[i],
                        data = br.ReadBytes(0x80)
                    });

                //Load CTPK
                br.BaseStream.Position = dataOffset;
                ctpk = new CTPK(new MemoryStream(br.ReadBytes((int)(br.BaseStream.Length - dataOffset))));
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //Header
                bw.WriteStruct(header);
                bw.Write(0);
                var dataOffset = 0x28 + sSpr3.Count() * 8 + sSpr3.Count() * 0x80;
                bw.Write(dataOffset);

                //Entries
                foreach (var spr3 in sSpr3) bw.WriteStruct(spr3.entry);
                foreach (var spr3 in sSpr3) bw.Write(spr3.data);

                //ctpk data
                var save = new MemoryStream();
                ctpk.Save(save, true);
                save.Position = 0;

                save.CopyTo(bw.BaseStream);
            }
        }
    }
}
