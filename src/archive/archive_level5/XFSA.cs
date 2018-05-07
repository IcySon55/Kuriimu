using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;
using System;
using Cetera.Hash;
using System.Text;

namespace archive_level5.XFSA
{
    public sealed class XFSA
    {
        public List<XFSAFileInfo> Files = new List<XFSAFileInfo>();
        Stream _stream = null;

        Header header;
        Level5.Method table0Comp;
        List<Table0Entry> table0;
        Level5.Method table1Comp;
        List<Table1Entry> table1;
        Level5.Method entriesComp;
        List<FileEntry> entries = new List<FileEntry>();
        Level5.Method stringComp;
        byte[] stringTable;
        List<string> fileNames = new List<string>();

        public XFSA(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Table 0
                br.BaseStream.Position = header.table0Offset;
                table0Comp = GetCompressionMethod(br.BaseStream);
                table0 = new BinaryReaderX(new MemoryStream(Level5.Decompress(br.BaseStream))).ReadMultiple<Table0Entry>(header.table0EntryCount);

                //Table 1
                br.BaseStream.Position = header.table1Offset;
                table1Comp = GetCompressionMethod(br.BaseStream);
                table1 = new BinaryReaderX(new MemoryStream(Level5.Decompress(br.BaseStream))).ReadMultiple<Table1Entry>(header.table1EntryCount);

                //File Entry Table
                br.BaseStream.Position = header.fileEntryTableOffset;
                entriesComp = GetCompressionMethod(br.BaseStream);
                entries = new BinaryReaderX(new MemoryStream(Level5.Decompress(br.BaseStream))).ReadMultiple<FileEntry>(header.fileEntryCount);

                //String Table
                br.BaseStream.Position = header.nameTableOffset;
                stringComp = GetCompressionMethod(br.BaseStream);
                stringTable = Level5.Decompress(br.BaseStream);

                //Add Files
                using (var stringReader = new BinaryReaderX(new MemoryStream(stringTable)))
                {
                    foreach (var dir in table0)
                    {
                        stringReader.BaseStream.Position = dir.dirNameOffset;
                        var dirName = stringReader.ReadCStringSJIS();
                        var fileCountInDir = 0;
                        foreach (var file in entries.Where((e, i) => i >= dir.fileEntryOffset && i < dir.fileEntryOffset + dir.fileCountInDir))
                        {
                            stringReader.BaseStream.Position = dir.firstFileNameInDir + file.nameOffset;
                            var fileName = stringReader.ReadCStringSJIS();
                            Files.Add(new XFSAFileInfo
                            {
                                State = ArchiveFileState.Archived,
                                FileName = Path.Combine(dirName, fileName),
                                FileData = new SubStream(br.BaseStream, header.dataOffset + (file.offset << 4), file.size),
                                fileEntry = file,
                                fileCountInDir = fileCountInDir++,
                                dirEntry = dir
                            });
                        }
                    }
                }
            }
        }

        private Level5.Method GetCompressionMethod(Stream str)
        {
            var comp = (byte)(str.ReadByte() & 7);
            str.Position--;

            return (Level5.Method)comp;
        }

