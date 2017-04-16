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
                String tmp = br.ReadCStringA();
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
                    tmp = br.ReadCStringA();
                }

                hashes = new List<uint>();
                foreach(var name in filenames)
                {
                    bool found = false;
                    int count = 0;
                    uint crc32 = Crc32.Create(Encoding.ASCII.GetBytes(name.Split('/').Last().ToLower()));
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

        public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                bw.BaseStream.Position = 0x48;

                //first unknown half of info section
                bw.Write(unk1);
                bw.Write(unk2);

                //filename List
                byte[] dirnameList = GetDirNameList();

                //entryList and Data
                uint offset = 0;
                uint dataOffset = (uint)(0x48 + unk1.Length + unk2.Length + Files.Count * 0x10 + dirnameList.Length);
                foreach (var file in Files)
                {
                    bw.Write(file.Entry.crc32);
                    bw.Write(file.Entry.nameOffset);
                    bw.Write(offset);
                    bw.Write((int)file.FileSize.GetValueOrDefault());

                    long bk = bw.BaseStream.Position;
                    bw.BaseStream.Position = dataOffset+offset;
                    bw.Write(new BinaryReaderX(file.FileData).ReadBytes((int)file.FileSize.GetValueOrDefault()));
                    bw.BaseStream.Position = bk;

                    offset += (uint)file.FileSize.GetValueOrDefault();
                }

                //write filenameList
                bw.Write(dirnameList);

                //Write Header
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);
            }
        }

        public byte[] GetDirNameList()
        {
            List<byte> list = new List<byte>();
            list.Add(0);
            int pos = 0;
            String lastFolder = "";
            Encoding ascii = Encoding.ASCII;

            foreach (var file in filenames)
            {
                if (!file.Contains('/'))
                {
                    list.AddRange(ascii.GetBytes(file));
                    list.Add(0);
                }
                else
                {
                    bool found = false;

                    while (!found)
                    {
                        if (lastFolder != foldernames[pos])
                        {
                            list.AddRange(ascii.GetBytes(foldernames[pos]));
                            list.Add(0);
                        }

                        if (!file.Contains(foldernames[pos])) pos++;
                        else
                            if (pos + 1 < foldernames.Count)
                                if (!file.Contains(foldernames[pos + 1]))
                                {
                                    lastFolder = foldernames[pos];
                                    found = true;
                                }
                                else pos++;
                            else
                            {
                                lastFolder = foldernames[pos];
                                found = true;
                            }
                    }

                    list.AddRange(ascii.GetBytes(file.Split('/').Last()));
                    list.Add(0);
                }
            }

            for (int i = pos + 1; i < foldernames.Count; i++)
            {
                list.AddRange(ascii.GetBytes(foldernames[i]));
                list.Add(0);
            }

            while (list.Count % 4 != 0) list.Add(0);

            return list.ToArray();
        }
    }
}
