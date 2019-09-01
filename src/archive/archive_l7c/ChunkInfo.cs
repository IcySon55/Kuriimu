using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archive_l7c
{
    class ChunkInfo
    {
        public long Offset { get; }
        public long Length { get; set; }
        public L7cDecoder Decoder { get; set; }

        public ChunkInfo(long offset, long length)
        {
            Offset = offset;
            Length = length;
        }
    }
}
