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

        private Header header;
        private List<Info1> info1;
        private List<InfoEntry> infos;
        private byte[] infx;

        public CGRP(String filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
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

        /*public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {

            }
        }*/
    }
}
