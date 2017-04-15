using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cetera.Image;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Hash;

namespace archive_fa
{
    public sealed class FA
    {
        public List<FAFileInfo> Files = new List<FAFileInfo>();

        private Header header;
        private byte[] unk1;
        private byte[] unk2;

        private List<Entry> entries;
        private List<String> filenames;
		private List<String> foldernames;
		private List<uint> hashes;

		public FA(String filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //unknown lists
                unk1 = br.ReadBytes(header.offset1-header.offset0);
                unk2 = br.ReadBytes(header.entryOffset-header.offset1);

                //Entries
                entries = br.ReadMultiple<Entry>(header.entryCount);
                br.BaseStream.Position++;

                //Names
                filenames=new List<string>();
                foldernames=new List<string>();
				String currentFolder = "";
                String tmp = Encoding.GetEncoding("SJIS").GetString(Encoding.ASCII.GetBytes(br.ReadCStringA()));
                while (tmp != "")
                {
                    if (tmp[tmp.Length - 1] == '/')
                    {
                        foldernames.Add(tmp);
						currentFolder = tmp;
                    }
                    else
                    {
                        filenames.Add(currentFolder+tmp);
                    }
                    tmp = Encoding.GetEncoding("SJIS").GetString(Encoding.ASCII.GetBytes(br.ReadCStringA()));
                }

				hashes = new List<uint>();
                foreach(var name in filenames)
				{
					bool found = false;
					int count = 0;
					uint crc32 = Crc32.Create(Encoding.GetEncoding("SJIS").GetBytes(name.Split('/').Last().ToLower()));
					while (!found && count < entries.Count) {
						if (entries[count].crc32 == crc32)
						{
							Files.Add(new FAFileInfo
							{
								State = ArchiveFileState.Archived,
								FileName = name,
								FileData = new SubStream(br.BaseStream, entries[count].fileOffset + header.dataOffset, entries[count].fileSize),
								Entry = entries[count]
							});
							found = true;
						} else 
							count++;
					}
					hashes.Add(crc32);
				}
            }
        }

        /*public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                //get nameList Length
                int nameListLength = 0;
                for (int i = 0; i < Files.Count; i++) nameListLength += Files[i].FileName.Length + 1;
                while (nameListLength % 4 != 0) nameListLength += 1;

                //Offsets
                int nameOffset = (Files.Count + 1) * 0x20 + Files.Count * 0x4;
                int dataOffset = nameOffset + nameListLength + Files.Count * 0x4 * 2;

                //Header
                bw.WriteStruct(new Header
                {
                    texCount = (short)Files.Count,
                    texSecOffset = dataOffset,
                    crc32SecOffset = nameOffset + nameListLength,
                    texInfoOffset = nameOffset + nameListLength + Files.Count * 0x4
                });

                //entryList
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.WriteStruct(new Entry
                    {
                        nameOffset = nameOffset,
                        texDataSize = (int)Files[i].FileData.Length,
                        texOffset = dataOffset,
                        format = Files[i].Entry.format,
                        width = Files[i].Entry.width,
                        height = Files[i].Entry.height,
                        mipLvl = Files[i].Entry.mipLvl,
                        type = Files[i].Entry.type,
                        bitmapSizeOffset = Files[i].Entry.bitmapSizeOffset,
                        timeStamp = Files[i].Entry.timeStamp,
                    });
                    nameOffset += Files[i].FileName.Length + 1;
                    dataOffset += (int)Files[i].FileData.Length;
                }

                //texInfo 1 List
                for (int i = 0; i < Files.Count; i++) bw.Write((int)Files[i].FileData.Length);

                //nameList
                for (int i = 0; i < Files.Count; i++) { bw.WriteASCII(Files[i].FileName); bw.Write((byte)0); }
                while (bw.BaseStream.Position % 4 != 0) bw.BaseStream.Position++;

                //crc32List
                for (int i = 0; i < Files.Count; i++) { bw.Write(Files[i].hashEntry.crc32); bw.Write(Files[i].hashEntry.entryNr); }

                //texInfo 2 List
                for (int i = 0; i < Files.Count; i++) bw.Write(Files[i].texInfo);

                //Write data
                int texSecSize = 0;
                for (int i = 0; i < Files.Count; i++)
                {
                    bw.Write(new BinaryReaderX(Files[i].FileData, true).ReadBytes((int)Files[i].FileData.Length));
                    texSecSize += (int)Files[i].FileData.Length;
                }
                bw.BaseStream.Position = 0xc;
                bw.Write(texSecSize);
            }
        }*/
    }
}
