using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using KuriimuContract;
using Cetera;
using Cetera.IO;
using Cetera.Compression;

namespace archive_nus3bank
{
	public sealed class NUS3 : List<NUS3.Node>, IDisposable
	{
		public class Node
		{
			public String filename;
			public TONE.ToneEntry entry;
			public Stream FileData;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Header
		{
			public String4 magic;
			public int fileSize; //without magic
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BankToc
		{
			String8 magic;
			public int size;
			public int entryCount;
		}

		public class BankTocEntry
		{
			public BankTocEntry(Stream input)
			{
				using (var br = new BinaryReaderX(input, true))
				{
					magic = br.ReadStruct<String4>();
					secSize = br.ReadInt32();
				}
			}
			public String4 magic;
			public int secSize;
			public int offset;
		}

		public class PROP
		{
			public PROP(Stream input)
			{
				using (var br = new BinaryReaderX(input, true))
				{
					unk1 = br.ReadInt32();
					unk2 = br.ReadInt32();
					unk3 = br.ReadInt32();
					projectNameSize = br.ReadByte();
					projectName = readASCII(br.BaseStream);

					br.BaseStream.Position += 1;
					while (br.BaseStream.Position % 4 > 0)
					{
						br.BaseStream.Position += 1;
					}

					unk4 = br.ReadInt32();
					dateSize = br.ReadByte();
					//date = readASCII(br.BaseStream);
					date = Encoding.ASCII.GetString(br.ReadBytes(0x17));
				}
			}
			int unk1;
			int unk2;
			int unk3;
			public byte projectNameSize;
			public String projectName;
			int unk4;
			public byte dateSize;
			public String date;
		}

		public class BINF
		{
			public BINF(Stream input)
			{
				using (var br = new BinaryReaderX(input, true))
				{
					unk1 = br.ReadInt32();
					unk2 = br.ReadInt32();
					nameSize = br.ReadByte();
					name = readASCII(br.BaseStream);

					br.BaseStream.Position += 1;
					while (br.BaseStream.Position % 4 > 0)
					{
						br.BaseStream.Position += 1;
					}

					ID = br.ReadInt32();
				}
			}
			int unk1;
			int unk2;
			public byte nameSize;
			public String name;
			public int ID;
		}

		public class TONE
		{
			public TONE(Stream input)
			{
				using (var br = new BinaryReaderX(input, true))
				{
					toneCount = br.ReadInt32();

					toneEntries = new ToneEntry[toneCount];
					for (int i = 0; i < toneCount; i++)
					{
						toneEntries[i] = new ToneEntry();
						toneEntries[i].offset = br.ReadInt32() + banktocEntries[4].offset + 8;
						toneEntries[i].metaSize = br.ReadInt32();

						long bk = br.BaseStream.Position;
						if (toneEntries[i].metaSize > 0xc)
						{
							br.BaseStream.Position = toneEntries[i].offset + 6;
							byte tmp;
							tmp = br.ReadByte();
							if (tmp == 0 || tmp > 9)
							{
								br.BaseStream.Position += 5;
							}
							else
							{
								br.BaseStream.Position += 1;
							}
							toneEntries[i].nameSize = br.ReadByte();
							toneEntries[i].name = readASCII(br.BaseStream);

							br.BaseStream.Position += 1;
							while (br.BaseStream.Position % 4 > 0)
							{
								br.BaseStream.Position += 1;
							}

							if (br.ReadInt32() != 0) br.BaseStream.Position -= 4;
							br.BaseStream.Position += 4;
							toneEntries[i].packOffset = banktocEntries[6].offset + 8 + br.ReadInt32();
							toneEntries[i].size = br.ReadInt32();
						}
						br.BaseStream.Position = bk;
					}
				}
			}
			public int toneCount;
			public ToneEntry[] toneEntries;

			public class ToneEntry
			{
				public int offset;
				public int metaSize;
				public byte nameSize;
				public String name;
				public int packOffset;
				public int size;
			}
		}

		public Header header;
		public BankToc banktocHeader;
		public static List<BankTocEntry> banktocEntries;
		public PROP prop;
		public BINF binf;

		public TONE tone;

		Stream stream;

		public NUS3(String filename, bool isZlibCompressed = false)
		{
			if (isZlibCompressed) throw new NotImplementedException();

			//var bytes = File.ReadAllBytes(filename);
			stream = File.OpenRead(filename);

			using (var br = new BinaryReaderX(stream, true))
			{
				//Header
				header = br.ReadStruct<Header>();

				//Banktoc
				banktocHeader = br.ReadStruct<BankToc>();

				int offset = 0x18 + banktocHeader.entryCount * 0x8;
				banktocEntries = new List<BankTocEntry>();
				for (int i = 0; i < banktocHeader.entryCount; i++)
				{
					banktocEntries.Add(new BankTocEntry(br.BaseStream));
					banktocEntries[i].offset = offset;
					offset += banktocEntries[i].secSize + 8;
				}

				//PROP
				//br.BaseStream.Position = banktocEntries[0].offset;
				if (br.ReadStruct<Header>().magic != "PROP") throw new Exception();
				prop = new PROP(br.BaseStream);

				//BINF
				//br.BaseStream.Position = banktocEntries[1].offset;
				if (br.ReadStruct<Header>().magic != "BINF") throw new Exception();
				binf = new BINF(br.BaseStream);

				//GRP - not yet mapped
				if (br.ReadStruct<Header>().magic != "GRP ") throw new Exception();
				br.ReadBytes(banktocEntries[2].secSize);

				//DTON - not yet mapped
				if (br.ReadStruct<Header>().magic != "DTON") throw new Exception();
				br.ReadBytes(banktocEntries[3].secSize);

				//TONE
				if (br.ReadStruct<Header>().magic != "TONE") throw new Exception();
				var tmp = br.BaseStream.Position;
				tone = new TONE(br.BaseStream);
				br.BaseStream.Position = tmp + banktocEntries[4].secSize;

				//JUNK - not yet mapped
				if (br.ReadStruct<Header>().magic != "JUNK") throw new Exception();
				br.ReadBytes(banktocEntries[5].secSize);

				//PACK and finishing
				if (br.ReadStruct<Header>().magic != "PACK") throw new Exception();
				for (int i = 0; i < tone.toneCount; i++)
				{
					br.BaseStream.Position = tone.toneEntries[i].packOffset;

					Add(new Node
					{
						filename = tone.toneEntries[i].name + ".idsp",
						entry = tone.toneEntries[i],
						FileData = new SubStream(stream, tone.toneEntries[i].packOffset, tone.toneEntries[i].size)
					});
				}
			}
		}

		public static String readASCII(Stream input)
		{
			using (var br = new BinaryReaderX(input, true))
			{
				String result = "";
				Encoding ascii = Encoding.GetEncoding("ascii");

				byte[] character = br.ReadBytes(1);
				while (character[0] != 0x00)
				{
					result += ascii.GetString(character);
					character = br.ReadBytes(1);
				}

				return result;
			}
		}

		public void Dispose() => stream?.Dispose();
	}
}
