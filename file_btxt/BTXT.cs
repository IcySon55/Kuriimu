using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;

namespace file_btxt
{
	public class BTXT
	{
		public Header Header = new Header();
		public List<uint> LabelOffsets = new List<uint>();
		public List<uint> StringOffsets = new List<uint>();
		public List<Label> Labels = new List<Label>();

		public Encoding FileEncoding = Encoding.Unicode;

		public BTXT(string filename)
		{
			using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReaderX br = new BinaryReaderX(fs);

				// Header
				Header.Identifier = br.ReadBytes(8);
				if (!Header.Identifier.SequenceEqual(new byte[] { 0x0, 0x0, 0x0, 0x0, 0x24, 0x10, 0x12, 0xFF }))
					throw new InvalidBTXTException("The file provided is not a valid BTXT file.");

				Header.NumberOfLabels = br.ReadUInt16();
				Header.NumberOfStrings = br.ReadUInt16();

				// Create enough labels
				for (int i = 0; i < Header.NumberOfLabels; i++)
					Labels.Add(new Label());

				// Attributes
				for (int i = 0; i < Header.NumberOfLabels; i++)
				{
					Label label = Labels[i];
					label.StringCount = br.ReadUInt32();

					for (int j = 0; j < label.StringCount; j++)
					{
						String str = new String();
						str.Attribute2 = br.ReadUInt32();
						label.Strings.Add(str);
					}
				}

				// Label Offsets
				for (int i = 0; i < Header.NumberOfLabels; i++)
					LabelOffsets.Add(br.ReadUInt32());

				// String Offsets
				for (int i = 0; i < Header.NumberOfStrings; i++)
					StringOffsets.Add(br.ReadUInt32());

				// Set the offset start position
				uint offsetStart = (uint)br.BaseStream.Position;

				// Labels
				for (int i = 0; i < Header.NumberOfLabels; i++)
				{
					Label label = Labels[i];
					uint start = offsetStart + LabelOffsets[i];
					uint length = (i + 1 >= LabelOffsets.Count) ? offsetStart + StringOffsets[0] - start : offsetStart + LabelOffsets[i + 1] - start;

					br.BaseStream.Seek(start, SeekOrigin.Begin);
					label.Name = Encoding.ASCII.GetString(StripNulls(br.ReadBytes((int)length)));
				}

				// Text
				int adjust = 0;
				for (int i = 0; i < Header.NumberOfLabels; i++)
				{
					Label label = Labels[i];

					for (int j = 0; j < label.StringCount; j++)
					{
						uint start = offsetStart + StringOffsets[adjust];
						uint length = (adjust + 1 >= StringOffsets.Count) ? (uint)br.BaseStream.Length - start : offsetStart + StringOffsets[adjust + 1] - start;

						br.BaseStream.Seek(start, SeekOrigin.Begin);
						String str = label.Strings[j];
						str.Text = StripNulls(br.ReadBytes((int)length));
						adjust++;
					}
				}

				br.Close();
			}
		}

		private byte[] StripNulls(byte[] input)
		{
			List<byte> result = input.ToList();

			while (result[result.Count - 1] == '\0')
				result.RemoveAt(result.Count - 1);

			return result.ToArray();
		}

		//public bool Save(string filename)
		//{
		//	bool result = false;

		//	try
		//	{
		//		using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
		//		{
		//			BinaryWriterX bw = new BinaryWriterX(fs);

		//			Header
		//			bw.WriteASCII(Header.Identifier);
		//			uint fileSizeOffset = (uint)bw.BaseStream.Position;
		//			bw.Write(Header.FileSize);
		//			bw.Write(Header.NumberOfEntries);
		//			bw.Write(Header.Version);
		//			bw.Write(Header.HasLabels);
		//			uint labelsOffset = (uint)bw.BaseStream.Position;
		//			bw.Write(Header.LabelsOffset - Header.Size);

		//			uint entryStart = Header.Size;
		//			uint textStart = (uint)bw.BaseStream.Position + (uint)(Labels.Count * 2 * 8);

		//			Text
		//			bw.BaseStream.Seek(textStart, SeekOrigin.Begin);
		//			for (int i = 0; i < Header.NumberOfEntries; i++)
		//			{
		//				Label label = Labels[i];
		//				label.TextOffset = (uint)bw.BaseStream.Position - Header.Size;
		//				bw.Write(label.Text);
		//				bw.Write(new byte[] { 0x0, 0x0 });
		//			}

		//			Extra
		//			for (int i = 0; i < Header.NumberOfEntries; i++)
		//			{
		//				Label label = Labels[i];
		//				label.ExtraOffset = (uint)bw.BaseStream.Position - Header.Size;
		//				bw.Write(FileEncoding.GetBytes(label.Extra));
		//				bw.Write(new byte[] { 0x0, 0x0 });
		//			}

		//			Pad to the nearest 8 bytes
		//			PaddingWrite(bw);

		//			Set label offset variables
		//			uint labelsOffsets = (uint)bw.BaseStream.Position;
		//			uint labelsStart = (uint)bw.BaseStream.Position + (uint)(Labels.Count * 4);

		//			Grab the new LabelsOffset
		//			if (Header.HasLabels == 0x0101)
		//				Header.LabelsOffset = (uint)bw.BaseStream.Position - Header.Size;
		//			else
		//				Header.LabelsOffset = 0;

		//			Text Offsets
		//			bw.BaseStream.Seek(entryStart, SeekOrigin.Begin);
		//			for (int i = 0; i < Header.NumberOfEntries; i++)
		//			{
		//				Label label = Labels[i];
		//				bw.Write(label.TextID);
		//				bw.Write(label.TextOffset);
		//			}
		//			Extra Offsets
		//			for (int i = 0; i < Header.NumberOfEntries; i++)
		//			{
		//				Label label = Labels[i];
		//				bw.Write(label.ExtraID);
		//				bw.Write(label.ExtraOffset);
		//			}

		//			Labels
		//			if (Header.HasLabels == 0x0101)
		//			{
		//				Label Names
		//				bw.BaseStream.Seek(labelsStart, SeekOrigin.Begin);
		//				for (int i = 0; i < Header.NumberOfEntries; i++)
		//				{
		//					Label label = Labels[i];
		//					label.NameOffset = (uint)bw.BaseStream.Position - Header.Size;
		//					bw.WriteASCII(label.Name);
		//					bw.Write((byte)0x0);
		//				}

		//				Pad to the nearest 8 bytes
		//				PaddingWrite(bw);
		//				Grab the new filesize
		//				Header.FileSize = (uint)bw.BaseStream.Position;

		//				Label Offsets
		//				bw.BaseStream.Seek(labelsOffsets, SeekOrigin.Begin);
		//				for (int i = 0; i < Header.NumberOfEntries; i++)
		//					bw.Write(Labels[i].NameOffset);
		//			}

		//			Update LabelsOffset
		//			bw.BaseStream.Seek(labelsOffset, SeekOrigin.Begin);
		//			bw.Write(Header.LabelsOffset);

		//			Update FileSize
		//			bw.BaseStream.Seek(fileSizeOffset, SeekOrigin.Begin);
		//			bw.Write(Header.FileSize);

		//			bw.Close();
		//		}

		//		result = true;
		//	}
		//	catch (Exception)
		//	{ }

		//	return result;
		//}

		private void PaddingWrite(BinaryWriterX bw, int alignment = 8)
		{
			long remainder = bw.BaseStream.Position % alignment;
			if (remainder > 0)
				for (int i = 0; i < alignment - remainder; i++)
					bw.Write((byte)0x0);
		}
	}
}