using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.NARC
{
    public sealed class NARC
    {
        public List<NARCFileInfo> Files = new List<NARCFileInfo>();
        Stream _stream = null;

        byte[] nameP;

        GenericHeader genHeader;
        FATHeader fatHeader;
        FNTHeader fntHeader;
        FIMGHeader gmifHeader;

        public NARC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                genHeader = br.ReadStruct<GenericHeader>();

                //FAT
                fatHeader = br.ReadStruct<FATHeader>();
                var entries = br.ReadMultiple<FATEntry>((int)fatHeader.fileCount);

                //FNT
                fntHeader = br.ReadStruct<FNTHeader>();
                nameP = br.ReadBytes((int)(fntHeader.secSize - 8));
                var names = GetNames(new MemoryStream(nameP), fntHeader);

                //GMIF - FileData
                gmifHeader = br.ReadStruct<FIMGHeader>();
                var dataOffset = br.BaseStream.Position;
                for (int i = 0; i < fatHeader.fileCount; i++)
                    Files.Add(new NARCFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = names[i],
                        FileData = new SubStream(br.BaseStream, dataOffset + entries[i].startOffset, entries[i].endOffset - entries[i].startOffset),
                        entry = entries[i]
                    });

            }
        }

        public List<string> GetNames(Stream input, FNTHeader fntHeader)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var firstDirecOffset = br.ReadUInt32();
                var firstFilePos = br.ReadUInt16();
                var direcCount = br.ReadByte();
                var parentDir = br.ReadByte();

                //need samples with multi directories
                if (direcCount > 1) throw new System.Exception("Seems like the archive has more than one directory. Create an issue on our github with this errored file as sample!");

                var names = new List<string>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var id = br.ReadByte();
                    if ((id & 0x80) == 1) throw new System.Exception("Seems like the archive has more than one directory. Create an issue on our github with this errored file as sample!");

                    var stringLength = id & 0x7f;
                    names.Add(br.ReadString(stringLength));
                }

                return names;
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                uint dataOffset = (uint)(0x10 + 0xc + Files.Count * 0x8 + 0x8 + nameP.Length + 0x8);

                //Files
                uint offset = 0;
                var data = new MemoryStream();
                foreach (var file in Files)
                {
                    file.FileData.CopyTo(data);
                    using (var dataB = new BinaryWriterX(data, true)) dataB.WriteAlignment(0x80, 0xff);

                    file.entry.startOffset = offset;
                    offset += (uint)file.FileSize;
                    file.entry.endOffset = offset;
                    offset = (uint)((offset + 0x7f) & ~0x7f);
                }
                bw.BaseStream.Position = dataOffset;
                data.Position = 0;
                data.CopyTo(bw.BaseStream);

                //FAT
                bw.BaseStream.Position = 0x10;
                bw.WriteStruct(fatHeader);
                foreach (var file in Files)
                {
                    bw.WriteStruct(file.entry);
                }

                //FNT
                bw.WriteStruct(fntHeader);
                bw.Write(nameP);

                //GMIF
                gmifHeader.secSize = (uint)(bw.BaseStream.Length - dataOffset + 8);
                bw.WriteStruct(gmifHeader);

                //GenHeader
                bw.BaseStream.Position = 0;
                genHeader.secSize = (uint)(bw.BaseStream.Length);
                bw.WriteStruct(genHeader);
            }
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
