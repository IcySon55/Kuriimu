using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

namespace archive_level5.ARC0
{
    public sealed class ARC0
    {
        public List<ARC0FileInfo> Files = new List<ARC0FileInfo>();
        Stream _stream = null;
        private Import imports = new Import();

        public Header header;
        public byte[] table1;
        public byte[] table2;
        public byte[] nameC;
        List<FileEntry> entries = new List<FileEntry>();
        List<string> fileNames = new List<string>();

        public ARC0(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Table 1
                br.BaseStream.Position = header.offset1;
                table1 = br.ReadBytes((int)(header.offset2 - header.offset1));

                //Table 2
                br.BaseStream.Position = header.offset2;
                table2 = br.ReadBytes((int)(header.fileEntriesOffset - header.offset2));

                //File Entry Table
                br.BaseStream.Position = header.fileEntriesOffset;
                entries = new BinaryReaderX(new MemoryStream(imports.level5.Decompress(new MemoryStream(br.ReadBytes((int)(header.nameOffset - header.fileEntriesOffset))), 0)))
                  .ReadMultiple<FileEntry>(header.fileEntriesCount);

                //NameTable
                br.BaseStream.Position = header.nameOffset;
                nameC = br.ReadBytes((int)(header.dataOffset - header.nameOffset));
                fileNames = GetFileNames(imports.level5.Decompress(new MemoryStream(nameC), 0));

                //Add Files
                List<uint> offsets = new List<uint>();
                uint GetInt(byte[] ba) => ba.Aggregate(0u, (i, b) => (i << 8) | b);
                foreach (var name in fileNames)
                {
                    var crc32 = imports.crc32.Create(Encoding.GetEncoding("SJIS").GetBytes(name.Split('/').Last()), 0);
                    var entry = entries.Find(c => c.crc32 == GetInt(crc32) && !offsets.Contains(c.fileOffset));
                    offsets.Add(entry.fileOffset);
                    Files.Add(new ARC0FileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = name,
                        FileData = new SubStream(br.BaseStream, header.dataOffset + entry.fileOffset, entry.fileSize),
                        entry = entry
                    });
                }
            }
        }

        public List<string> GetFileNames(byte[] namePart)
        {
            List<string> names = new List<string>();

            using (var br = new BinaryReaderX(new MemoryStream(namePart)))
            {
                string currentDir = "";
                br.BaseStream.Position = 1;

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    string tmpString = br.ReadCStringSJIS();
                    if (tmpString[tmpString.Length - 1] == '/')
                    {
                        currentDir = tmpString;
                    }
                    else
                    {
                        names.Add(currentDir + tmpString);
                    }
                }
            }

            return names;
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                int dataOffset = ((0x48 + table1.Length + table2.Length + nameC.Length + entries.Count * 0x10 + 4) + 0x4) & ~0x4;

                //Table 1 & 2
                bw.BaseStream.Position = 0x48;
                bw.Write(table1);
                bw.Write(table2);

                //FileEntries Table
                bw.Write((entries.Count * 0x10) << 3);

                uint offset = 0;
                List<ARC0FileInfo> files = new List<ARC0FileInfo>();
                foreach (var entry in entries)
                {
                    var file = Files.Find(c => c.entry.fileOffset == entry.fileOffset);
                    files.Add(file);

                    //catch file limits
                    if (file.FileData.Length > 0xffffffff)
                    {
                        throw new Exception("File " + file.FileName + " is too big to pack into this archive type!");
                    }
                    else if (offset + dataOffset > 0xffffffff)
                    {
                        throw new Exception("The archive can't be bigger than 0xffffffff Bytes.");
                    }

                    //edit entry
                    entry.fileOffset = offset;
                    entry.fileSize = (uint)file.FileData.Length;

                    //write entry
                    bw.WriteStruct(entry);

                    //edit values
                    offset = (uint)(((offset + file.FileData.Length) + 0x4) & ~0x4);
                }

                //Nametable
                bw.Write(nameC);

                //Files
                bw.BaseStream.Position = dataOffset;
                foreach (var file in files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                    bw.BaseStream.Position = (bw.BaseStream.Position + 0x4) & ~0x4;
                }

                //Header
                header.nameOffset = (uint)(0x48 + table1.Length + table2.Length + entries.Count * 0x10 + 4);
                header.dataOffset = (uint)dataOffset;
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