        public void Save(Stream xfsa)
        {
            //Update FileInfo
            int offset = 0;
            foreach (var file in Files)
            {
                entries[file.dirEntry.fileEntryOffset + file.fileCountInDir].offset = offset >> 4;
                file.fileEntry.offset = offset >> 4;

                entries[file.dirEntry.fileEntryOffset + file.fileCountInDir].size = (int)file.FileSize;
                file.fileEntry.size = (int)file.FileSize;

                var newOffset = ((offset + file.FileSize) % 16 == 0) ? offset + file.FileSize + 16 : (offset + file.FileSize + 0xf) & ~0xf;
                offset = (int)newOffset;
            }

            using (var bw = new BinaryWriterX(xfsa))
            {
                //Table 0
                bw.BaseStream.Position = 0x24;
                header.table0Offset = (int)bw.BaseStream.Position;
                header.table0EntryCount = (short)table0.Count;
                bw.Write(CompressTable(table0, table0Comp));

                //Table 1
                bw.BaseStream.Position = (bw.BaseStream.Position + 3) & ~3;
                header.table1Offset = (int)bw.BaseStream.Position;
                header.table1EntryCount = (short)table1.Count;
                bw.Write(CompressTable(table1, table1Comp));

                //FileEntries
                bw.BaseStream.Position = (bw.BaseStream.Position + 3) & ~3;
                header.fileEntryTableOffset = (int)bw.BaseStream.Position;
                header.fileEntryCount = entries.Count;
                bw.Write(CompressTable(entries, entriesComp));

                //StringTable
                bw.BaseStream.Position = (bw.BaseStream.Position + 3) & ~3;
                header.nameTableOffset = (int)bw.BaseStream.Position;
                bw.Write(Level5.Compress(new MemoryStream(stringTable), stringComp));

                //FileData
                bw.BaseStream.Position = (bw.BaseStream.Position + 0xf) & ~0xf;
                header.dataOffset = (int)bw.BaseStream.Position;
                foreach (var file in Files)
                {
                    bw.BaseStream.Position = header.dataOffset + (file.fileEntry.offset << 4);
                    file.FileData.CopyTo(bw.BaseStream);
                    if (bw.BaseStream.Position % 16 == 0)
                        bw.WritePadding(16);
                    else
                        bw.WriteAlignment(16);
                }

                //Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }

            //Table 1
            /*var ms = new MemoryStream();
            new BinaryWriterX(ms, true).WriteMultiple(table0);
            ms.Position = 0;
            var newTable1 = Level5.Compress(ms, (Level5.Method)table1Comp);

            //Table 2
            ms = new MemoryStream();
            new BinaryWriterX(ms, true).WriteMultiple(table1);
            ms.Position = 0;
            var newTable2 = Level5.Compress(ms, (Level5.Method)table2Comp);

            //Update Entries
            Files = Files.OrderBy(f => f.entry.entry.comb1 & 0x01ffffff).ToList();
            int offset = 0;
            foreach (var file in Files) offset = file.UpdateEntry(offset);

            //Get compressed Entry section
            Files = Files.OrderBy(f => f.entry.ID).ToList();
            ms = new MemoryStream();
            new BinaryWriterX(ms, true).WriteMultiple(Files.Select(f => f.entry.entry));
            ms.Position = 0;
            var newEntryTable = Level5.Compress(ms, (Level5.Method)entriesComp);

            //Update header
            header.nameTableOffset = (uint)(0x24 + ((newTable1.Length + 3) & ~3) + ((newTable2.Length + 3) & ~3) + ((newEntryTable.Length + 3) & ~3));
            header.dataOffset = (uint)(((header.nameTableOffset + nameC.Length) + 0xf) & ~0xf);

            using (BinaryWriterX bw = new BinaryWriterX(xfsa))
            {
                //Header
                bw.WriteStruct(header);

                //Table 1
                bw.Write(newTable1);
                bw.WriteAlignment(4);

                //Table 2
                bw.Write(newTable2);
                bw.WriteAlignment(4);

                //Entries
                bw.Write(newEntryTable);
                bw.WriteAlignment(4);

                //Names
                bw.Write(nameC);
                bw.WriteAlignment();

                //Files
                Files = Files.OrderBy(f => f.entry.entry.comb1 & 0x01ffffff).ToList();
                foreach (var file in Files)
                {
                    file.FileData.CopyTo(bw.BaseStream);
                    if (bw.BaseStream.Position % 0x10 != 0)
                        bw.WriteAlignment();
                    else
                    {
                        bw.Write(new byte[0x10]);
                    }
                }


                //FileEntries Table
                //bw.Write((entries.Count * 0xc) << 3);

                /*uint offset = 0;
                List<XFSAFileInfo> files = new List<XFSAFileInfo>();
                foreach (var entry in entries)
                {
                    var file = Files.Find(c => c.entry.comb1 == entry.comb1);
                    files.Add(file);

                    //catch file limits
                    if (file.FileData.Length >= 0x100000)
                    {
                        throw new Exception("File " + file.FileName + " is too big to pack into this archive type!");
                    }
                    else if (offset + dataOffset >= 0x20000000)
                    {
                        throw new Exception("The archive can't be bigger than 0x10000000 Bytes.");
                    }

                    //edit entry
                    entry.comb1 = (entry.comb1 & 0xfe000000) | (offset >> 4);
                    entry.comb2 = (entry.comb1 & 0xfff00000) | ((uint)file.FileData.Length);

                    //write entry
                    bw.WriteStruct(entry);

                    //edit values
                    offset = (uint)(((offset + file.FileData.Length) + 0xf) & ~0xf);
                }*/

            //Nametable
            //bw.Write(nameC);

            //Files
            //bw.BaseStream.Position = dataOffset;
            //foreach (var file in files)
            //{
            //    file.FileData.CopyTo(bw.BaseStream);
            //    bw.BaseStream.Position = (bw.BaseStream.Position + 0xf) & ~0xf;
            //}

            //Header
            //header.nameTableOffset = (uint)(0x24 + table1.Length + table2.Length + entries.Count * 0xc + 4);
            //header.dataOffset = (uint)dataOffset;
            //bw.BaseStream.Position = 0;
            //bw.WriteStruct(header);
            //}
        }

        private byte[] CompressTable<T>(List<T> table, Level5.Method method)
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true))
                bw.WriteMultiple(table);
            ms.Position = 0;
            return Level5.Compress(ms, method);
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
