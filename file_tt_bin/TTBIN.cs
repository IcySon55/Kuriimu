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

		public List<pckEntryStruct> pckEntries = new List<pckEntryStruct>();
		public List<int> pckCrc32Count = new List<int>();

		public List<Header> headerList = new List<Header>();
		public List<List<EditorStruct>> editorEntries = new List<List<EditorStruct>>();
		public List<List<TextStruct>> textEntries = new List<List<TextStruct>>();
		public List<byte[]> editorRest = new List<byte[]>();
		public List<byte[]> textRest = new List<byte[]>();

		public byte type = 0;
		public int labelCount;

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
			int entryCount = br.ReadInt32();
			br.BaseStream.Position = 0x8;
			if (entryCount * 3 * 4 + 4 == br.ReadInt32())
			{
				type = 2;
				return null;
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
						if (decomp == null) extractXpck(br); else extractXpck(new BinaryReaderX(new MemoryStream(decomp)));
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
						//extractCfgBin(new BinaryReaderX(new MemoryStream(cont)), "XPCK" + count.ToString());
						count += xpckEntries[i].fileSize;
						br.BaseStream.Position = bk;
					}
				}
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
				br.BaseStream.Position = 4 + i * 3 * 4;
				var entry = br.ReadStruct<pckEntryStruct>();
				pckEntries.Add(entry);

				br.BaseStream.Position = entry.offset;
				if (br.ReadUInt16() == 0x64)
				{
					int blockCount = br.ReadUInt16();
					pckCrc32Count.Add(blockCount);
					br.BaseStream.Position += blockCount * 4;
					using (BinaryReaderX br2 = new BinaryReaderX(new MemoryStream(br.ReadBytes((int)entry.length - (blockCount * 4 + 4)))))
					{
						extractCfgBin(br2, "pckEntry" + (i + 1).ToString() + "/");
					}
				}
				else
				{
					pckCrc32Count.Add(-1);
					using (BinaryReaderX br2 = new BinaryReaderX(new MemoryStream(br.ReadBytes((int)entry.length))))
					{
						extractCfgBin(br2, "pckEntry" + i.ToString() + "/");
					}
				}
			}
		}

		public void extractCfgBin(BinaryReaderX br, string prefix = "")
		{
			long bk;

			//Header
			Header cfgHeader = br.ReadStruct<Header>();
			headerList.Add(cfgHeader);

			List<EditorStruct> editorEntriesLocal = new List<EditorStruct>();
			editorEntriesLocal.Add(br.ReadStruct<EditorStruct>());

			//getting Editor's notes entries
			while (true)
			{
				Label label = new Label();

				EditorStruct entryE = br.ReadStruct<EditorStruct>();
				editorEntriesLocal.Add(entryE);

				label.Name = prefix + "editor" + (editorEntriesLocal.Count - 1).ToString();
				label.TextID = entryE.ID;
				label.TextOffset = cfgHeader.dataOffset + entryE.entryOffset;

				bk = br.BaseStream.Position;
				br.BaseStream.Position = label.TextOffset;
				string text = ""; byte part = br.ReadByte();
				while (part != 0)
				{
					text += Encoding.GetEncoding("ascii").GetString(new byte[] { part });
					part = br.ReadByte();
				}
				br.BaseStream.Position = label.TextOffset;
				label.Text = text;
				br.BaseStream.Position = bk;

				Labels.Add(label);

				if (entryE.endingFlag == 0x0101)
				{
					break;
				}
			}
			editorEntries.Add(editorEntriesLocal);

			bool found = false;
			bk = br.BaseStream.Position;
			while (br.BaseStream.Position < cfgHeader.dataOffset && found == false)
			{
				if (br.ReadInt32() == (int)(editorEntries[editorEntries.Count - 1][editorEntries[editorEntries.Count - 1].Count - 1].entryOffset + Labels[Labels.Count - 1].Text.Length + 1))
				{
					found = true;
				}
			}

			br.BaseStream.Position = bk;
			if (found == false)
			{
				editorRest.Add(br.ReadBytes((int)(cfgHeader.dataOffset - br.BaseStream.Position)));
				textRest.Add(null);
				textEntries.Add(null);
			}
			else
			{
				editorRest.Add(null);

				List<TextStruct> textEntriesLocal = new List<TextStruct>();

				textEntriesLocal.Add(br.ReadStruct<TextStruct>());

				//getting text entries
				TextStruct entryT;
				TextStruct entryT2;
				int entryCount = 1;
				do
				{
					Label label = new Label();

					entryT = br.ReadStruct<TextStruct>();
					textEntriesLocal.Add(entryT);

					entryT2 = br.ReadStruct<TextStruct>();
					br.BaseStream.Position -= 0x14;

					if (entryT.entryOffset != 0xFFFFFFFF)
					{
						label.Name = prefix + "text" + entryCount.ToString(); entryCount++;
						label.TextID = entryT.ID;
						label.TextOffset = cfgHeader.dataOffset + entryT.entryOffset;

						bk = br.BaseStream.Position;
						br.BaseStream.Position = label.TextOffset;
						int count = 0; byte part = br.ReadByte();
						while (part != 0)
						{
							count++;
							part = br.ReadByte();
						}
						count++;
						br.BaseStream.Position = label.TextOffset;
						label.Text = getUnicodeString(new BinaryReaderX(new MemoryStream(br.ReadBytes(count))));
						br.BaseStream.Position = bk;

						Labels.Add(label);
					}
				} while (entryT.unk3 != 0xffffff00 && (entryT2.entryOffset <= br.BaseStream.Length || entryT2.entryOffset == 0xFFFFFFFF));
				textEntries.Add(textEntriesLocal);

				if (br.BaseStream.Position < cfgHeader.dataOffset) textRest.Add(br.ReadBytes((int)cfgHeader.dataOffset - (int)br.BaseStream.Position));
			}
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
			long bk;
			labelCount = 0;

			if (type == 2)
			{
				using (BinaryWriterX br = new BinaryWriterX(File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)))
				{
					br.Write(pckEntries.Count);
					for (int i = 0; i < pckEntries.Count; i++)
					{
						br.WriteStruct<pckEntryStruct>(pckEntries[i]);
					}
					for (int i = 0; i < pckEntries.Count; i++)
					{
						int offset = (int)br.BaseStream.Length;

						int basis = 0;
						if (pckCrc32Count[i] != -1)
						{
							br.Write((short)0x64);
							br.Write((short)pckCrc32Count[i]);
							br.BaseStream.Position += pckCrc32Count[i] * 4;
							basis = pckCrc32Count[i] * 4 + 4;
						}

						BinaryReaderX br2 = createCfg(i);
						br2.BaseStream.Position = 0;
						br.Write(br2.ReadBytes((int)br2.BaseStream.Length));

						bk = br.BaseStream.Position;
						br.BaseStream.Position = i * 3 * 4 + 4 + 4;
						br.Write(offset);
						br.Write((int)br2.BaseStream.Length + 4 + pckCrc32Count[i] * 4);

						br.BaseStream.Position = bk;
					}
					br.Close();
				}
			}
			else if (type == 1)
			{
				using (BinaryWriterX br = new BinaryWriterX(File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write)))
				{
					BinaryReaderX br2 = createCfg(0);
					br2.BaseStream.Position = 0;
					br.Write(br2.ReadBytes((int)br2.BaseStream.Length));
					br.Close();
				}
			}
		}

		public BinaryReaderX createCfg(int part)
		{
			long bk;

			BinaryWriterX br = new BinaryWriterX(new MemoryStream());

			br.WriteStruct<Header>(headerList[part]);

			for (int j = 0; j < editorEntries[part].Count; j++)
			{
				if (j > 0)
				{
					EditorStruct bk2 = editorEntries[part][j];

					var success1 = (headerList[part].dataOffset > br.BaseStream.Length) ? bk2.entryOffset = 0 : bk2.entryOffset = (uint)br.BaseStream.Length - headerList[part].dataOffset;

					bk = br.BaseStream.Position;
					var success2 = (headerList[part].dataOffset > br.BaseStream.Length) ? br.BaseStream.Position = headerList[part].dataOffset : br.BaseStream.Position = br.BaseStream.Length;
					br.Write(Encoding.GetEncoding("shift-jis").GetBytes(Labels[labelCount++].Text));
					br.Write((byte)0x00);
					br.BaseStream.Position = bk;

					editorEntries[part][j] = bk2;
				}

				br.WriteStruct<EditorStruct>(editorEntries[part][j]);
			}

			if (editorRest[part] != null) br.Write(editorRest[part]);
			else
			{
				for (int j = 0; j < textEntries[part].Count; j++)
				{
					if (j > 0 && textEntries[part][j].entryOffset != 0xFFFFFFFF)
					{
						TextStruct bk2 = textEntries[part][j];

						var success1 = (headerList[part].dataOffset > br.BaseStream.Length) ? bk2.entryOffset = 0 : bk2.entryOffset = (uint)br.BaseStream.Length - headerList[part].dataOffset;

						bk = br.BaseStream.Position;
						var success2 = (headerList[part].dataOffset > br.BaseStream.Length) ? br.BaseStream.Position = headerList[part].dataOffset : br.BaseStream.Position = br.BaseStream.Length;
						br.Write(Encoding.GetEncoding("shift-jis").GetBytes(Labels[labelCount++].Text));
						br.Write((byte)0x00);
						br.BaseStream.Position = bk;

						textEntries[part][j] = bk2;
					}

					br.WriteStruct<TextStruct>(textEntries[part][j]);
				}
				if (textRest[part] != null) br.Write(textRest[part]);
			}

			br.BaseStream.Position = 0x8;
			br.Write((int)(br.BaseStream.Length - headerList[part].dataOffset));

			br.BaseStream.Position = br.BaseStream.Length;
			while (br.BaseStream.Position % 16 != 0) br.Write((byte)0xFF);

			return new BinaryReaderX(br.BaseStream);
		}
	}
}