using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_spc
{
    public class SPC
    {
        public List<SpcFileInfo> Files = new List<SpcFileInfo>();
        Stream _stream;

        public SPC(Stream input)
        {

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
