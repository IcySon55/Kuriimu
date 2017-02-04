using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using KuriimuContract;

namespace image_xi
{
	class XI
	{
		public enum Format : byte
		{
			RGBA8888, RGBA4444,
			RGBA5551, RGB8, RGB565,
			LA8 = 11, LA4, L8, HILO8, A8,
			L4 = 26, A4, ETC1, ETC1A4
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct Header
		{
			public Magic magic; // IMGC
			public int const1; // 30 30 00 00
			public short const2; // 30 00
			public Format imageFormat;
			public byte const3; // 01
			public byte combineFormat;
			public byte bitDepth;
			public short bytesPerTile;
			public short width;
			public short height;
			public int const4; // 30 00 00 00
			public int const5; // 30 00 01 00
			public int tableDataOffset; // always 0x48
			public int const6; // 03 00 00 00
			public int const7; // 00 00 00 00
			public int const8; // 00 00 00 00
			public int const9; // 00 00 00 00
			public int const10; // 00 00 00 00
			public int tableSize1;
			public int tableSize2;
			public int imgDataSize;
			public int const11; // 00 00 00 00
			public int const12; // 00 00 00 00

			public void checkConst()
			{
				var ex = new Exception("Unknown xi file format!");
				var ex2 = new Exception("No xi file format!");

				if (magic != "IMGC") throw ex2;
				if (const1 != 0x3030) throw ex;
				if (const2 != 0x30) throw ex;
				if (const3 != 1) throw ex;
				if (const4 != 0x30) throw ex;
				if (const5 != 0x10030) throw ex;
				if (tableDataOffset != 0x48) throw ex;
				if (const6 != 3) throw ex;
				if (const7 != 0) throw ex;
				if (const8 != 0) throw ex;
				if (const9 != 0) throw ex;
				if (const10 != 0) throw ex;
				if (const11 != 0) throw ex;
				if (const12 != 0) throw ex;
				if (((tableSize1 + 3) & ~3) != tableSize2) throw ex;
				if (bytesPerTile != 8 * bitDepth) throw ex;
			}
		}

		public static Bitmap Load(Stream input)
		{
			using (var br = new BinaryReaderX(input))
			{
				//check header
				var header = br.ReadStruct<Header>();
				header.checkConst();

				//decompress table
				br.BaseStream.Position = header.tableDataOffset;
				byte[] table = Decomp(new BinaryReaderX(new MemoryStream(br.ReadBytes(header.tableSize2))));

				//get decompressed picture data
				br.BaseStream.Position = header.tableDataOffset + header.tableSize2;
				byte[] tex = Decomp(new BinaryReaderX(new MemoryStream(br.ReadBytes(header.imgDataSize))));

				//order pic blocks by table
				byte[] pic = Order(new BinaryReaderX(new MemoryStream(table)), table.Length, new BinaryReaderX(new MemoryStream(tex)),header.width,header.height,header.imageFormat);
				
				//return decompressed picture data
				return ImageCommon.FromTexture(tex, header.width, header.height, (ImageCommon.Format)Enum.Parse(typeof(ImageCommon.Format), header.imageFormat.ToString()));
			}
		}

		public static UInt32 getUInt32(UInt32 n)
		{
			return BitConverter.ToUInt32(BitConverter.GetBytes(n).Reverse().ToArray(), 0);
		}

		public static byte[] Decomp(BinaryReaderX br)
		{
			int sizeAndMethod = br.ReadInt32();
			int method = sizeAndMethod % 8;
			int size = sizeAndMethod / 8;
			switch (method)
			{
				case 0://No compression
					return br.ReadBytes(size);
				case 1://LZSS
					return CommonCompression.Decomp_LZSS(br, size);
				case 2://4bit Huffman
					return CommonCompression.Decomp_Huff(br, 1, size);
				case 3://8bit Huffman
					return CommonCompression.Decomp_Huff(br, 0, size);
				case 4://RLE
					return CommonCompression.Decomp_RLE(br, size);
				default:
					throw new Exception("Unknown compression!");
			}
		}

		public static byte[] Order(BinaryReaderX table, int tableLength, BinaryReaderX tex, int w, int h, Format format)
		{
			switch (format)
			{
				case Format.RGB8:
					var result = new byte[w * h * 3];
					int resultCount = 0;
					int[] tileorder = {7,6,15,14,5,4,13,12,23,22,31,30,};

					for (int i = 0; i<tableLength;i+=2)
					{
						int entry = table.ReadUInt16();
						if (entry == 0xFFFF)
						{
							for (int j = 0; j < 64 * 3; j++)
							{
								result[resultCount++] = 0;
							}
						} else
						{
							tex.BaseStream.Position = entry * 64 * 3;
							for (int j = 0; j < 64 * 3; j++)
							{
								result[resultCount++] = tex.ReadByte();
							}
							/*int[] tileorder = {0,8,1,9,16,24,17,25,2,10,3,11,18,26,19,27,32,40,33,41,48,56,49,57,34,42,35,43,50,58,51,59,4,12,5,13,20,28,21,29,6,14,7,15,22,30,23,31,36,44,37,45,52,60,53,61,38,46,39,47,54,62,55,63};
							tex.BaseStream.Position = entry * 64 * 3;
							foreach (int j in tileorder)
							{
								result[resultCount + j * 3] = tex.ReadByte();
								result[resultCount + j * 3 + 1] = tex.ReadByte();
								result[resultCount + j * 3 + 2] = tex.ReadByte();
							}
							resultCount += (64*3);*/
						}
					}
					return result;
				default:
					throw new Exception("Unsupported Picture encoding!");
			}
		}
	}
}