using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KuriimuContract;

namespace file_ttbin
{
	public sealed class TTBIN
	{
		public List<Label> Labels = new List<Label>();
		public string text;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct Header
		{
			public uint entryCount;
			public uint dataOffset;
			public uint dataLength;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct EditorStruct
		{
			public uint entryOffset;
			public uint ID;
			public ushort endingFlag;
			public ushort filling;
		}
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct TextStruct
		{
			public uint entryOffset;
			public uint ID;
			public uint unk1;
			public uint unk2;
			public uint entryNr;
		}

		public TTBIN(string filename)
		{
			using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				//Header
				var header = br.ReadStruct<Header>();
				br.BaseStream.Position += 0xc;
				EditorStruct entryE;

				//getting Editor's notes entries
				int count = 0;
				while (true)
				{
					Label label = new Label();
					entryE = br.ReadStruct<EditorStruct>();
					text = "";

					label.Name = "editor" + count.ToString(); count++;
					label.TextEncoding = Encoding.ASCII;
					label.TextID = entryE.ID;
					label.TextOffset = header.dataOffset + entryE.entryOffset;

					long posBk = br.BaseStream.Position;
					br.BaseStream.Position = label.TextOffset;
					while (true)
					{
						byte part = br.ReadByte();
						if (part == 0x00) break;

						text += (char)part;
					}
					label.Text = text;
					br.BaseStream.Position = posBk;

					Labels.Add(label);

					if (entryE.endingFlag == 0x0101)
					{
						break;
					}
				}
				br.BaseStream.Position += 0x14;

				count = 0;

				//getting text entries
				TextStruct entry; TextStruct entry2;
				do
				{
					Label label = new Label();

					entry = br.ReadStruct<TextStruct>();
					entry2 = br.ReadStruct<TextStruct>();
					br.BaseStream.Position -= 0x14;

					label.Name = "text" + count.ToString();
					count += 1;
					label.TextID = entry.ID;
					label.TextOffset = header.dataOffset + entry.entryOffset;

					long posBk = br.BaseStream.Position;
					br.BaseStream.Position = label.TextOffset;
					long textSize = (entry.entryNr == 0xffffff00) ? br.BaseStream.Length - label.TextOffset : header.dataOffset + (entry2.entryOffset - entry.entryOffset);
					label.Text = getUnicodeString(new BinaryReaderX(new MemoryStream(br.ReadBytes((int)textSize))));
					br.BaseStream.Position = posBk;
					Labels.Add(label);
				} while (entry.entryNr != 0xffffff00);

				br.Close();
			}
		}

		public static string getUnicodeString(BinaryReaderX br)
		{
			Encoding sjis = Encoding.GetEncoding("shift-jis");
			string uni = ""; byte part;

			do
			{
				part = br.ReadByte();
				uni += (part >= 0x80) ? sjis.GetString(new byte[] { part, br.ReadByte() }) : sjis.GetString(new byte[] { part });
			} while (part != 0x00);
			return uni;
		}
	}
}