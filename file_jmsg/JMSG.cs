using KuriimuContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace file_jmsg
{
	public sealed class JMSG
	{
		public Header Header = new Header();
		public List<Label> Labels = new List<Label>();
		public Encoding FileEncoding = Encoding.Unicode;

		public JMSG(string filename)
		{
			using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReaderX br = new BinaryReaderX(fs);

				// Header
				Header.Identifier = br.ReadString(4);
				if (Header.Identifier != "jMSG")
					throw new InvalidJMSGException("The file provided is not a valid JMSG file.");

				Header.FileSize = br.ReadUInt32();

				if (Header.FileSize != br.BaseStream.Length)
					throw new InvalidJMSGException("The file provided is not a valid JMSG file. Filesize mismtach.");

				Header.NumberOfEntries = br.ReadUInt32();
				Header.Version = br.ReadUInt32();
				Header.HasLabels = br.ReadUInt32(); // 0x01 || 0x0101
				Header.LabelsOffset = br.ReadUInt32() + Header.Size;

				// Text Offsets
				for (int i = 0; i < Header.NumberOfEntries; i++)
				{
					Label label = new Label();
					label.TextID = br.ReadUInt32();
					label.TextOffset = br.ReadUInt32() + Header.Size;
					Labels.Add(label);
				}

				// Extra Offsets
				for (int i = 0; i < Header.NumberOfEntries; i++)
				{
					Label label = Labels[i];
					label.ExtraID = br.ReadUInt32();
					label.ExtraOffset = br.ReadUInt32() + Header.Size;
				}

				// Text
				for (int i = 0; i < Header.NumberOfEntries; i++)
				{
					Label label = Labels[i];

					List<byte> result = new List<byte>();
					br.BaseStream.Seek(label.TextOffset, SeekOrigin.Begin);

					byte[] unichar = br.ReadBytes(2);
					while ((unichar[0] != 0x0 || unichar[1] != 0x0) && br.BaseStream.Position < br.BaseStream.Length)
					{
						result.AddRange(unichar);

						unichar = br.ReadBytes(2);

						if (br.ByteOrder == ByteOrder.BigEndian)
							Array.Reverse(unichar);
					}

					label.Text = result.ToArray();

					// Extra
					result.Clear();
					br.BaseStream.Seek(label.ExtraOffset, SeekOrigin.Begin);

					unichar = br.ReadBytes(2);
					while ((unichar[0] != 0x0 || unichar[1] != 0x0) && br.BaseStream.Position < br.BaseStream.Length)
					{
						result.AddRange(unichar);

						unichar = br.ReadBytes(2);

						if (br.ByteOrder == ByteOrder.BigEndian)
							Array.Reverse(unichar);
					}

					label.Extra = Encoding.Unicode.GetString(result.ToArray());
				}

				// Labels
				if (Header.HasLabels == 0x0101)
				{
					br.BaseStream.Seek(Header.LabelsOffset, SeekOrigin.Begin);

					// Label Offsets
					for (int i = 0; i < Header.NumberOfEntries; i++)
					{
						Label label = Labels[i];
						label.NameOffset = br.ReadUInt32() + Header.Size;
					}

					// Label Names
					for (int i = 0; i < Header.NumberOfEntries; i++)
					{
						Label label = Labels[i];

						List<byte> result = new List<byte>();
						br.BaseStream.Seek(label.NameOffset, SeekOrigin.Begin);

						byte unichar = br.ReadByte();
						while (unichar != 0x0 && br.BaseStream.Position < br.BaseStream.Length)
						{
							result.Add(unichar);

							unichar = br.ReadByte();
						}

						label.Name = Encoding.ASCII.GetString(result.ToArray());
					}
				}

				br.Close();
			}
		}

		public bool Save(string filename)
		{
			bool result = false;

			try
			{
				using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					BinaryWriterX bw = new BinaryWriterX(fs);

					// Header
					bw.WriteASCII(Header.Identifier);
					uint fileSizeOffset = (uint)bw.BaseStream.Position;
					bw.Write(Header.FileSize);
					bw.Write(Header.NumberOfEntries);
					bw.Write(Header.Version);
					bw.Write(Header.HasLabels);
					uint labelsOffset = (uint)bw.BaseStream.Position;
					bw.Write(Header.LabelsOffset - Header.Size);

					uint entryStart = Header.Size;
					uint textStart = (uint)bw.BaseStream.Position + (uint)(Labels.Count * 2 * 8);

					// Text
					bw.BaseStream.Seek(textStart, SeekOrigin.Begin);
					for (int i = 0; i < Header.NumberOfEntries; i++)
					{
						Label label = Labels[i];
						label.TextOffset = (uint)bw.BaseStream.Position - Header.Size;
						bw.Write(label.Text);
						bw.Write(new byte[] { 0x0, 0x0 });
					}

					// Extra
					for (int i = 0; i < Header.NumberOfEntries; i++)
					{
						Label label = Labels[i];
						label.ExtraOffset = (uint)bw.BaseStream.Position - Header.Size;
						bw.Write(FileEncoding.GetBytes(label.Extra));
						bw.Write(new byte[] { 0x0, 0x0 });
					}

					// Pad to the nearest 8 bytes
					PaddingWrite(bw);

					// Set label offset variables
					uint labelsOffsets = (uint)bw.BaseStream.Position;
					uint labelsStart = (uint)bw.BaseStream.Position + (uint)(Labels.Count * 4);

					// Grab the new LabelsOffset
					if (Header.HasLabels == 0x0101)
						Header.LabelsOffset = (uint)bw.BaseStream.Position - Header.Size;
					else
						Header.LabelsOffset = 0;

					// Text Offsets
					bw.BaseStream.Seek(entryStart, SeekOrigin.Begin);
					for (int i = 0; i < Header.NumberOfEntries; i++)
					{
						Label label = Labels[i];
						bw.Write(label.TextID);
						bw.Write(label.TextOffset);
					}
					// Extra Offsets
					for (int i = 0; i < Header.NumberOfEntries; i++)
					{
						Label label = Labels[i];
						bw.Write(label.ExtraID);
						bw.Write(label.ExtraOffset);
					}

					// Labels
					if (Header.HasLabels == 0x0101)
					{
						// Label Names
						bw.BaseStream.Seek(labelsStart, SeekOrigin.Begin);
						for (int i = 0; i < Header.NumberOfEntries; i++)
						{
							Label label = Labels[i];
							label.NameOffset = (uint)bw.BaseStream.Position - Header.Size;
							bw.WriteASCII(label.Name);
							bw.Write((byte)0x0);
						}

						// Pad to the nearest 8 bytes
						PaddingWrite(bw);
						// Grab the new filesize
						Header.FileSize = (uint)bw.BaseStream.Position;

						// Label Offsets
						bw.BaseStream.Seek(labelsOffsets, SeekOrigin.Begin);
						for (int i = 0; i < Header.NumberOfEntries; i++)
							bw.Write(Labels[i].NameOffset);
					}

					// Update LabelsOffset
					bw.BaseStream.Seek(labelsOffset, SeekOrigin.Begin);
					bw.Write(Header.LabelsOffset);

					// Update FileSize
					bw.BaseStream.Seek(fileSizeOffset, SeekOrigin.Begin);
					bw.Write(Header.FileSize);

					bw.Close();
				}

				result = true;
			}
			catch (Exception)
			{ }

			return result;
		}

		private void PaddingWrite(BinaryWriterX bw, int alignment = 8)
		{
			long remainder = bw.BaseStream.Position % alignment;
			if (remainder > 0)
				for (int i = 0; i < alignment - remainder; i++)
					bw.Write((byte)0x0);
		}
	}
}