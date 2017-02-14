using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Cetera.Compression;
using KuriimuContract;

namespace file_ttbin
{
	public sealed class TTBIN
	{
		public List<Label> Labels = new List<Label>();

		public List<EditorStruct> editPlaceholder = new List<EditorStruct>();
		public List<TextStruct> textPlaceholder = new List<TextStruct>();
		public List<int> textPlaceholderCount = new List<int>();
		public List<int> editPlaceholderCount = new List<int>();
		public List<int> textCountList = new List<int>();
		public Header cfgHeader;
		public byte[] rest;

		public byte type = 0;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct XpckHeader
		{
			public Magic magic;
			public byte fileCount;
			public byte unk1;
			public short fileInfoOffset;
			public short filenameTableOffset;
			public short dataOffset;
			public short fileInfoSize;
			public short filenameTableSize;
			public uint dataSize;

			public void correctHeader()
			{
				fileInfoOffset *= 4;
				filenameTableOffset *= 4;
				dataOffset *= 4;
				fileInfoSize *= 4;
				filenameTableSize *= 4;
				dataSize *= 4;
			}
		}
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct XpckEntry
		{
			public uint crc32;
			public uint merged1;
			public int fileSizeTmp;

			public uint offset => (merged1 >> 16) * 4;
			public int fileSize => (fileSizeTmp & 0xFFFF) | (((fileSizeTmp >> 16) % 8) << 8 | ((fileSizeTmp >> 16) / 8));
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Header
		{
			public uint unk1;
			public uint dataOffset;
			public uint dataLength;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct pckEntryStruct
		{
			public uint ID;
			public uint offset;
			public uint length;
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
			public uint unk3;
		}

		public byte[] Identify(BinaryReaderX br)
		{
			//possible identifications: PCK, cfg.bin, XPCK-Archive
			//if cfg.bin
			br.BaseStream.Position = 0x18;
			uint t1 = br.ReadUInt32();
			br.BaseStream.Position = 0x24;
			uint t2 = br.ReadUInt32();
			br.BaseStream.Position = 0;
			if (t1 == 0x0 && t2 == 0x14)
			{
				type = 1;
				return null;
			}

			//if PCK
			int firstOff = br.ReadInt32();
			if (firstOff < br.BaseStream.Length)
			{
				br.BaseStream.Position = br.ReadInt32() * 3 * 4 + 4;
				if (br.ReadByte() == 0x64)
				{
					type = 2;
					return null;
				}
			}
			br.BaseStream.Position = 0;

			//if XPCK
			if (br.ReadString(4) == "XPCK")
			{
				type = 3;
				return null;
			}
			else
			{
				br.BaseStream.Position = 0;
				byte[] result = CriWare.GetDecompressedBytes(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length)));
				using (BinaryReaderX br2 = new BinaryReaderX(new MemoryStream(result)))
				{
					if (br2.ReadString(4) == "XPCK")
					{
						type = 3;
						return result;
					}
				}
			}

			return null;
		}

		public TTBIN(string filename)
		{
			//Identify type
			using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				byte[] decomp = Identify(br);
				br.BaseStream.Position = 0;

				switch (type)
				{
					case 1: //cfg.bin
						extractCfgBin(br);
						break;
					case 2: //PCK
						extractPck(br);
						break;
					case 3: //XPCK
						if (decomp == null)
						{
							extractXpck(br);
						}
						else
						{
							extractXpck(new BinaryReaderX(new MemoryStream(decomp)));
						}
						break;
					default:
						throw new Exception("Unsupported binary format!");
				}
			}
		}

