using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.IO;
using Kontract.Interface;
using System.IO;
using System.Collections;

namespace archive_rdp
{
    public class RESRDP
    {
        public List<RdpFileInfo> Files = new List<RdpFileInfo>();
        Stream _stream;

        public RESRDP(Stream res, Stream rdp)
        {
            _stream = rdp;

            //Read RES
            ReadRES(res);
        }

        (List<ResSubEntry>,List<string>) ReadRES(Stream res)
        {
            using (var br = new BinaryReaderX(res, true))
            {
                //RES Header
                var resHeader = br.ReadStruct<ResHeader>();
                br.BaseStream.Position = (br.BaseStream.Position + 0xf) & ~0xf;

                //RES Entries
                var upperEntries = br.ReadMultiple<ResEntry>(resHeader.entryCount);
                br.BaseStream.Position = (br.BaseStream.Position + 0xf) & ~0xf;
                var upperEntries2 = br.ReadMultiple<ResEntry>(resHeader.entryCount);

                var subEntryList = new List<ResSubEntry>();
                foreach (var entry in upperEntries)
                {
                    br.BaseStream.Position = entry.offset;
                    subEntryList.AddRange(br.ReadMultiple<ResSubEntry>(entry.entryCount));
                }
                foreach (var entry in upperEntries2)
                {
                    br.BaseStream.Position = entry.offset;
                    subEntryList.AddRange(br.ReadMultiple<ResSubEntry>(entry.entryCount));
                }

                return subEntryList;
            }
        }

        public void Save(Stream output)
        {

        }

        public void Close()
        {
            _stream?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }
    }
}
