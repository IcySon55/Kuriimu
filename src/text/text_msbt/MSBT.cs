using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.IO;
using Cetera.Hash;

namespace text_msbt
{
    public sealed class MSBT
    {
        public const string LabelFilter = @"^[a-zA-Z0-9_]+$";
        public const uint LabelHashMagic = 0x492;
        public const int LabelMaxLength = 64;

        private const long _byteOrderOffset = 0x8;
        private const long _headerSize = 0x20;
        private byte _paddingChar = 0xAB;

        public Header Header = new Header();
        public LBL1 LBL1 = new LBL1();
        public NLI1 NLI1 = new NLI1();
        public ATO1 ATO1 = new ATO1();
        public ATR1 ATR1 = new ATR1();
        public TSY1 TSY1 = new TSY1();
        public TXT2 TXT2 = new TXT2();
        public Encoding Encoding = Encoding.Unicode;
        public List<string> SectionOrder = new List<string>();
        public bool HasLabels;

        public MSBT(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                // Get ByteOrder
                br.BaseStream.Position = _byteOrderOffset;
                br.ByteOrder = (ByteOrder)br.ReadUInt16();
                br.BaseStream.Position = 0;

                // Header
                Header = br.ReadStruct<Header>();
                Encoding = Header.EncodingByte == EncodingByte.UTF8 ? Encoding.UTF8 : Header.ByteOrder == ByteOrder.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode;

                if (Header.Magic != "MsgStdBn")
                    throw new InvalidMSBTException("The file provided is not a valid MSBT file.");
                if (Header.FileSize != br.BaseStream.Length)
                    throw new InvalidMSBTException("The file provided is not a valid MSBT file. Filesize mismtach.");

                for (var i = 0; i < Header.NumberOfSections; i++)
                {
                    switch (br.PeekString())
                    {
                        case "LBL1":
                            ReadLBL1(br);
                            SectionOrder.Add("LBL1");
                            break;
                        case "NLI1":
                            ReadNLI1(br);
                            SectionOrder.Add("NLI1");
                            break;
                        case "ATO1":
                            ReadATO1(br);
                            SectionOrder.Add("ATO1");
                            break;
                        case "ATR1":
                            ReadATR1(br);
                            SectionOrder.Add("ATR1");
                            break;
                        case "TSY1":
                            ReadTSY1(br);
                            SectionOrder.Add("TSY1");
                            break;
                        case "TXT2":
                            ReadTXT2(br);
                            SectionOrder.Add("TXT2");
                            break;
                    }
                }
            }
        }

        // Tools
        public string GetString(byte[] bytes)
        {
            var sb = new StringBuilder();
            using (var br = new BinaryReaderX(new MemoryStream(bytes), Encoding, Header.ByteOrder))
            {
                while (br.BaseStream.Length != br.BaseStream.Position)
                {
                    var c = br.ReadChar();
                    sb.Append(c);
                    if (c == 0xE)
                    {
                        sb.Append((char)br.ReadInt16());
                        sb.Append((char)br.ReadInt16());
                        int count = br.ReadInt16();
                        sb.Append((char)count);
                        for (var i = 0; i < count; i++)
                        {
                            sb.Append((char)br.ReadByte());
                        }
                    }
                }
            }
            return sb.Remove(sb.Length - 1, 1).ToString();
        }

