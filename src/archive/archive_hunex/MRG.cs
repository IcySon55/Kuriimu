using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_hunex
{
    public class MRG
    {
        private Stream _mrgStream = null;

        public List<MRGFileInfo> Files = new List<MRGFileInfo>();
        private List<MRGEntry> Entries = new List<MRGEntry>();

        private List<string> fileNameList = new List<string>();

        public MRG(Stream mrgStream, string baseTerm)
        {
            _mrgStream = mrgStream;

            using (var br = new BinaryReaderX(mrgStream, true))
            {
                var magic = br.ReadString(6);
                var numberofEntries = br.ReadUInt16();

                for (int i = 0; i < numberofEntries; ++i)
                {
                    var sectorOffset = br.ReadUInt16();
                    var offset = br.ReadUInt16();
                    var sectorSizeUpper = br.ReadUInt16();
                    var size = br.ReadUInt16();

                    var entry = new MRGEntry(sectorOffset, offset, sectorSizeUpper, size, numberofEntries);
                    Entries.Add(entry);
                }

                fileNameList.Add(baseTerm + ".nam");

                // These seem to always be the first 2 entries regardless of what internal .NAM says
                fileNameList.Add("unknown1.mrg");
                fileNameList.Add("unknown2.mrg");

                for (int i = 0; i < numberofEntries; ++i)
                {
                    string fileName = "unknown" + i.ToString();

                    if (i * 32 < Entries[0].Size)
                    {
                        byte[] bstr = br.ReadBytes(32);

                        // Seems random \x01 bytes can be present in string
                        bstr = bstr.Where(x => x != 0x1).ToArray();

                        // Get String from bytes as CP-932 and remove line ends and trailing null bytes
                        string str = Encoding.GetEncoding(932).GetString(bstr).Replace("\r\n", "").TrimEnd('\x00');

                        if (!String.IsNullOrEmpty(str))
                            fileName = str;
                    }

                    fileNameList.Add(fileName + ".mzx");
                }

                for (int i = 0; i < numberofEntries; ++i)
                {
                    Entries[i].Name = fileNameList[i];
                }

                Files = Entries.Select(o => new MRGFileInfo
                {
                    Entry = o,
                    FileName = o.Name,
                    FileData = new SubStream(mrgStream, o.Offset, o.Size),
                    State = ArchiveFileState.Archived
                }).ToList();
            }
        }

        public void Close()
        {
            _mrgStream?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _mrgStream = null;
        }
    }
}
