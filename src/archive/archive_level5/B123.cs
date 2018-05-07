using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cetera.Hash;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.B123
{
    public sealed class B123
    {
        public List<B123FileInfo> Files = new List<B123FileInfo>();
        Stream _stream = null;

        public Header header;

        private List<T0Entry> table0;
        private List<uint> table1;
        private List<FileEntry> entries;
        public byte[] nameBody;

        public B123(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Table 0
                //Internally sorted by dirNameHash
                br.BaseStream.Position = header.table0Offset;
                table0 = br.ReadMultiple<T0Entry>(header.table0Count);

                //Table 1
                br.BaseStream.Position = header.table1Offset;
                table1 = br.ReadMultiple<uint>(header.table1Count);

                //File Entry Table
                br.BaseStream.Position = header.fileEntriesOffset;
                entries = br.ReadMultiple<FileEntry>(header.fileEntriesCount);

                //NameTable
                br.BaseStream.Position = header.nameOffset;
                nameBody = br.ReadBytes((int)(header.dataOffset - header.nameOffset));

                //Add Files
                using (var stringTable = new BinaryReaderX(new MemoryStream(nameBody)))
                {
                    foreach (var dir in table0)
                    {
                        stringTable.BaseStream.Position = dir.dirNameOffset;
                        var dirName = stringTable.ReadCStringSJIS();

                        var fileCountInDir = 0;
                        foreach (var file in entries.Where((f, i) => i >= dir.fileEntryOffset && i < dir.fileEntryOffset + dir.fileCountInDir))
                        {
                            stringTable.BaseStream.Position = dir.firstFileNameOffset + file.nameOffsetInFolder;
                            var fileName = stringTable.ReadCStringSJIS();
                            Files.Add(new B123FileInfo
                            {
                                State = ArchiveFileState.Archived,
                                FileName = Path.Combine(dirName, fileName),
                                FileData = new SubStream(br.BaseStream, header.dataOffset + file.fileOffset, file.fileSize),
                                dirEntry = dir,
                                fileCountInDir = fileCountInDir++,
                                fileEntry = file
                            });
                        }
                    }
                }
            }
        }

        public void Save(Stream output)
        {
            //Update FileInfo
            uint offset = 0;
            foreach (var file in Files)
            {
                entries[(int)(file.dirEntry.fileEntryOffset + file.fileCountInDir)].fileOffset = offset;
                file.fileEntry.fileOffset = offset;

                entries[(int)(file.dirEntry.fileEntryOffset + file.fileCountInDir)].fileSize = (uint)file.FileSize;
                file.fileEntry.fileSize = (uint)file.FileSize;

                offset = (uint)((offset + file.FileSize + 3) & ~3);
            }

            using (var bw = new BinaryWriterX(output))
            {
                bw.BaseStream.Position = 0x48;

                //Table 0
                header.table0Offset = (uint)bw.BaseStream.Position;
                header.table0Count = (short)table0.Count;
                bw.WriteMultiple(table0);

                //Table 1
                header.table1Offset = (uint)bw.BaseStream.Position;
                header.table1Count = (short)table1.Count;
                bw.WriteMultiple(table1);

                //FileEntries
                header.fileEntriesOffset = (uint)bw.BaseStream.Position;
                header.fileEntriesCount = entries.Count;
                bw.WriteMultiple(entries);

                //StringTable
                header.nameOffset = (uint)bw.BaseStream.Position;
                bw.Write(nameBody);

                //FileData
                var dataOffset = ((0x48 + table0.Count * 0x18 + table1.Count * 4 + ((nameBody.Length + 0x3) & ~0x3) + entries.Count * 0x10) + 0x3) & ~0x3;
                header.dataOffset = (uint)dataOffset;
                foreach (var file in Files)
                {
                    bw.BaseStream.Position = dataOffset + file.fileEntry.fileOffset;
                    file.FileData.CopyTo(bw.BaseStream);
                }

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);

                /*int dataOffset = ((0x48 + table0.Count * 0x18 + table1.Count * 4 + nameBody.Length + entries.Count * 0x10 + 4) + 0x3) & ~0x3;

                //Table 0 & 1
                bw.BaseStream.Position = 0x48;
                bw.WriteMultiple(table0);
                bw.WriteMultiple(table1);

                //FileEntries Table
                //bw.Write((entries.Count * 0x10) << 3);

                uint offset = 0;
                List<B123FileInfo> files = new List<B123FileInfo>();
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
                bw.Write(nameBody);

                //Files
                bw.BaseStream.Position = dataOffset;
                foreach (var file in files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                    bw.BaseStream.Position = (bw.BaseStream.Position + 0x4) & ~0x4;
                }*/
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }

    public static class Extensions
    {
        public static void WriteMultiple<T>(this BinaryWriterX bw, IEnumerable<T> list)
        {
            if (list.Count() <= 0)
            {
                bw.Write(0);
                return;
            }

            var ms = new MemoryStream();
            using (var bwIntern = new BinaryWriterX(ms, true))
                foreach (var t in list)
                    bwIntern.WriteStruct(t);
            bw.Write(ms.ToArray());
        }

        /*public static void WriteStringsCompressed(this BinaryWriterX bw, IEnumerable<string> list, Level5.Method comp, Encoding enc)
        {
            var ms = new MemoryStream();
            using (var bwIntern = new BinaryWriterX(ms, true))
                foreach (var t in list)
                {
                    bwIntern.Write(enc.GetBytes(t));
                    bwIntern.Write((byte)0);
                }
            bw.Write(Level5.Compress(ms, comp));
        }*/
    }
}
