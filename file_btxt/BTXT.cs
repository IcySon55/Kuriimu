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
		public List<Object1> Object1s = new List<Object1>();
		public List<uint> Offsets = new List<uint>();
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

				Header.NumberOfEntries = br.ReadUInt16();
				Header.NumberOfUnknown1 = br.ReadUInt16();

				// Object1
				for (int i = 0; i < Header.NumberOfEntries; i++)
				{
					Object1 obj = new Object1();
					obj.Value1 = br.ReadUInt32();
					obj.Value2 = br.ReadUInt32();
					Object1s.Add(obj);
				}

				br.ReadUInt32(); // 0x00 00 00 00
				Header.LabelLength = br.ReadUInt32();

				// Offsets
				for (int i = 0; i < Header.NumberOfEntries; i++)
				{
					Offsets.Add(br.ReadUInt32());
				}

				// Set the offset start position
				uint offsetStart = (uint)br.BaseStream.Position;

				// Labels
				for (int i = 0; i < Header.NumberOfEntries; i++)
				{
					Label label = new Label();
					label.Name = br.ReadString((int)Header.LabelLength);
					Labels.Add(label);
				}

				// Text
				for (int i = 0; i < Header.NumberOfEntries; i++)
				{
					Label label = Labels[i];
					uint start = offsetStart + Offsets[i];
					uint length = (i + 1 >= Offsets.Count) ? (uint)br.BaseStream.Length - start : offsetStart + Offsets[i + 1] - start;

					br.BaseStream.Seek(start, SeekOrigin.Begin);
					label.Text = br.ReadBytes((int)length);
				}

				br.Close();
			}
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