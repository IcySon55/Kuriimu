using Kontract;
using Kontract.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace text_gmd
{
    //Version 1
    public class GMDv1 : IGMD
    {
        public GMDContent GMDContent { get; set; } = new GMDContent();
        private uint _v1Constant = 0x29080170;  //Dual Destinies - 0x29080170; Resident Evil Revelations - 0x11e85510

        #region Structs
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic Magic;
            public int Version;
            public Language Language;
            public long Zero1;
            public int LabelCount;
            public int SectionCount;
            public int LabelSize;
            public int SectionSize;
            public int NameSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class LabelEntry
        {
            public int SectionID;
            public int LabelOffset; //relative to LabelDataOffset and after subtracting (_v1Constant + Header.LabelCount * 0x80)
        }
        #endregion

        public void Load(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                // Set endianess
                if (br.PeekString() == "\0DMG")
                    br.ByteOrder = GMDContent.ByteOrder = ByteOrder.BigEndian;

                // Header
                var Header = br.ReadStruct<Header>();
                var Name = br.ReadCStringA();
                var HeaderLength = (int)br.BaseStream.Position;
                GMDContent.Name = Name;

                // Label Entries
                var LabelEntries = (Header.LabelCount > 0) ? br.ReadMultiple<LabelEntry>(Header.LabelCount) : new List<LabelEntry>();
                var LabelDataOffset = (int)br.BaseStream.Position;

                // Text
                br.BaseStream.Position = 0x28 + (Header.NameSize + 1) + (Header.LabelCount * 0x8) + Header.LabelSize;
                var text = br.ReadBytes(Header.SectionSize);

                // Text deobfuscation
                var deXor = XOR.DeXOR(text);

                using (var brt = new BinaryReaderX(deXor))
                {
                    var counter = 0;
                    for (var i = 0; i < Header.SectionCount; i++)
                    {
                        var bk = brt.BaseStream.Position;
                        var tmp = brt.ReadByte();
                        while (tmp != 0)
                            tmp = brt.ReadByte();
                        var textSize = brt.BaseStream.Position - bk;
                        brt.BaseStream.Position = bk;

                        //Get Label if existent
                        var label = "";
                        if (LabelEntries.Find(l => l.SectionID == i) != null)
                        {
                            bk = br.BaseStream.Position;
                            br.BaseStream.Position = LabelEntries.Find(l => l.SectionID == i).LabelOffset - LabelEntries.First().LabelOffset + (HeaderLength + Header.LabelCount * 0x8);
                            label = br.ReadCStringA();
                            br.BaseStream.Position = bk;
                        }

                        GMDContent.Content.Add(new Label
                        {
                            Name = label == "" ? "no_name_" + counter++.ToString() : label,
                            Text = brt.ReadString((int)textSize, Encoding.UTF8).Replace("\r\n", "\xa").Replace("\xa", "\r\n"),
                            TextID = i
                        });
                    }
                }
            }
        }

        public void Save(string filename, Platform platform, Game game)
        {
            //Get Text Section
            var TextBlob = Encoding.UTF8.GetBytes(GMDContent.Content.Aggregate("", (output, c) => output + c.Text.Replace("\r\n", "\xa").Replace("\xa", "\r\n") + "\0"));

            //XOR, if needed
            if (platform == Platform.CTR && game == Game.DD)
                TextBlob = new BinaryReaderX(XOR.ReXOR(TextBlob, 0)).ReadAllBytes();

            //Get Label Blob
            var LabelBlob = Encoding.ASCII.GetBytes(GMDContent.Content.Aggregate("", (output, c) => output + (c.Name.Contains("no_name") ? "" : c.Name + "\0")));

            //Create LabelEntries
            var Entries = new List<LabelEntry>();
            var LabelOffset = 0;
            var LabelCount = GMDContent.Content.Count(c => !c.Name.Contains("no_name"));
            for (int i = 0; i < GMDContent.Content.Count(); i++)
            {
                if (!GMDContent.Content[i].Name.Contains("no_name"))
                {
                    Entries.Add(new LabelEntry
                    {
                        SectionID = i,
                        LabelOffset = LabelOffset + (0x29080170 + LabelCount * 0x80)
                    });
                    LabelOffset += Encoding.ASCII.GetByteCount(GMDContent.Content[i].Name) + 1;
                }
            }

            //Header
            var Header = new Header
            {
                Magic = GMDContent.ByteOrder == ByteOrder.BigEndian ? "\0DMG" : "GMD\0",
                Version = 0x00010201,
                Language = Language.ENGLISH,
                Zero1 = 0,
                LabelCount = LabelCount,
                SectionCount = GMDContent.Content.Count(),
                LabelSize = LabelBlob.Length,
                SectionSize = TextBlob.Length,
                NameSize = Encoding.ASCII.GetByteCount(GMDContent.Name)
            };

            //Write stuff
            using (var bw = new BinaryWriterX(File.Create(filename), GMDContent.ByteOrder))
            {
                //Header
                bw.WriteStruct(Header);
                bw.Write(Encoding.ASCII.GetBytes(GMDContent.Name + "\0"));

                //LabelEntries
                foreach (var entry in Entries)
                    bw.WriteStruct(entry);

                //Labels
                bw.Write(LabelBlob);

                //Text Sections
                bw.Write(TextBlob);
            }
        }

        public void RenameLabel(int labelId, string labelName)
        {
            GMDContent.Content[labelId].Name = labelName;
        }
    }
}
