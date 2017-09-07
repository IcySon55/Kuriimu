using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Kuriimu.Kontract;
using System.IO;
using Kuriimu.IO;

namespace archive_asg_fnt
{
    public class FNTFileInfo : ArchiveFileInfo
    {
        public int offset;

        public int Write(Stream input, int offset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                this.offset = offset;

                FileData.CopyTo(bw.BaseStream);

                return (offset + (int)FileSize + 0x7f) & ~0x7f;
            }
        }
    }
}
