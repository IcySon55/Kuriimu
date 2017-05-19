using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_umsbt
{
    public class PlainUMSBT
    {
        public List<PlainUmsbtFileInfo> Files = new List<PlainUmsbtFileInfo>();
        private Stream _stream = null;

        public PlainUMSBT(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = 0x38;
                var dataOffset = br.ReadUInt32();
                br.BaseStream.Position = 0;

                while (br.BaseStream.Position < dataOffset)
                {
                    string name = br.ReadCStringSJIS();
                    if (name != "")
                    {
                        while (br.BaseStream.Position % 0x40 != 0) br.BaseStream.Position++;
                        br.BaseStream.Position -= 8;

                        uint offset = br.ReadUInt32();
                        uint size = br.ReadUInt32();

                        Files.Add(new PlainUmsbtFileInfo
                        {
                            FileName = name,
                            FileData = new SubStream(br.BaseStream, offset, size),
                            State = ArchiveFileState.Archived
                        });
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = Files.Count * 0x40 + 0x80;

                for (int i = 0; i < Files.Count; i++)
                {
                    //write entry
                    bw.Write(Encoding.GetEncoding("SJIS").GetBytes(Files[i].FileName));
                    while (bw.BaseStream.Position % 0x40 != 0) bw.BaseStream.Position++;
                    bw.BaseStream.Position -= 8;
                    bw.Write(dataOffset);
                    bw.Write((uint)Files[i].FileSize);

                    //write FileData
                    long bk = bw.BaseStream.Position;
                    bw.BaseStream.Position = dataOffset;
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    dataOffset += (int)Files[i].FileSize;

                    //make to padding to 0x80
                    while (dataOffset % 0x80 != 0)
                    {
                        bw.Write((byte)0);
                        dataOffset++;
                    }

                    bw.BaseStream.Position = bk;
                }
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