        public byte[] GetBytes(string str)
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, Encoding, Header.ByteOrder))
            {
                for (var i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    bw.Write(c);
                    if (c == 0xE)
                    {
                        bw.Write((short)str[++i]);
                        bw.Write((short)str[++i]);
                        int count = str[++i];
                        bw.Write((short)count);
                        for (var j = 0; j < count; j++)
                        {
                            bw.Write((byte)str[++i]);
                        }
                    }
                }
                bw.Write('\0');
            }
            return ms.ToArray();
        }

        // Reading
        private void ReadLBL1(BinaryReaderX br)
        {
            LBL1.Section = br.ReadStruct<Section>();
            LBL1.NumberOfGroups = br.ReadUInt32();

            LBL1.Groups = br.ReadMultiple<Group>((int)LBL1.NumberOfGroups).ToList();

            foreach (Group grp in LBL1.Groups)
                for (var i = 0; i < grp.NumberOfLabels; i++)
                    LBL1.Labels.Add(new Label
                    {
                        Name = br.ReadString(Convert.ToInt32(br.ReadByte())),
                        Index = br.ReadUInt32(),
                        Checksum = (uint)LBL1.Groups.IndexOf(grp)
                    });

            //// Old rename correction
            //foreach (var lbl in LBL1.Labels)
            //{
            //    var previousChecksum = lbl.Checksum;
            //    lbl.Checksum = SimpleHash.Create(lbl.Name, LabelHashMagic, LBL1.NumberOfGroups);

            //    if (previousChecksum == lbl.Checksum) continue;
            //    LBL1.Groups[(int)previousChecksum].NumberOfLabels -= 1;
            //    LBL1.Groups[(int)lbl.Checksum].NumberOfLabels += 1;
            //}

            HasLabels = LBL1.Labels.Count > 0;

            _paddingChar = br.SeekAlignment(_paddingChar);
        }

        private void ReadNLI1(BinaryReaderX br)
        {
            NLI1.Section = br.ReadStruct<Section>();
            NLI1.Unknown = br.ReadBytes((int)NLI1.Section.Size); // Read in the entire section at once since we don't know what it's for

            _paddingChar = br.SeekAlignment(_paddingChar);
        }

        private void ReadATO1(BinaryReaderX br)
        {
            ATO1.Section = br.ReadStruct<Section>();
            ATO1.Unknown = br.ReadBytes((int)ATO1.Section.Size); // Read in the entire section at once since we don't know what it's for

            _paddingChar = br.SeekAlignment(_paddingChar); // TODO: Determine why this line was missing before
        }

        private void ReadATR1(BinaryReaderX br)
        {
            ATR1.Section = br.ReadStruct<Section>();
            ATR1.Unknown = br.ReadBytes((int)ATR1.Section.Size); // Read in the entire section at once since we don't know what it's for

            _paddingChar = br.SeekAlignment(_paddingChar);
        }

        private void ReadTSY1(BinaryReaderX br)
        {
            TSY1.Section = br.ReadStruct<Section>();
            TSY1.Unknown = br.ReadBytes((int)TSY1.Section.Size); // Read in the entire section at once since we don't know what it's for

            _paddingChar = br.SeekAlignment(_paddingChar);
        }

        private void ReadTXT2(BinaryReaderX br)
        {
            TXT2.Section = br.ReadStruct<Section>();
            var startOfStrings = (int)br.BaseStream.Position;
            TXT2.NumberOfStrings = br.ReadUInt32();

            var offsets = br.ReadMultiple<uint>((int)TXT2.NumberOfStrings);

            for (var i = 0; i < TXT2.NumberOfStrings; i++)
                TXT2.Strings.Add(new String
                {
                    Text = GetString(br.ReadBytes((i + 1 < offsets.Count ? startOfStrings + (int)offsets[i + 1] : startOfStrings + (int)TXT2.Section.Size) - (startOfStrings + (int)offsets[i]))),
                    Index = (uint)i
                });

            // Tie in LBL1 labels
            foreach (var lbl in LBL1.Labels)
                lbl.String = TXT2.Strings[(int)lbl.Index];

            _paddingChar = br.SeekAlignment(_paddingChar);
        }

        // Manipulation
        public Label AddLabel(string labelName)
        {
            var newString = new String();
            TXT2.Strings.Add(newString);
            TXT2.NumberOfStrings += 1;

            var newLabel = new Label
            {
                Name = labelName.Trim(),
                Index = (uint)TXT2.Strings.IndexOf(newString),
                Checksum = SimpleHash.Create(labelName.Trim(), LabelHashMagic, LBL1.NumberOfGroups),
                String = newString
            };
            LBL1.Labels.Add(newLabel);
            LBL1.Groups[(int)newLabel.Checksum].NumberOfLabels += 1;
            //ATR1.NumberOfAttributes += 1;

            return newLabel;
        }

        public void RenameLabel(Label label, string labelName)
        {
            label.Name = labelName.Trim();
            LBL1.Groups[(int)label.Checksum].NumberOfLabels -= 1;
            label.Checksum = SimpleHash.Create(labelName.Trim(), LabelHashMagic, LBL1.NumberOfGroups);
            LBL1.Groups[(int)label.Checksum].NumberOfLabels += 1;
        }

        public void RemoveLabel(Label label)
        {
            var textIndex = TXT2.Strings.IndexOf(label.String);
            for (var i = 0; i < TXT2.NumberOfStrings; i++)
                if (LBL1.Labels[i].Index > textIndex)
                    LBL1.Labels[i].Index -= 1;

            LBL1.Groups[(int)label.Checksum].NumberOfLabels -= 1;
            LBL1.Labels.Remove(label);
            //ATR1.NumberOfAttributes -= 1;
            TXT2.Strings.RemoveAt((int)label.Index);
            TXT2.NumberOfStrings -= 1;
        }

        // Saving
        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, Header.ByteOrder))
            {
                bw.BaseStream.Position = _headerSize;

                foreach (var section in SectionOrder)
                {
                    switch (section)
                    {
                        case "LBL1":
                            WriteLBL1(bw);
                            break;
                        case "NLI1":
                            WriteNLI1(bw);
                            break;
                        case "ATO1":
                            WriteATO1(bw);
                            break;
                        case "ATR1":
                            WriteATR1(bw);
                            break;
                        case "TSY1":
                            WriteTSY1(bw);
                            break;
                        case "TXT2":
                            WriteTXT2(bw);
                            break;
                    }
                }

                // Header
                Header.FileSize = (uint)bw.BaseStream.Position;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(Header);
            }
        }

        private void WriteLBL1(BinaryWriterX bw)
        {
            // Calculate Section Size
            uint newSize = sizeof(uint); // Number of Groups
            newSize = LBL1.Groups.Aggregate(newSize, (current, grp) => current + sizeof(uint) + sizeof(uint));
            newSize = LBL1.Labels.Aggregate(newSize, (current, lbl) => current + (uint)(sizeof(byte) + lbl.Name.Length + sizeof(uint)));
            LBL1.Section.Size = newSize;

            // Calculate Group Offsets
            uint runningTotal = 0;
            for (var i = 0; i < LBL1.Groups.Count; i++)
            {
                LBL1.Groups[i].Offset = LBL1.NumberOfGroups * sizeof(uint) * 2 + sizeof(uint) + runningTotal;
                runningTotal = LBL1.Labels.Where(lbl => lbl.Checksum == i).Aggregate(runningTotal, (current, lbl) => current + (uint)(sizeof(byte) + lbl.Name.Length + sizeof(uint)));
            }

            // Section
            bw.WriteStruct(LBL1.Section);
            bw.Write(LBL1.NumberOfGroups);

            // Groups
            foreach (var group in LBL1.Groups)
                bw.WriteStruct(group);

            // Labels
            foreach (var group in LBL1.Groups)
                foreach (var label in LBL1.Labels)
                    if (label.Checksum == LBL1.Groups.IndexOf(group))
                    {
                        bw.Write(Convert.ToByte(Encoding.ASCII.GetBytes(label.Name).Length));
                        bw.WriteASCII(label.Name);
                        bw.Write(label.Index);
                    }

            bw.WriteAlignment(_paddingChar);
        }

        private void WriteNLI1(BinaryWriterX bw)
        {
            bw.WriteStruct(NLI1.Section);
            bw.Write(NLI1.Unknown);

            bw.WriteAlignment(_paddingChar);
        }

        private void WriteATO1(BinaryWriterX bw)
        {
            bw.WriteStruct(ATO1.Section);
            bw.Write(ATO1.Unknown);

            bw.WriteAlignment(_paddingChar);
        }

        private void WriteATR1(BinaryWriterX bw)
        {
            bw.WriteStruct(ATR1.Section);
            //bw.Write(ATR1.NumberOfAttributes);
            bw.Write(ATR1.Unknown);

            bw.WriteAlignment(_paddingChar);
        }

        private void WriteTSY1(BinaryWriterX bw)
        {
            bw.WriteStruct(TSY1.Section);
            bw.Write(TSY1.Unknown);

            bw.WriteAlignment(_paddingChar);
        }

        private void WriteTXT2(BinaryWriterX bw)
        {
            // Optimization
            var stringBytes = TXT2.Strings.Select(str => GetBytes(str.Text)).ToList();

            // Calculate Section Size
            var newSize = TXT2.NumberOfStrings * sizeof(uint) + sizeof(uint);
            newSize = stringBytes.Aggregate(newSize, (current, str) => current + (uint)str.Length);
            TXT2.Section.Size = newSize;

            // Calculate String Offsets
            var offsets = new List<uint>();
            uint runningTotal = 0;
            for (var i = 0; i < TXT2.NumberOfStrings; i++)
            {
                offsets.Add(TXT2.NumberOfStrings * sizeof(uint) + sizeof(uint) + runningTotal);
                runningTotal += (uint)stringBytes[i].Length;
            }

            // Section
            bw.WriteStruct(TXT2.Section);
            bw.Write(TXT2.NumberOfStrings);

            // Offsets
            foreach (var offset in offsets)
                bw.Write(offset);

            // Strings
            foreach (var str in stringBytes)
                bw.Write(str);

            bw.WriteAlignment(_paddingChar);
        }
    }
}