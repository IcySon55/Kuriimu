using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;
using Kontract.Interface;

namespace archive_nintendo.GARC2
{
    public sealed class GARC2
    {
        public List<GARC2FileInfo> Files = new List<GARC2FileInfo>();
        Stream _stream = null;

        Header header;

        FatoHeader fatoHeader;
        List<uint> fatoOffsets;

        FatbHeader fatbHeader;
        List<FatbEntry> fatbEntries;

        FimbHeader fimbHeader;

        public GARC2(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //FATO
                fatoHeader = br.ReadStruct<FatoHeader>();
                br.BaseStream.Position = br.BaseStream.Position + 3 & ~3;
                fatoOffsets = br.ReadMultiple<uint>(fatoHeader.entryCount);

                //FATB
                fatbHeader = br.ReadStruct<FatbHeader>();
                fatbEntries = br.ReadMultiple<FatbEntry>((int)fatbHeader.entryCount);

                //FIMB
                fimbHeader = br.ReadStruct<FimbHeader>();

                for (int i = 0; i < fatbHeader.entryCount; i++)
                {
                    br.BaseStream.Position = fatbEntries[i].offset + header.dataOffset;

                    var size = fatbEntries[i].endOffset - fatbEntries[i].offset;
                    Files.Add(new GARC2FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}.bin",
                        FileData = new SubStream(br.BaseStream, header.dataOffset + fatbEntries[i].offset, size)
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                //filesize
                bw.BaseStream.Position = 0x18;

                //FATO
                bw.WriteStruct(fatoHeader);
                bw.WriteAlignment(4, 0xff);
                for (int i = 0; i < fatoOffsets.Count; i++) bw.Write(fatoOffsets[i]);

                //FATB
                bw.WriteStruct(fatbHeader);
                uint offset = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    fatbEntries[i].offset = offset;
                    offset += (uint)Files[i].FileSize;
                    offset = (uint)(offset + 3 & ~3);
                    fatbEntries[i].endOffset = offset;

                    bw.WriteStruct(fatbEntries[i]);
                }

                var fimbOffset = bw.BaseStream.Position;
                bw.BaseStream.Position += 0xc;

                //Writing FileData
                var dataOffset = bw.BaseStream.Position;
                for (int i = 0; i < Files.Count; i++)
                {
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(4, 0xff);
                }

                //FIMB
                fimbHeader.dataSize = (uint)bw.BaseStream.Length - (uint)dataOffset;
                bw.BaseStream.Position = fimbOffset;
                bw.WriteStruct(fimbHeader);

                //Header
                header.fileSize = (uint)bw.BaseStream.Length;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
