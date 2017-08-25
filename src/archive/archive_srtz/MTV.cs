using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kuriimu.Compression;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_srtz.MTV
{
    public class MTV
    {
        public List<MtvArchiveFileInfo> Files = new List<MtvArchiveFileInfo>();

        private Stream _stream = null;
        bool compressed = false;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["TIM2"] = ".tm2"
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
                    byte[] fileData;
                    int extraNameID = -1;

                    var magic = br.ReadString(4);

                    if (magic == "TIM2")
                    {
                        br.BaseStream.Position += 0xc;
                        var size = br.ReadInt32() + 0x10;
                        br.BaseStream.Position -= 0x14;
                        fileData = br.ReadBytes(size);
                    }
                    else if (magic == "SShd")
                    {
                        var origOffset = br.BaseStream.Position - 4;

                        //Header
                        var headerSize = br.ReadInt32();
                        br.BaseStream.Position += headerSize - 8;
                        br.BaseStream.Position = (br.BaseStream.Position + 15) & ~15;
                        headerSize = (headerSize + 15) & ~15;

                        //Body
                        br.ReadString(4);   //SSbd
                        var bodySize = br.ReadInt32();
                        br.BaseStream.Position += bodySize - 8;
                        br.BaseStream.Position = (br.BaseStream.Position + 16) & ~15;
                        bodySize = (bodySize + 16) & ~15;

                        br.BaseStream.Position = origOffset;
                        fileData = br.ReadBytes(headerSize + bodySize);
                    }
                    else
                    {
                        compressed = true;
                        br.BaseStream.Position -= 4;

                        var data = LZSSVLE.Decompress(br.BaseStream, true);

                        using (var br2 = new BinaryReaderX(new MemoryStream(data)))
                        {
                            var nr = br2.ReadUInt32();
                            if (nr > 0 && nr < 0x10)
                            {
                                br2.BaseStream.Position -= 4;
                                var header = new VarHeader(br2.BaseStream);

                                for (int i = 0; i < header.fileEntries - 1; i++)
                                {
                                    br2.BaseStream.Position = header.entries[i].offset;
                                    fileData = br2.ReadBytes(header.entries[i].size);

                                    var Afi = new MtvArchiveFileInfo
                                    {
                                        FileData = new MemoryStream(fileData),
                                        State = ArchiveFileState.Archived,
                                        id = i
                                    };
                                    Files.Add(Afi);

                                    // Filename
                                    var brP = new BinaryReaderX(Afi.FileData, true);
                                    var matchedP = brP.ReadString(4);
                                    var extensionP = _knownFiles.ContainsKey(matchedP) ? _knownFiles[matchedP] : ".bin";
                                    brP.BaseStream.Position = 0;
                                    Afi.FileName = index.ToString("000000") + "_" + i.ToString("00") + extensionP;
                                }

                                extraNameID = header.fileEntries - 1;
                                br2.BaseStream.Position = header.entries.Last().offset;
                                fileData = br2.ReadBytes(header.entries.Last().size);

                                List<byte> byteTmp = new List<byte>();
                                var bk = br.BaseStream.Position;
                                while (br.BaseStream.Position % 0x10 != 0) byteTmp.Add(br.ReadByte());
                                if (!byteTmp.ToArray().Equals(new byte[byteTmp.Count]))
                                    br.BaseStream.Position = bk;
                            }
                            else
                            {
                                fileData = data.ToArray();
                            }
                        }
                    }

                    var afi = new MtvArchiveFileInfo
                    {
                        FileData = new MemoryStream(fileData),
                        State = ArchiveFileState.Archived,
                        id = extraNameID
                    };
                    Files.Add(afi);

                    // Filename
                    var br3 = new BinaryReaderX(afi.FileData, true);
                    var matched = br3.ReadString(4);
                    var extension = _knownFiles.ContainsKey(matched) ? _knownFiles[matched] : ".bin";
                    br3.BaseStream.Position = 0;
                    string extra = (extraNameID > -1) ? "_" + extraNameID.ToString("00") : "";
                    afi.FileName = index.ToString("000000") + extra + extension;

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
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }
    }
}