		public void extractXpck(BinaryReaderX br)
		{
			var header = br.ReadStruct<XpckHeader>();
			header.correctHeader();

			long bk = br.BaseStream.Position;
			br.BaseStream.Position = header.filenameTableOffset;
			byte[] filenameTable = CriWare.GetDecompressedBytes(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length)));
			//File.OpenWrite("nametable.bin").Write(filenameTable, 0, filenameTable.Length);
			br.BaseStream.Position = bk;
			int count = 0;
			using (BinaryReaderX br2 = new BinaryReaderX(new MemoryStream(filenameTable)))
			{
				List<XpckEntry> xpckEntries = new List<XpckEntry>();
				for (int i = 0; i < header.fileCount; i++)
				{
					xpckEntries.Add(br.ReadStruct<XpckEntry>());
					bk = br.BaseStream.Position;
					string name = getFileName(br2);

					if (name.Contains("cfg.bin"))
					{
						br.BaseStream.Position = xpckEntries[i].offset + header.dataOffset;
						byte[] cont = br.ReadBytes(xpckEntries[i].fileSize);
						File.OpenWrite(name).Write(cont, 0, cont.Length);
						//extractCfgBin(new BinaryReaderX(new MemoryStream(cont)), "XPCK" + count.ToString());
						count += xpckEntries[i].fileSize;
						br.BaseStream.Position = bk;
					}
				}
				throw new Exception(count.ToString());
			}
		}

		public string getFileName(BinaryReaderX br)
		{
			Encoding ascii = Encoding.GetEncoding("ascii");
			byte[] entry = { br.ReadByte() };
			string name = "";
			while (entry[0] != 0x00)
			{
				name += ascii.GetString(entry);
				entry[0] = br.ReadByte();
			}
			return name;
		}

		public void extractPck(BinaryReaderX br)
		{
			int pckEntryCount = br.ReadInt32();
			for (int i = 0; i < pckEntryCount; i++)
			{
				textCountList.Add(0);
				var entry = br.ReadStruct<pckEntryStruct>();
				br.BaseStream.Position = entry.offset + 2;
				int blockCountCheck = br.ReadUInt16();
				br.BaseStream.Position += blockCountCheck * 4;
				using (BinaryReaderX br2 = new BinaryReaderX(new MemoryStream(br.ReadBytes((int)entry.length - (blockCountCheck * 4 + 4)))))
				{
					extractCfgBin(br2, "pckEntry" + i.ToString() + "/");
				}
				br.BaseStream.Position = 4 + (i + 1) * 3 * 4;
			}
		}

		public void extractCfgBin(BinaryReaderX br, string prefix = "")
		{
			//Header
			cfgHeader = br.ReadStruct<Header>();
			editPlaceholder.Add(br.ReadStruct<EditorStruct>());
			int editCount = 1;
			EditorStruct entryE;

			//getting Editor's notes entries
			int count = 0;
			while (true)
			{
				Label label = new Label();
				entryE = br.ReadStruct<EditorStruct>();
				editPlaceholder.Add(entryE);
				editCount++;
				string text = "";

				label.Name = prefix + "editor" + count.ToString(); count++;
				label.TextID = entryE.ID;
				label.TextOffset = cfgHeader.dataOffset + entryE.entryOffset;

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
			editPlaceholderCount.Add(editCount);
			textPlaceholder.Add(br.ReadStruct<TextStruct>());
			int textCount = 1;

			if (br.ReadUInt32() > br.BaseStream.Position) return;
			br.BaseStream.Position -= 0x4;

			count = 0;

			//getting text entries
			TextStruct entry; TextStruct entry2;
			do
			{
				Label label = new Label();

				if (prefix != "") textCountList[textCountList.Count - 1]++;

				entry = br.ReadStruct<TextStruct>();
				textPlaceholder.Add(entry);
				textCount++;
				entry2 = br.ReadStruct<TextStruct>();
				br.BaseStream.Position -= 0x14;

				label.Name = prefix + "text" + count.ToString();
				count += 1;
				label.TextID = entry.ID;
				label.TextOffset = cfgHeader.dataOffset + entry.entryOffset;

				long posBk = br.BaseStream.Position;
				br.BaseStream.Position = label.TextOffset;
				long textSize = (entry.unk3 == 0xffffff00 || entry2.entryOffset > br.BaseStream.Length) ? br.BaseStream.Length - label.TextOffset : cfgHeader.dataOffset + (entry2.entryOffset - entry.entryOffset);
				label.Text = getUnicodeString(new BinaryReaderX(new MemoryStream(br.ReadBytes((int)textSize))));
				br.BaseStream.Position = posBk;
				Labels.Add(label);
			} while (entry.unk3 != 0xffffff00 && entry2.entryOffset <= br.BaseStream.Length);

			//fill textEntries to max
			while (br.BaseStream.Position + 0x14 <= cfgHeader.dataOffset)
			{
				textPlaceholder.Add(br.ReadStruct<TextStruct>());
				textCount++;
			}
			textPlaceholderCount.Add(textCount);
			if (br.BaseStream.Position != cfgHeader.dataOffset) rest = br.ReadBytes((int)cfgHeader.dataOffset - (int)br.BaseStream.Position);
		}

		public static string getUnicodeString(BinaryReaderX br)
		{
			Encoding sjis = Encoding.GetEncoding("shift-jis");
			string uni = ""; byte part;

			do
			{
				part = br.ReadByte();
				uni += (part >= 0x80) ? sjis.GetString(new byte[] { part, br.ReadByte() }) : (part > 0x00) ? sjis.GetString(new byte[] { part }) : "";
			} while (part != 0x00);
			return uni;
		}

		public void Save(string filename)
		{
			using (BinaryWriterX br = new BinaryWriterX(File.Open(filename, FileMode.Open, FileAccess.Write, FileShare.Write)))
			{
				br.WriteStruct<Header>(cfgHeader);
				int labelCount = 0;
				for (int i = 0; i < textPlaceholderCount.Count; i++)
				{
					for (int j = 0; j < editPlaceholderCount[i]; j++)
					{
						br.WriteStruct<EditorStruct>(editPlaceholder[j]);
					}
					for (int j = 0; j < textPlaceholderCount[i]; j++)
					{
						br.WriteStruct<TextStruct>(textPlaceholder[j]);
					}
					br.Write(rest);

					for (int j = 0; j < textCountList[i]; j++)
					{
						br.Write(Encoding.GetEncoding("shift-jis").GetBytes(Labels[labelCount++].Text));
						br.Write(0x00);
					}

					throw new Exception("Test");
				}
				br.Close();
			}
		}
	}
}