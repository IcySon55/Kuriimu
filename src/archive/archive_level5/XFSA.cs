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
        byte table1Comp;
        List<Table1Entry> table1;
        byte table2Comp;
        List<Table2Entry> table2;
        byte[] nameC;
        byte entriesComp;
        List<FileEntryIndex> entries = new List<FileEntryIndex>();
        List<string> fileNames = new List<string>();

        public XFSA(Stream input)
        {
            _stream = input;
            using (var br = new BinaryReaderX(input, true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //1st table
                br.BaseStream.Position = header.offset1;
                table1Comp = (byte)(br.ReadByte() & 7);
                br.BaseStream.Position--;
                var tmpT1 = br.ReadBytes((int)(header.offset2 - header.offset1));
                table1 = new BinaryReaderX(Support.DecompressToStream(tmpT1)).ReadMultiple<Table1Entry>(header.table1EntryCount);

                //2nd table
                br.BaseStream.Position = header.offset2;
                table2Comp = (byte)(br.ReadByte() & 7);
                br.BaseStream.Position--;
                var tmpT2 = br.ReadBytes((int)(header.fileEntryTableOffset - header.offset2));
                table2 = new BinaryReaderX(Support.DecompressToStream(tmpT2)).ReadMultiple<Table2Entry>(header.table2EntryCount);

                //File Entry Table
                br.BaseStream.Position = header.fileEntryTableOffset;
                entriesComp = (byte)(br.ReadByte() & 7);
                br.BaseStream.Position--;
                entries = new BinaryReaderX(Support.DecompressToStream(br.ReadBytes((int)(header.nameTableOffset - header.fileEntryTableOffset))))
                    .ReadMultipleEntriesInc((int)header.fileEntryCount).ToList();

                //Name Table
                br.BaseStream.Position = header.nameTableOffset;
                nameC = br.ReadBytes((int)(header.dataOffset - header.nameTableOffset));
                fileNames = GetFileNames(Level5.Decompress(new MemoryStream(nameC)));

                //Add Files
                List<uint> combs = new List<uint>();
                foreach (var name in fileNames)
                {
                    var crc32 = Crc32.Create(name.Split('/').Last(), Encoding.GetEncoding("SJIS"));
                    var entry = entries.Find(c => c.entry.crc32 == crc32 && !combs.Contains(c.entry.comb1));
                    combs.Add(entry.entry.comb1);
                    Files.Add(new XFSAFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = name,
                        FileData = new SubStream(br.BaseStream, header.dataOffset + ((entry.entry.comb1 & 0x01ffffff) << 4), entry.entry.comb2 & 0x000fffff),
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

        public void Save(Stream xfsa)
        {
            //Table 1
            var ms = new MemoryStream();
            new BinaryWriterX(ms, true).WriteMultiple(table1);
            ms.Position = 0;
            var newTable1 = Level5.Compress(ms, (Level5.Method)table1Comp);

            //Table 2
            ms = new MemoryStream();
            new BinaryWriterX(ms, true).WriteMultiple(table2);
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
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
