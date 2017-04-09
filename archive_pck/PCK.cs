using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.IO;
using Kuriimu.Contract;

namespace archive_pck
{
    public sealed class PCK
    {
        public List<PckFileInfo> Files = new List<PckFileInfo>();

        List<PCKEntry> entries = new List<PCKEntry>();

        public PCK(Stream input)
        {
            using (BinaryReaderX pckBr = new BinaryReaderX(input, true))
            {
                int pckEntryCount = pckBr.ReadInt32();

                entries.AddRange(pckBr.ReadMultiple<PCKEntry>(pckEntryCount).OrderBy(e => e.fileOffset));

                for (int i = 0; i < entries.Count; i++)
                {
                    pckBr.BaseStream.Position = entries[i].fileOffset;
                    int blockOffset = (pckBr.ReadInt16() == 0x64) ? pckBr.ReadInt16() + 1 : 0;

                    Files.Add(new PckFileInfo()
                    {
                        FileData = new SubStream(
                            input,
                            entries[i].fileOffset + blockOffset * 4,
                            entries[i].fileLength - blockOffset * 4),
                        FileName = "File " + i,
                        State = ArchiveFileState.Archived,
                        Entry = entries[i]
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            using (BinaryWriterX pckBw = new BinaryWriterX(input))
            {
                pckBw.Write(Files.Count);

                //entryList + Data
                int dataPos = 4 + Files.Count * 0xc;
                for (int i = 0; i < Files.Count; i++)
                {
                    pckBw.Write(Files[i].Entry.crc32);
                    pckBw.Write(dataPos);

                    pckBw.BaseStream.Position = dataPos;
                    pckBw.Write(new byte[] { 0x64, 0, 0, 0 });
                    pckBw.Write(new BinaryReaderX(Files[i].FileData, true).ReadBytes((int)Files[i].FileData.Length));
                    pckBw.BaseStream.Position = 4 + i * 0xc + 8;

                    pckBw.Write((int)Files[i].FileData.Length + 4);
                    dataPos += (int)Files[i].FileData.Length + 4;
                }
            }
        }
    }
}
