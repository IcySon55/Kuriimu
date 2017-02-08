using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using KuriimuContract;
using Cetera;

namespace image_xi
{
	public class XI
	{
		public enum Format : byte
		{
			RGBA8888, RGBA4444,
			RGBA5551, RGB888, RGB565,
			LA88 = 11, LA44, L8, HL88, A8,
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
				byte[] table = Decomp(new BinaryReaderX(new MemoryStream(br.ReadBytes(header.tableSize1))));
				//File.OpenWrite("table.bin").Write(table,0,table.Length);

				//get decompressed picture data
				br.BaseStream.Position = header.tableDataOffset + header.tableSize2;
				byte[] tex = Decomp(new BinaryReaderX(new MemoryStream(br.ReadBytes(header.imgDataSize))));
				//File.OpenWrite("xi.bin").Write(tex, 0, tex.Length);

				//order pic blocks by table
				byte[] pic = Order(new BinaryReaderX(new MemoryStream(table)), table.Length, new BinaryReaderX(new MemoryStream(tex)), header.width, header.height, header.bitDepth);
				//File.OpenWrite("pic.bin").Write(pic, 0, pic.Length);

				//return decompressed picture data
				var settings = new ImageCommon.Settings
				{
					Width = header.width,
					Height = header.height,
					Orientation = ImageCommon.Orientation.TransposeTile,
					Format = ImageCommon.Settings.ConvertFormat(header.imageFormat),
					PadToPowerOf2 = false
				};
				return ImageCommon.Load(pic, settings);
			}
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
					return CommonCompression.Decomp_Huff(br, 4, size);
				case 3://8bit Huffman
					return CommonCompression.Decomp_Huff(br, 8, size);
				case 4://RLE
					return CommonCompression.Decomp_RLE(br, size);
				default:
					throw new Exception(br.BaseStream.Position.ToString());//throw new Exception("Unknown compression!");
			}
		}

		public static byte[] Order(BinaryReaderX table, int tableLength, BinaryReaderX tex, int w, int h, byte bitDepth)
		{
			var ms = new MemoryStream();
			for (int i = 0; i < tableLength; i += 2)
			{
				int entry = table.ReadUInt16();
				if (entry == 0xFFFF)
				{
					for (int j = 0; j < (64 * bitDepth) / 8; j++)
					{
						ms.WriteByte(0);
					}
				}
				else
				{
					tex.BaseStream.Position = entry * (64 * bitDepth / 8);
					for (int j = 0; j < (64 * bitDepth) / 8; j++)
					{
						ms.WriteByte(tex.ReadByte());
					}
				}
			}
			return ms.ToArray();
		}
	}
}