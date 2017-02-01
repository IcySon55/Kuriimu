using System;
using System.Drawing;
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
				byte[] table = Decomp(br, header.tableDataOffset, header.tableSize2);

				//get decompressed picture data
				byte[] tex = Decomp(br, header.tableDataOffset + header.tableSize2, header.imgDataSize);

				//return decompressed picture data
				return ImageCommon.FromTexture(tex, header.width, header.height, (ImageCommon.Format)Enum.Parse(typeof(ImageCommon.Format), header.imageFormat.ToString()));
			}
		}

		public static UInt32 getUInt32(UInt32 n)
		{
			return BitConverter.ToUInt32(BitConverter.GetBytes(n).Reverse().ToArray(), 0);
		}

		public static byte[] Decomp(BinaryReaderX br, int offset, int compSize)
		{
			br.BaseStream.Position = offset;

			int sizeAndMethod = br.ReadInt32();
			int method = ((sizeAndMethod >> 3) << 3) ^ sizeAndMethod;
			int size = sizeAndMethod >> 3;
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
	}
}