using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_srtz.SEG
{
    public class SEG
    {
        public List<SegArchiveFileInfo> Files = new List<SegArchiveFileInfo>();

        private Stream _segStream = null;
        private Stream _binStream = null;
        private Stream _sizeStream = null;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["TIM2"] = ".tm2"
        };

        public SEG(Stream segInput, Stream binInput, Stream sizeInput = null)
        {
            _segStream = segInput;
            _binStream = binInput;
            _sizeStream = sizeInput;

            // Offsets and Sizes
            using (var br = new BinaryReaderX(segInput, true))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var afi = new SegArchiveFileInfo
                    {
                        Entry = new SegFileEntry { Offset = br.ReadUInt32() }
                    };
                    Files.Add(afi);

                    if (Files.IndexOf(afi) == 0) continue;

                    var prev = Files[Files.IndexOf(afi) - 1];
                    prev.Entry.Size = afi.Entry.Offset - prev.Entry.Offset;

                    if (afi.Entry.Offset == _binStream.Length) break;
                }
                Files.Remove(Files.Last());
            }

            // Uncompressed Sizes
            if (sizeInput != null)
                using (var br = new BinaryReaderX(sizeInput, true))
                    for (var i = 0; i < Files.Count; i++)
                        Files[i].Entry.UncompressedSize = br.ReadUInt32();

            // Files
            for (var i = 0; i < Files.Count; i++)
            {
                var substream = new SubStream(binInput, Files[i].Entry.Offset, Files[i].Entry.Size);
                var matched = new BinaryReaderX(substream, true).ReadString(4);
                var extension = _knownFiles.ContainsKey(matched) ? _knownFiles[matched] : ".bin";
                substream.Position = 0;

                var afi = Files[i];
                afi.FileName = i.ToString("000000") + extension;
                afi.FileData = substream;
                afi.State = ArchiveFileState.Archived;
            }
        }

        public void Save(Stream segOutput, Stream binOutput, Stream sizeOutput = null)
        {
            // Offsets and Sizes
            using (var bw = new BinaryWriterX(segOutput))
            {
                bw.Write(Files[0].Entry.Offset);

                uint runningTotal = 0;
                for (var i = 1; i < Files.Count; i++)
                {
                    Files[i].Entry.Offset = runningTotal + (uint)Files[i - 1].FileData.Length;
                    runningTotal += (uint)Files[i - 1].FileData.Length;
                    bw.Write(Files[i].Entry.Offset);
                }

                bw.Write(runningTotal + (uint)Files.Last().FileData.Length);
            }

            // Size
            if (sizeOutput != null)
                using (var bw = new BinaryWriterX(sizeOutput))
                {
                    foreach (var afi in Files)
                        bw.Write(afi.Entry.UncompressedSize);
                    bw.Write((uint)0);
                }

            // Files
            using (var bw = new BinaryWriterX(binOutput))
                foreach (var afi in Files)
                    afi.FileData.CopyTo(bw.BaseStream);
        }

        public void Close()
        {
            _segStream?.Dispose();
            _segStream = null;
            _binStream?.Dispose();
            _binStream = null;
            _sizeStream?.Dispose();
            _sizeStream = null;
        }
    }
}
