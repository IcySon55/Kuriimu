using KuriimuContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace file_msbt
{
	public sealed class MSBT
	{
		public const string LabelFilter = @"^[a-zA-Z0-9_]+$";
		public const int LabelMaxLength = 64;

		public Header Header = new Header();
		public LBL1 LBL1 = new LBL1();
		public NLI1 NLI1 = new NLI1();
		public ATO1 ATO1 = new ATO1();
		public ATR1 ATR1 = new ATR1();
		public TSY1 TSY1 = new TSY1();
		public TXT2 TXT2 = new TXT2();
		public Encoding FileEncoding = Encoding.Unicode;
		public List<string> SectionOrder = new List<string>();
		public bool HasLabels = false;

		private byte paddingChar = 0xAB;

		public MSBT(string filename)
		{
			using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReaderX br = new BinaryReaderX(fs);

				// Header
				Header.Identifier = br.ReadString(8);
				if (Header.Identifier != "MsgStdBn")
					throw new InvalidMSBTException("The file provided is not a valid MSBT file.");

				// Byte Order
				Header.ByteOrderMark = br.ReadBytes(2);
				br.ByteOrder = Header.ByteOrderMark[0] > Header.ByteOrderMark[1] ? ByteOrder.LittleEndian : ByteOrder.BigEndian;

				Header.Unknown1 = br.ReadUInt16();

				// Encoding
				Header.EncodingByte = (EncodingByte)br.ReadByte();
				FileEncoding = (Header.EncodingByte == EncodingByte.UTF8 ? Encoding.UTF8 : Encoding.Unicode);

				Header.Unknown2 = br.ReadByte();
				Header.NumberOfSections = br.ReadUInt16();
				Header.Unknown3 = br.ReadUInt16();
				Header.FileSizeOffset = (uint)br.BaseStream.Position; // Record offset for future use
				Header.FileSize = br.ReadUInt32();
				Header.Unknown4 = br.ReadBytes(10);

				if (Header.FileSize != br.BaseStream.Length)
					throw new InvalidMSBTException("The file provided is not a valid MSBT file.");

				SectionOrder.Clear();
				for (int i = 0; i < Header.NumberOfSections; i++)
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

				br.Close();
			}
		}

		// Tools
		public uint LabelChecksum(string label)
		{
			uint group = 0;

			for (int i = 0; i < label.Length; i++)
			{
				group *= 0x492;
				group += label[i];
				group &= 0xFFFFFFFF;
			}

			return group % LBL1.NumberOfGroups;
		}

		// Reading
		private void ReadLBL1(BinaryReaderX br)
		{
			LBL1.Identifier = br.ReadString(4);
			LBL1.SectionSize = br.ReadUInt32();
			LBL1.Padding1 = br.ReadBytes(8);
			long startOfLabels = br.BaseStream.Position;
			LBL1.NumberOfGroups = br.ReadUInt32();

			for (int i = 0; i < LBL1.NumberOfGroups; i++)
			{
				Group grp = new Group();
				grp.NumberOfLabels = br.ReadUInt32();
				grp.Offset = br.ReadUInt32();
				LBL1.Groups.Add(grp);
			}

			foreach (Group grp in LBL1.Groups)
			{
				br.BaseStream.Seek(startOfLabels + grp.Offset, SeekOrigin.Begin);

				for (int i = 0; i < grp.NumberOfLabels; i++)
				{
					Label lbl = new Label();
					lbl.Length = Convert.ToUInt32(br.ReadByte());
					lbl.Name = br.ReadString((int)lbl.Length);
					lbl.Index = br.ReadUInt32();
					lbl.Checksum = (uint)LBL1.Groups.IndexOf(grp);
					LBL1.Labels.Add(lbl);
				}
			}

			// Old rename correction
			foreach (Label lbl in LBL1.Labels)
			{
				uint previousChecksum = lbl.Checksum;
				lbl.Checksum = LabelChecksum(lbl.Name);

				if (previousChecksum != lbl.Checksum)
				{
					LBL1.Groups[(int)previousChecksum].NumberOfLabels -= 1;
					LBL1.Groups[(int)lbl.Checksum].NumberOfLabels += 1;
				}
			}

			if (LBL1.Labels.Count > 0)
				HasLabels = true;

			PaddingSeek(br);
		}

		private void ReadNLI1(BinaryReaderX br)
		{
			NLI1.Identifier = br.ReadString(4);
			NLI1.SectionSize = br.ReadUInt32();
			NLI1.Padding1 = br.ReadBytes(8);
			NLI1.Unknown2 = br.ReadBytes((int)NLI1.SectionSize); // Read in the entire section at once since we don't know what it's for

			PaddingSeek(br);
		}

		private void ReadATO1(BinaryReaderX br)
		{
			ATO1.Identifier = br.ReadString(4);
			ATO1.SectionSize = br.ReadUInt32();
			ATO1.Padding1 = br.ReadBytes(8);
			ATO1.Unknown2 = br.ReadBytes((int)ATO1.SectionSize); // Read in the entire section at once since we don't know what it's for
		}

		private void ReadATR1(BinaryReaderX br)
		{
			ATR1.Identifier = br.ReadString(4);
			ATR1.SectionSize = br.ReadUInt32();
			ATR1.Padding1 = br.ReadBytes(8);
			ATR1.NumberOfAttributes = br.ReadUInt32();
			ATR1.Unknown2 = br.ReadBytes((int)ATR1.SectionSize - sizeof(uint)); // Read in the entire section at once since we don't know what it's for

			PaddingSeek(br);
		}

		private void ReadTSY1(BinaryReaderX br)
		{
			TSY1.Identifier = br.ReadString(4);
			TSY1.SectionSize = br.ReadUInt32();
			TSY1.Padding1 = br.ReadBytes(8);
			TSY1.Unknown2 = br.ReadBytes((int)TSY1.SectionSize); // Read in the entire section at once since we don't know what it's for

			PaddingSeek(br);
		}

		private void ReadTXT2(BinaryReaderX br)
		{
			TXT2.Identifier = br.ReadString(4);
			TXT2.SectionSize = br.ReadUInt32();
			TXT2.Padding1 = br.ReadBytes(8);
			long startOfStrings = br.BaseStream.Position;
			TXT2.NumberOfStrings = br.ReadUInt32();

			List<uint> offsets = new List<uint>();
			for (int i = 0; i < TXT2.NumberOfStrings; i++)
				offsets.Add(br.ReadUInt32());
			for (int i = 0; i < TXT2.NumberOfStrings; i++)
			{
				String str = new String();
				uint nextOffset = (i + 1 < offsets.Count) ? ((uint)startOfStrings + offsets[i + 1]) : ((uint)startOfStrings + TXT2.SectionSize);

				br.BaseStream.Seek(startOfStrings + offsets[i], SeekOrigin.Begin);

				List<byte> result = new List<byte>();
				while (br.BaseStream.Position < nextOffset - (FileEncoding == Encoding.UTF8 ? 1 : 2) && br.BaseStream.Position < Header.FileSize)
				{
					if (Header.EncodingByte == EncodingByte.UTF8)
						result.Add(br.ReadByte());
					else
					{
						byte[] unichar = br.ReadBytes(2);

						if (br.ByteOrder == ByteOrder.BigEndian)
							Array.Reverse(unichar);

						result.AddRange(unichar);
					}
				}
				str.Text = result.ToArray();
				str.Index = (uint)i;
				TXT2.Strings.Add(str);
			}

			br.BaseStream.Seek((FileEncoding == Encoding.UTF8 ? 1 : 2), SeekOrigin.Current);

			// Tie in LBL1 labels
			foreach (Label lbl in LBL1.Labels)
				lbl.String = TXT2.Strings[(int)lbl.Index];

			PaddingSeek(br);
		}

		private void PaddingSeek(BinaryReaderX br)
		{
			long remainder = br.BaseStream.Position % 16;
			if (remainder > 0)
			{
				paddingChar = br.ReadByte();
				br.BaseStream.Seek(-1, SeekOrigin.Current);
				br.BaseStream.Seek(16 - remainder, SeekOrigin.Current);
			}
		}

		// Manipulation
		public Label AddLabel(string name)
		{
			String nstr = new String();
			nstr.Text = new byte[] { };
			TXT2.Strings.Add(nstr);

			Label nlbl = new Label();
			nlbl.Length = (uint)name.Trim().Length;
			nlbl.Name = name.Trim();
			nlbl.Index = (uint)TXT2.Strings.IndexOf(nstr);
			nlbl.Checksum = LabelChecksum(name.Trim());
			nlbl.String = nstr;
			LBL1.Labels.Add(nlbl);

			LBL1.Groups[(int)nlbl.Checksum].NumberOfLabels += 1;
			ATR1.NumberOfAttributes += 1;
			TXT2.NumberOfStrings += 1;

			return nlbl;
		}

		public void RenameLabel(Label label, string newName)
		{
			label.Length = (uint)Encoding.ASCII.GetBytes(newName.Trim()).Length;
			label.Name = newName.Trim();
			LBL1.Groups[(int)label.Checksum].NumberOfLabels -= 1;
			label.Checksum = LabelChecksum(newName.Trim());
			LBL1.Groups[(int)label.Checksum].NumberOfLabels += 1;
		}

		public void RemoveLabel(Label label)
		{
			int textIndex = TXT2.Strings.IndexOf(label.String);
			for (int i = 0; i < TXT2.NumberOfStrings; i++)
				if (LBL1.Labels[i].Index > textIndex)
					LBL1.Labels[i].Index -= 1;

			LBL1.Groups[(int)label.Checksum].NumberOfLabels -= 1;
			LBL1.Labels.Remove(label);
			ATR1.NumberOfAttributes -= 1;
			TXT2.Strings.RemoveAt((int)label.Index);
			TXT2.NumberOfStrings -= 1;
		}

		// Saving
		public bool Save(string filename)
		{
			bool result = false;

			try
			{
				using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					BinaryWriterX bw = new BinaryWriterX(fs);

					// Byte Order
					bw.ByteOrder = Header.ByteOrderMark[0] > Header.ByteOrderMark[1] ? ByteOrder.LittleEndian : ByteOrder.BigEndian;

					// Header
					bw.WriteASCII(Header.Identifier);
					bw.Write(Header.ByteOrderMark);
					bw.Write(Header.Unknown1);
					bw.Write((byte)Header.EncodingByte);
					bw.Write(Header.Unknown2);
					bw.Write(Header.NumberOfSections);
					bw.Write(Header.Unknown3);
					bw.Write(Header.FileSize);
					bw.Write(Header.Unknown4);

					foreach (string section in SectionOrder)
					{
						if (section == "LBL1")
							WriteLBL1(bw);
						else if (section == "NLI1")
							WriteNLI1(bw);
						else if (section == "ATO1")
							WriteATO1(bw);
						else if (section == "ATR1")
							WriteATR1(bw);
						else if (section == "TSY1")
							WriteTSY1(bw);
						else if (section == "TXT2")
							WriteTXT2(bw);
					}

					// Update FileSize
					long fileSize = bw.BaseStream.Position;
					bw.BaseStream.Seek(Header.FileSizeOffset, SeekOrigin.Begin);
					bw.Write((uint)fileSize);

					bw.Close();

					result = true;
				}
			}
			catch (Exception)
			{ }

			return result;
		}

		private bool WriteLBL1(BinaryWriterX bw)
		{
			bool result = false;

			try
			{
				// Calculate Section Size
				uint newSize = sizeof(uint);

				foreach (Group grp in LBL1.Groups)
					newSize += (uint)(sizeof(uint) + sizeof(uint));

				foreach (Label lbl in LBL1.Labels)
					newSize += (uint)(sizeof(byte) + lbl.Name.Length + sizeof(uint));

				// Calculate Group Offsets
				uint offsetsLength = LBL1.NumberOfGroups * sizeof(uint) * 2 + sizeof(uint);
				uint runningTotal = 0;
				for (int i = 0; i < LBL1.Groups.Count; i++)
				{
					LBL1.Groups[i].Offset = offsetsLength + runningTotal;
					foreach (Label lbl in LBL1.Labels)
						if (lbl.Checksum == i)
							runningTotal += (uint)(sizeof(byte) + lbl.Name.Length + sizeof(uint));
				}

				// Write
				bw.WriteASCII(LBL1.Identifier);
				bw.Write(newSize);
				bw.Write(LBL1.Padding1);
				bw.Write(LBL1.NumberOfGroups);

				foreach (Group grp in LBL1.Groups)
				{
					bw.Write(grp.NumberOfLabels);
					bw.Write(grp.Offset);
				}

				foreach (Group grp in LBL1.Groups)
				{
					foreach (Label lbl in LBL1.Labels)
					{
						if (lbl.Checksum == LBL1.Groups.IndexOf(grp))
						{
							bw.Write(Convert.ToByte(lbl.Length));
							bw.WriteASCII(lbl.Name);
							bw.Write(lbl.Index);
						}
					}
				}

				PaddingWrite(bw);

				result = true;
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		private bool WriteNLI1(BinaryWriterX bw)
		{
			bool result = false;

			try
			{
				bw.WriteASCII(NLI1.Identifier);
				bw.Write(NLI1.SectionSize);
				bw.Write(NLI1.Padding1);
				bw.Write(NLI1.Unknown2);

				PaddingWrite(bw);

				result = true;
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		private bool WriteATO1(BinaryWriterX bw)
		{
			bool result = false;

			try
			{
				bw.WriteASCII(ATO1.Identifier);
				bw.Write(ATO1.SectionSize);
				bw.Write(ATO1.Padding1);
				bw.Write(ATO1.Unknown2);

				result = true;
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		private bool WriteATR1(BinaryWriterX bw)
		{
			bool result = false;

			try
			{
				bw.WriteASCII(ATR1.Identifier);
				bw.Write(ATR1.SectionSize);
				bw.Write(ATR1.Padding1);
				bw.Write(ATR1.NumberOfAttributes);
				bw.Write(ATR1.Unknown2);

				PaddingWrite(bw);

				result = true;
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		private bool WriteTSY1(BinaryWriterX bw)
		{
			bool result = false;

			try
			{
				bw.WriteASCII(TSY1.Identifier);
				bw.Write(TSY1.SectionSize);
				bw.Write(TSY1.Padding1);
				bw.Write(TSY1.Unknown2);

				PaddingWrite(bw);

				result = true;
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		private bool WriteTXT2(BinaryWriterX bw)
		{
			bool result = false;

			try
			{
				// Calculate Section Size
				uint newSize = TXT2.NumberOfStrings * sizeof(uint) + sizeof(uint);

				for (int i = 0; i < TXT2.NumberOfStrings; i++)
					newSize += (uint)TXT2.Strings[i].Text.Length + (uint)(Header.EncodingByte == EncodingByte.UTF8 ? 1 : 2);

				bw.WriteASCII(TXT2.Identifier);
				bw.Write(newSize);
				bw.Write(TXT2.Padding1);
				long startOfStrings = bw.BaseStream.Position;
				bw.Write(TXT2.NumberOfStrings);

				List<uint> offsets = new List<uint>();
				uint offsetsLength = TXT2.NumberOfStrings * sizeof(uint) + sizeof(uint);
				uint runningTotal = 0;
				for (int i = 0; i < TXT2.NumberOfStrings; i++)
				{
					offsets.Add(offsetsLength + runningTotal);
					runningTotal += (uint)TXT2.Strings[i].Text.Length + (uint)(Header.EncodingByte == EncodingByte.UTF8 ? 1 : 2);
				}
				for (int i = 0; i < TXT2.NumberOfStrings; i++)
					bw.Write(offsets[i]);
				for (int i = 0; i < TXT2.NumberOfStrings; i++)
				{
					if (Header.EncodingByte == EncodingByte.UTF8)
					{
						bw.Write(TXT2.Strings[i].Text);
						bw.Write((byte)0x0);
					}
					else
					{
						if (Header.ByteOrderMark[0] == 0xFF)
							bw.Write(TXT2.Strings[i].Text);
						else
							for (int j = 0; j < TXT2.Strings[i].Text.Length; j += 2)
							{
								bw.Write(TXT2.Strings[i].Text[j + 1]);
								bw.Write(TXT2.Strings[i].Text[j]);
							}
						bw.Write(new byte[] { 0x0, 0x0 });
					}
				}

				PaddingWrite(bw);

				result = true;
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		private void PaddingWrite(BinaryWriterX bw)
		{
			long remainder = bw.BaseStream.Position % 16;
			if (remainder > 0)
				for (int i = 0; i < 16 - remainder; i++)
					bw.Write(paddingChar);
		}
	}
}