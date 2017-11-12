using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.IO;
using System.IO;

namespace archive_skb
{
    public class SKBFileInfo : ArchiveFileInfo
    {

    }

    public class Entry
    {
        public uint offset = 0;
        public uint size = 0;
    }
}
