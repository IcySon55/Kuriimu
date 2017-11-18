using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_hunex
{
    public class HED
    {
        public bool hasNameFile = false;
        public bool isEncrypted = false;

        private Stream _hedStream = null;
        private Stream _namStream = null;
        private Stream _mrgStream = null;
        private Stream _cpkStream = null;

        private NAM nam = null;

        public List<HEDFileInfo> Files = new List<HEDFileInfo>();
        private List<HEDFileEntry> Entries = new List<HEDFileEntry>();

        public HED(Stream hedStream, Stream namStream, Stream mrgStream, Stream cpkStream, string baseTerm)
        {
            _hedStream = hedStream;
            _mrgStream = mrgStream;

            if (namStream != null)
            {
                _namStream = namStream;
                nam = new NAM(namStream, baseTerm);
                hasNameFile = true;
            }

            if (cpkStream != null)
            {
                _cpkStream = cpkStream;
                isEncrypted = true;
            }

            using (var hed = new BinaryReaderX(hedStream, true))
            {
                ushort unk1 = hed.ReadUInt16();
                ushort firstEntryHigh = hed.ReadUInt16();

                var entryLength = 8;
                if ((firstEntryHigh & 0x0FFF) != 0)
                {
                    entryLength = 4;
                }

                var recordCount = hed.BaseStream.Length / entryLength;

                // Jump back to the start of the stream after assertaining our entry length
                hed.BaseStream.Position = 0;

                for (int i = 0; i < recordCount; ++i)
                {
                    byte[] blob = hed.ReadBytes(entryLength);
                    using (var ms = new MemoryStream(blob))
                    using (var br = new BinaryReaderX(ms, true))
                    {
                        // No entry may contain all 0xFF bytes.
                        // This is due to the EOF marker for HED files being 16 0xFF bytes.
                        var firstWord = br.ReadUInt32();
                        if (firstWord == 0xFFFFFFFF)
                            continue;

                        string namFileName = "";
                        string suffix = i.ToString();
                        if (nam != null)
                            namFileName = nam.GetName(i);

                        br.BaseStream.Position = 0;

                        HEDFileEntry entry = new HEDFileEntry(br, namFileName, suffix);

                        Entries.Add(entry);
                    }
                }

                Files = Entries.Select(o => new HEDFileInfo
                {
                    Entry = o,
                    FileName = String.Format("{0}-{1}{2}", o.Name, o.Suffix, o.Extension),
                    FileData = new SubStream(mrgStream, o.Offset, o.Size),
                    State = ArchiveFileState.Archived
                }).ToList();
            }
        }

        public void Close()
        {
            _hedStream?.Dispose();
            _hedStream = null;

            _mrgStream?.Dispose();
            _mrgStream = null;

            _namStream?.Dispose();
            _namStream = null;

            _cpkStream?.Dispose();
            _cpkStream = null;

            nam?.Close();
            nam = null;

            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
        }
    }
}
