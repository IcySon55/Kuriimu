using System;
using System.Collections.Generic;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;
using System.Text;

namespace archive_umsbt
{
    public class UMSBT
    {
        public List<UmsbtFileInfo> Files = new List<UmsbtFileInfo>();
        private Stream _stream = null;

        public UMSBT(Stream input, bool plain=false)
        {
            _stream = input;
            if (plain) LoadPlainMSBT(input);
            else
            {
                using (var br = new BinaryReaderX(input, true))
                {
                    uint index = 0;
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        var info = new UmsbtFileInfo();
                        info.Entry = br.ReadStruct<UmsbtFileEntry>();
                        info.FileName = index.ToString("00000000") + ".msbt";
                        info.FileData = new SubStream(input, info.Entry.Offset, info.Entry.Size);
                        info.State = ArchiveFileState.Archived;

                        if (info.Entry.Offset == 0 && info.Entry.Size == 0)
                            break;
                        else
                            Files.Add(info);

                        index++;
                    }
                }
            }
        }

        public void LoadPlainMSBT(Stream input)
        {
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

                        Files.Add(new UmsbtFileInfo
                        {
                            State = ArchiveFileState.Archived,
                            FileName = name,
                            FileData = new SubStream(br.BaseStream, offset, size)
                        });
                    }
                }
            }
        }

        public void Save(Stream output, bool plain)
        {
            if (plain) SavePlainMSBT(output);
            else
            {
                using (var bw = new BinaryWriterX(output))
                {
                    uint padding = 24;
                    uint headerLength = ((uint)Files.Count) * (sizeof(uint) * 2) + padding;
                    uint runningTotal = 0;

                    foreach (var info in Files)
                    {
                        info.Entry.Offset = headerLength + runningTotal;
                        info.Entry.Size = (uint)info.FileData.Length;
                        runningTotal += info.Entry.Size;
                        bw.WriteStruct(info.Entry);
                    }

                    bw.Write(new byte[padding]);

                    foreach (var info in Files)
                        info.FileData.CopyTo(bw.BaseStream);
                }
            }
        }

        public void SavePlainMSBT(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                var dataOffset = Files.Count * 0x40 + 0x80;

                for (int i=0;i<Files.Count;i++)
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
                    while (dataOffset % 0x80 != 0) {
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
