using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_srtz.MTV
{
    public class MTV
    {
        public List<MtvArchiveFileInfo> Files = new List<MtvArchiveFileInfo>();

        private Stream _stream = null;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["TIM2"] = ".tim2"
        };

        public MTV(Stream input)
        {
            _stream = input;

            // Offsets and Sizes
            using (var br = new BinaryReaderX(input, true))
            {
                int index = 0;
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var data = LZSSVLE.Decompress(br.BaseStream, true);
                    var header = data.Take(0x20).ToArray();
                    var fileData = data.Skip(0x20).ToArray();

                    var afi = new MtvArchiveFileInfo
                    {
                        FileData = new MemoryStream(fileData),
                        State = ArchiveFileState.Archived
                    };
                    Files.Add(afi);
                    using (var br2 = new BinaryReaderX(new MemoryStream(header)))
                        afi.Header = br2.ReadStruct<CompressionHeader>();

                    // Filename
                    var br3 = new BinaryReaderX(afi.FileData, true);
                    var matched = br3.ReadString(4);
                    var extension = _knownFiles.ContainsKey(matched) ? _knownFiles[matched] : ".bin";
                    br3.BaseStream.Position = 0;
                    afi.FileName = index.ToString("000000") + extension;

                    br.BaseStream.Position = (br.BaseStream.Position + 15) & ~15;
                    index++;
                }
            }
        }

        public void Save(Stream output)
        {
            return;
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
