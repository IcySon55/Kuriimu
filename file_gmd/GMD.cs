using KuriimuContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace file_gmd
{
	public sealed class GMD
	{
		public Header Header = new Header();
		public List<Label> Labels = new List<Label>();
		private byte[] Unknown1024 = null;

		public Encoding FileEncoding = Encoding.UTF8;

		public GMD(string filename)
		{
			using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReaderX br = new BinaryReaderX(fs);

				// Header
				Header.Identifier = br.ReadString(4);
				if (Header.Identifier != "GMD")
					throw new InvalidGMDException("The file provided is not a valid GMD file.");

				Header.Unknown1 = br.ReadBytes(8);
				Header.Unknown2 = br.ReadUInt32();
				Header.Unknown3 = br.ReadBytes(4);
				Header.NumberOfLabels = br.ReadUInt32();
				Header.NumberOfOffsets = br.ReadUInt32();
				Header.Unknown4 = br.ReadBytes(4);
				Header.DataSize = br.ReadUInt32();
				Header.NameLength = br.ReadUInt32();
				Header.Name = br.ReadString((int)Header.NameLength + 1);

				// Read in the label metadata
				for (int i = 0; i < Header.NumberOfLabels; i++)
				{
					Label label = new Label();
					label.ID = br.ReadUInt32();
					label.Unknown1 = br.ReadUInt32();
					label.Unknown2 = br.ReadUInt32();
					label.Unknown3 = br.ReadUInt32();
					label.Unknown4 = br.ReadUInt32();
					Labels.Add(label);
				}

				// Read in the 1 KB unknown data block
				Unknown1024 = br.ReadBytes(0x400);

				// Read in the label names
				foreach (Label label in Labels)
					label.Name = br.ReadASCIIStringUntil(0x0);

				// Read in the text data
				foreach (Label label in Labels)
					label.Text = br.ReadBytesUntil(0x0);

				br.Close();
			}
		}

		// Manipulation
		//TODO: Manipulation functions

		// Saving
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
					bw.Write(Header.Unknown1);
					bw.Write(Header.Unknown2);
					bw.Write(Header.Unknown3);
					bw.Write(Header.NumberOfLabels);
					bw.Write(Header.NumberOfOffsets);
					bw.Write(Header.Unknown4);
					long dataSizeOffset = bw.BaseStream.Position;
					bw.Write(Header.DataSize);
					bw.Write(Header.NameLength);
					bw.WriteASCII(Header.Name + "\0");

					foreach (Label label in Labels)
					{
						bw.Write(label.ID);
						bw.Write(label.Unknown1);
						bw.Write(label.Unknown2);
						bw.Write(label.Unknown3);
						bw.Write(label.Unknown4);
					}

					bw.Write(Unknown1024);

					// Read in the label names
					foreach (Label label in Labels)
					{
						bw.WriteASCII(label.Name);
						bw.Write(0x0);
					}

					// Read in the text data
					uint dataSize = 0;
					foreach (Label label in Labels)
					{
						bw.Write(label.Text);
						bw.Write(0x0);
						dataSize += (uint)label.Text.Length + 1;
					}

					// Update DataSize
					bw.BaseStream.Seek(dataSizeOffset, SeekOrigin.Begin);
					bw.Write(dataSize);

					bw.Close();
				}
			}
			catch (Exception)
			{ }

			return result;
		}
	}
}