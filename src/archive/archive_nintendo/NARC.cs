using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_nintendo.NARC
{
    public sealed class NARC
    {
        public List<NARCFileInfo> Files = new List<NARCFileInfo>();
        Stream _stream = null;

        public NARC(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                var genHeader = br.ReadStruct<GenericHeader>();

                //FAT
                var fatHeader = br.ReadStruct<FATHeader>();
                var entries = br.ReadMultiple<FATEntry>((int)fatHeader.fileCount);

                //FNT
                var fntHeader = br.ReadStruct<FNTHeader>();
                var names = GetNames(new MemoryStream(br.ReadBytes((int)(fntHeader.secSize - 8))), fntHeader);

                //GMIF - FileData
                var gmifHeader = br.ReadStruct<FIMGHeader>();
                var dataOffset = br.BaseStream.Position;
                for (int i = 0; i < fatHeader.fileCount; i++)
                    Files.Add(new NARCFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = names[i],
                        FileData = new SubStream(br.BaseStream, dataOffset + entries[i].startOffset, entries[i].endOffset - entries[i].startOffset)
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
