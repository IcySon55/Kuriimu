﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;
using Kontract.Interface;

namespace archive_nintendo.GARC4
{
    public sealed class GARC4
    {
        public List<GARC4FileInfo> Files = new List<GARC4FileInfo>();
        Stream _stream = null;

        private static Dictionary<string, string> _knownFiles = new Dictionary<string, string>
        {
            ["BCH"] = ".bch",
            ["PC"] = ".pc",
            ["SB"] = ".sb",
            ["CGFX"] = ".cgfx",
        };

        Header header;

        FatoHeader fatoHeader;
        List<uint> fatoOffsets;

        FatbHeader fatbHeader;
        List<FatbEntry> fatbEntries;

        FimbHeader fimbHeader;

        public GARC4(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //FATO
                fatoHeader = br.ReadStruct<FatoHeader>();
                br.BaseStream.Position = br.BaseStream.Position + 3 & ~3;
                fatoOffsets = br.ReadMultiple<uint>(fatoHeader.entryCount);

                //FATB
                fatbHeader = br.ReadStruct<FatbHeader>();
                fatbEntries = br.ReadMultiple<FatbEntry>((int)fatbHeader.entryCount);

                //FIMB
                fimbHeader = br.ReadStruct<FimbHeader>();

                for (int i = 0; i < fatbHeader.entryCount; i++)
                {
                    br.BaseStream.Position = fatbEntries[i].offset + header.dataOffset;
                    var mag = br.ReadByte();
                    var extension = (mag == 0x11) ? ".lz11" : "";
                    if (extension == "")
                    {
                        br.BaseStream.Position--;
                        var magS = br.ReadString(2);
                        extension = _knownFiles.ContainsKey(magS) ? _knownFiles[magS] : ".bin";

                        if (extension == ".bin")
                        {
                            br.BaseStream.Position -= 2;
                            magS = br.ReadString(4);
                            extension = _knownFiles.ContainsKey(magS) ? _knownFiles[magS] : ".bin";
                        }
                    }

                    var size = (fatbEntries[i].size == 0) ? fatbEntries[i].endOffset - fatbEntries[i].offset : fatbEntries[i].size;
                    Files.Add(new GARC4FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = $"{i:00000000}" + extension,
                        FileData = new SubStream(br.BaseStream, fatbEntries[i].offset + header.dataOffset, size)
                    });
                }
            }
        }

        public void Save(Stream input)
        {
            using (var bw = new BinaryWriterX(input))
            {
                //filesize
                //largestFilesize
                bw.BaseStream.Position = 0x1c;

                //FATO
                bw.WriteStruct(fatoHeader);
                bw.Write((ushort)0xFFFF);
                for (int i = 0; i < fatoOffsets.Count; i++) bw.Write(fatoOffsets[i]);

                //FATB
                bw.WriteStruct(fatbHeader);
                uint offset = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    fatbEntries[i].offset = offset;
                    offset += (uint)Files[i].FileSize;
                    offset = (uint)(offset + 3 & ~3);
                    fatbEntries[i].endOffset = offset;
                    fatbEntries[i].size = (uint)Files[i].FileSize;

                    bw.WriteStruct(fatbEntries[i]);
                }

                var fimbOffset = bw.BaseStream.Position;
                bw.BaseStream.Position += 0xc;
                var dataOffset = bw.BaseStream.Position;

                //Writing FileData
                uint largestFileSize = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    if (Files[i].FileSize > largestFileSize) largestFileSize = (uint)Files[i].FileSize;
                    Files[i].FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(4, 0xff);
                }

                //FIMB
                fimbHeader.dataSize = (uint)bw.BaseStream.Length - (uint)dataOffset;
                bw.BaseStream.Position = fimbOffset;
                bw.WriteStruct(fimbHeader);

                //Header
                header.fileSize = (uint)bw.BaseStream.Length;
                header.largestFileSize = largestFileSize;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
