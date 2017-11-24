using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.IO;
using Kontract;

namespace text_mes
{
    public class MES
    {
        public List<Label> Labels = new List<Label>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public Magic magic;
            public int headerLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct EntryHeader
        {
            public int entryOffset;
            public int entryCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ScenarioHeader
        {
            public int scenarioOffset;
            public int scenarioCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Entry
        {
            public int nameOffset;
            public int nameLength;
            public int stringOffset;
            public int stringLength;
            public long unk1;
            public long unk2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ScenarioEntry
        {
            public int textEntryCount;
            public int containerOffset;
        }

        public Header header;
        public EntryHeader entryHeader;
        public ScenarioHeader sceHeader;
        public byte[] unk1;
        public List<Entry> entries;
        public List<ScenarioEntry> sceEntries;
        public List<List<int>> scenarios = new List<List<int>>();

        public MES(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Headers
                header = br.ReadStruct<Header>();
                entryHeader = br.ReadStruct<EntryHeader>();

                br.BaseStream.Position += 0x10;
                sceHeader = br.ReadStruct<ScenarioHeader>();

                unk1 = br.ReadBytes(0x18);

                //Text Entries
                br.BaseStream.Position = entryHeader.entryOffset;
                entries = br.ReadMultiple<Entry>(entryHeader.entryCount);

                //scenario entries
                br.BaseStream.Position = sceHeader.scenarioOffset;
                sceEntries = br.ReadMultiple<ScenarioEntry>(sceHeader.scenarioCount);

                //Add Labels
                foreach (var entry in entries)
                    Labels.Add(new Label
                    {
                        Name = br.PeekString((uint)entry.nameOffset, entry.nameLength),
                        Text = br.PeekString((uint)entry.stringOffset, entry.stringLength * 2, Encoding.GetEncoding("UTF-16")).Replace("\x1b", "\r\n").TrimEnd('\0')
                    });

                //Add scenarios for storage
                foreach (var sce in sceEntries)
                {
                    if (sce.containerOffset != 0)
                    {
                        br.BaseStream.Position = sce.containerOffset;
                        scenarios.Add(br.ReadMultiple<int>(sce.textEntryCount));
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                var textOffset = 0x40 + entries.Count() * 0x20 + sceEntries.Count() * 0x8;

                //Write texts and update textEntries
                bw.BaseStream.Position = textOffset;
                var count = 0;
                for (int i = 0; i < entries.Count(); i++)
                {
                    entries[i].nameOffset = (int)bw.BaseStream.Position;
                    entries[i].nameLength = Encoding.ASCII.GetByteCount(Labels[count].Name);
                    bw.Write(Encoding.ASCII.GetBytes(Labels[count].Name));
                    bw.WritePadding(2);
                    bw.WriteAlignment(2);

                    entries[i].stringOffset = (int)bw.BaseStream.Position;
                    entries[i].stringLength = Encoding.GetEncoding("UTF-16").GetByteCount(Labels[count].Text) / 2;
                    var modText = Labels[count++].Text.Replace("\r\n", "\x1b");
                    entries[i].stringLength += modText.Count(m => m == '\x1b');
                    bw.Write(Encoding.GetEncoding("UTF-16").GetBytes(modText));
                    bw.WritePadding(4 + modText.Count(m => m == '\x1b') * 4);
                }
                bw.WriteAlignment(4);

                //Write scenario Containers and update sceEntries
                count = 0;
                for (int i = 0; i < sceEntries.Count(); i++)
                {
                    if ((bw.BaseStream.Position & 0x4) == 0)
                        bw.WritePadding(4);
                    if (sceEntries[i].containerOffset != 0)
                    {
                        sceEntries[i].containerOffset = (int)bw.BaseStream.Position;
                        foreach (var sceTextEntry in scenarios[count++])
                            bw.Write(sceTextEntry);
                    }
                }

                //final nulls, they seem to have no reason to exist ;)
                bw.WritePadding(textOffset);

                //Write both tables
                bw.BaseStream.Position = 0x40;
                foreach (var entry in entries)
                    bw.WriteStruct(entry);
                foreach (var sceContainer in sceEntries)
                    bw.WriteStruct(sceContainer);

                //Header
                bw.BaseStream.Position = 0x0;
                bw.WriteStruct(header);
                bw.WriteStruct(entryHeader);
                bw.BaseStream.Position += 0x10;
                bw.WriteStruct(sceHeader);
                bw.Write(unk1);
            }
        }
    }
}
