using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public struct Entry
        {
            public int nameOffset;
            public int nameLength;
            public int stringOffset;
            public int stringLength;
            public long unk1;
            public long unk2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ScenarioEntry
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
                        Text = br.PeekString((uint)entry.stringOffset, entry.stringLength * 2, Encoding.GetEncoding("UTF-16")).Replace('\x1b', '\n').TrimEnd('\0')
                    });

                //Add scenarios for storage
                foreach (var sce in sceEntries)
                {
                    br.BaseStream.Position = sce.containerOffset;
                    scenarios.Add(br.ReadMultiple<int>(sce.textEntryCount));
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
