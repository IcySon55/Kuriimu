using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using archive_l7c.Compression;

namespace archive_l7c
{
    public class ChunkInfo
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
