using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cetera.Image;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_cgrp
{
    public sealed class CGRP
    {
        public List<CGRPFileInfo> Files = new List<CGRPFileInfo>();
        Stream _stream = null;

        private Header header;
        private List<Info1> info1;
        private List<InfoEntry> infos;
        private byte[] infx;

        public CGRP(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //infoSec
                br.ReadStruct<PartitionHeader>();
                int fileCount = br.ReadInt32();
                info1=new List<Info1>();
                infos = new List<InfoEntry>();
                for (int i = 0; i < fileCount; i++)
                    info1.Add(br.ReadStruct<Info1>());
                for (int i = 0; i < fileCount; i++)
                    infos.Add(br.ReadStruct<InfoEntry>());

                //Files
                br.BaseStream.Position = header.dataOffset;
                br.ReadStruct<PartitionHeader>();
                for (int i = 0; i < fileCount; i++)
                    Files.Add(new CGRPFileInfo
                    {
                        State=ArchiveFileState.Archived,
                        FileName = "File "+i,
                        FileData = new SubStream(br.BaseStream,br.BaseStream.Position+infos[i].dataOffset, infos[i].dataSize)
                    });

                //Infx
                br.BaseStream.Position = header.infxOffset;
                infx = br.ReadBytes(header.infxSize);
            }
        }

        public void Save(Stream input)
        {
            using (BinaryWriterX bw = new BinaryWriterX(input))
            {
                int infoPartSize = 0x8 + 0x4 + info1.Count * 0x8 + infos.Count * 0x10;
                while (infoPartSize % 0x10 != 0) infoPartSize++;
                uint dataPartSize = 0x20;
                for (int i = 0; i < Files.Count; i++)
                {
                    dataPartSize += (uint) Files[i].FileSize.GetValueOrDefault();
                    while (dataPartSize % 0x10 != 0) dataPartSize++;
                }

                //INFO
                bw.BaseStream.Position = 0x40;
                bw.WriteASCII("INFO");
                bw.Write(infoPartSize);

                bw.Write(info1.Count);
                for (int i = 0; i < info1.Count; i++)
                    bw.WriteStruct(info1[i]);

                uint dataOffset = 0x18;
                for (int i = 0; i < infos.Count; i++)
                {
                    infos[i].dataOffset = dataOffset;
                    infos[i].dataSize = (uint)Files[i].FileSize.GetValueOrDefault();

                    bw.WriteStruct(infos[i]);

                    uint tmp= (uint)Files[i].FileSize.GetValueOrDefault();
                    while (tmp % 16 != 0) tmp++;
                    dataOffset += tmp;
                }

                //FILES
                bw.BaseStream.Position = header.dataOffset;

                uint dataSize = (uint)bw.BaseStream.Position;

                bw.WriteASCII("FILE");
                bw.Write(dataPartSize);

                bw.BaseStream.Position += 0x18;
                for (int i = 0; i < infos.Count; i++)
                {
                    bw.Write(new BinaryReaderX(Files[i].FileData,true).ReadBytes((int) Files[i].FileSize.GetValueOrDefault()));
                    while (bw.BaseStream.Position % 0x10 != 0) bw.BaseStream.Position++;
                }

                dataSize = (uint)bw.BaseStream.Position - dataSize;

                //INFX
                uint infxOffset = (uint)bw.BaseStream.Position;
                bw.Write(infx);

                //Header
                header.dataSize = dataSize;
                header.infxOffset = infxOffset;

                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
