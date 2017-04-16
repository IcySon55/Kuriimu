using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.IO;
using Kuriimu.Contract;

namespace Cetera.Archive
{
    public sealed class DARC
    {
        public List<DarcFileInfo> Files;

        public class DarcFileInfo : ArchiveFileInfo
        {
            public Entry Entry;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public String4 magic;
            public ByteOrder byteOrder;
            public short headerSize;
            public int version;
            public int fileSize;
            public int tableOffset;
            public int tableLength;
            public int dataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Entry
        {
            private int tmp1;
            public int fileOffset;
            public int size;

            public bool IsFolder => (tmp1 >> 24) == 1;
            public int FilenameOffset => tmp1 & 0xFFFFFF;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DirEntry
        {
            public String dir;
            public int dirCount;
            public int lvl;
        }

        private Header header;

        public DARC(String filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //EntryList
                List<Entry> entries = new List<Entry>();
                entries.Add(br.ReadStruct<Entry>());
                entries.AddRange(br.ReadMultiple<Entry>(entries[0].size - 1));

                //FileData + names
                Files = new List<DarcFileInfo>();
                String[] paths = new String[entries[0].size];
                var basePos = br.BaseStream.Position;
                for (int i = 0; i < entries[0].size; i++)
                {
                    var entry = entries[i];
                    br.BaseStream.Position = basePos + entry.FilenameOffset;

                    var arcPath = br.ReadCStringW();
                    if (entry.IsFolder)
                    {
                        arcPath += '/';
                        for (int j = i; j < entry.size; j++)
                        {
                            paths[j] += arcPath;
                        }
                    }
                    else
                    {
                        paths[i] += arcPath;
                        Files.Add(new DarcFileInfo
                        {
                            FileName = paths[i],
                            FileData = new SubStream(br.BaseStream, entry.fileOffset, entry.size),
                            State = ArchiveFileState.Archived
                        });
                    }
                }
            }
        }

        public List<DirEntry> dirEntries;

        public void Save(String filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.BaseStream.Position = 0x1c;

                GetDirInfo();

                int nameListLength = 0;
                foreach (var name in dirEntries)
                    nameListLength += (name.dir.Length + 1) * 2;

                //Write Entry table
                int filesOffset = 0;
                int offset = 0;
                int dataOffset = 0x1c + dirEntries.Count * 0xc + nameListLength;
                for (int i = 0; i < dirEntries.Count; i++)
                {
                    if (dirEntries[i].dir != "." && dirEntries[i].dir.Contains('.'))
                    {
                        bw.Write(offset);
                        bw.Write(dataOffset);
                        bw.Write((uint)Files[filesOffset].FileSize.GetValueOrDefault());

                        dataOffset += (int)Files[filesOffset++].FileSize.GetValueOrDefault();
                        offset += (dirEntries[i].dir.Length + 1) * 2;
                    }
                    else
                    {
                        bw.Write(offset | 0x01000000);
                        bw.Write(dirEntries[i].lvl);
                        bw.Write(dirEntries[i].dirCount);

                        offset += (dirEntries[i].dir.Length + 1) * 2;
                    }
                }

                //Write names
                foreach (var dir in dirEntries)
                {
                    for (int i = 0; i < dir.dir.Length; i++)
                        bw.Write((short)dir.dir[i]);
                    bw.Write((short)0);
                }

                //Write FileData
                foreach (var data in Files)
                    bw.Write(new BinaryReaderX(data.FileData).ReadBytes((int)data.FileSize.GetValueOrDefault()));

                //Header
                bw.BaseStream.Position = 0;
                header.fileSize = (int)bw.BaseStream.Length;
                header.tableLength = dirEntries.Count * 0xc + nameListLength;
                header.dataOffset = 0x1c + dirEntries.Count * 0xc + nameListLength;
                bw.WriteStruct(header);
            }
        }

        public void GetDirInfo()
        {
            List<String> dirsNames = new List<string>();
            foreach (var file in Files)
            {
                String[] splitStr = file.FileName.Split('/');
                String tmp = "";
                foreach (var partStr in splitStr)
                {
                    tmp += partStr;
                    if (dirsNames.Find(e => e.Contains(tmp)) == null)
                        dirsNames.Add(tmp);
                    tmp += "/";
                }
            }

            List<string[]> result = new List<string[]>();
            foreach (var parts in dirsNames)
                result.Add(parts.Split('/'));

            int count = 1;
            dirEntries = new List<DirEntry>();
            foreach (var part in result)
            {
                foreach (var subpart in part)
                    if (dirEntries.Find(e => e.dir.Contains(subpart)) == null)
                    {
                        dirEntries.Add(new DirEntry
                        {
                            dir = subpart,
                            dirCount = count,
                            lvl = (subpart == "." || subpart == "") ? 0 : part.Length - 2
                        });
                    }
                    else
                    {
                        for (int i = 0; i < dirEntries.Count; i++)
                            if (dirEntries[i].dir == subpart)
                                dirEntries[i].dirCount++;
                    }
                count++;
            }
        }
    }
}