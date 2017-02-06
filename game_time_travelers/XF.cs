using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KuriimuContract
{
	public class XF
	{
		[DllImport("plugins/image_xi.dll", EntryPoint = "Load", CallingConvention = CallingConvention.Cdecl)]
		static extern Bitmap Load(Stream filename);

		public Bitmap bmp;
		public string txt;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		[DebuggerDisplay("[{offset_x}, {offset_y}, {glyph_width}, {glyph_height}]")]
		public struct CharSizeInfo
		{
			public sbyte offset_x;
			public sbyte offset_y;
			public byte glyph_width;
			public byte glyph_height;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
		[DebuggerDisplay("[{code_point}] {ColorChannel}:{ImageOffsetX}:{ImageOffsetY}")]
		public struct CharacterMap
		{
			public char code_point;
			ushort char_size;
			int image_offset;

			public int CharSizeInfoIndex => char_size % 1024;
			public int CharWidth => char_size / 1024;
			public int ColorChannel => image_offset % 16;
			public int ImageOffsetX => image_offset / 16 % 16384;
			public int ImageOffsetY => image_offset / 16 / 16384;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct XpckHeader
		{
			public Magic magic; //XPCK
			public byte fileEntries;
			public byte unk1;
			public short fileInfoOffset;
			public short filenameTableOffset;
			public short dataOffset;
			public short fileInfoSize;
			public short filenameTableSize;
			public int dataSize;

			public void CorrectHeader()
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
		struct FileEntry
		{
			public int unk1;
			public int offset;
			public int fileSize;
		}

		List<FileEntry> fileEntries;
		List<CharSizeInfo> lstCharSizeInfo;
		Dictionary<char, CharacterMap> dicGlyphLarge;
		Dictionary<char, CharacterMap> dicGlyphSmall;

		public XF(Stream input)
		{
			using (var br = new BinaryReaderX(input))
			{
				//parse Header
				var header = br.ReadStruct<XpckHeader>();
				header.CorrectHeader();

				//parse FileEntries
				fileEntries = new List<FileEntry>();
				for (int i = 0; i < header.fileEntries; i++)
					fileEntries.Add(br.ReadStruct<FileEntry>());

				//get xi image
				Stream file = File.OpenWrite("img.bin");
				br.BaseStream.Position = header.fileInfoOffset + fileEntries[0].offset;
				file.Write(br.ReadBytes(fileEntries[0].fileSize),0, fileEntries[0].fileSize);
				file.Close();

				//convert xi image to bmp
				bmp = game_time_travelers.image_xi.Load
				bmp = new XI(new MemoryStream(br.ReadBytes(0x3396C))).Image; // temporary hack -- only works with nrm_main.xf for now
				br.ReadBytes(0x28); // temporary hack -- should be the header
				var buf1 = CriWare.GetDecompressedBytes(br.BaseStream);
				var buf2 = CriWare.GetDecompressedBytes(br.BaseStream);
				var buf3 = CriWare.GetDecompressedBytes(br.BaseStream);

				lstCharSizeInfo = Enumerable.Range(0, buf1.Length / 4).Select(i => buf1.Skip(4 * i).Take(4).ToArray().ToStruct<CharSizeInfo>()).ToList();
				dicGlyphLarge = Enumerable.Range(0, buf2.Length / 8).Select(i => buf2.Skip(8 * i).Take(8).ToArray().ToStruct<CharacterMap>()).ToDictionary(x => x.code_point);
				dicGlyphSmall = Enumerable.Range(0, buf3.Length / 8).Select(i => buf3.Skip(8 * i).Take(8).ToArray().ToStruct<CharacterMap>()).ToDictionary(x => x.code_point);
			}
		}

		public static byte[] Decompress(BinaryReaderX br)
		{

		}

		public void SetTextColor(Color color)
		{
			attr.SetColorMatrix(new ColorMatrix(new[]
			{
					 new[] { 0, 0, 0, 0, 0f },
					 new[] { 0, 0, 0, 0, 0f },
					 new[] { 0, 0, 0, 0, 0f },
					 new[] { 0, 0, 0, 1, 0f },
					 new[] { color.R / 255f, color.G / 255f, color.B / 255f, 0, 1 }
				}));
		}

		CharacterMap GetCharacterMap(char c)
		{
			CharacterMap result;
			var success = dicGlyphLarge.TryGetValue(c, out result) || dicGlyphLarge.TryGetValue('?', out result);
			return result;
		}

		public float Draw(char ch, Color color, Graphics g, float x, float y)
		{
			var map = GetCharacterMap(ch);
			var sizeInfo = lstCharSizeInfo[map.CharSizeInfoIndex];

			var attr = new ImageAttributes();
			var matrix = Enumerable.Repeat(new float[5], 5).ToArray();
			matrix[map.ColorChannel] = new[] { color.R / 255f, color.G / 255f, color.B / 255f, 1f, 0 };
			attr.SetColorMatrix(new ColorMatrix(matrix));

			g.DrawImage(bmp,
				new[] { new PointF(x + sizeInfo.offset_x, y + sizeInfo.offset_y),
						new PointF(x + sizeInfo.offset_x + sizeInfo.glyph_width, y + sizeInfo.offset_y),
						new PointF(x + sizeInfo.offset_x, y + sizeInfo.offset_y + sizeInfo.glyph_height) },
				new RectangleF(map.ImageOffsetX, map.ImageOffsetY, sizeInfo.glyph_width, sizeInfo.glyph_height),
				GraphicsUnit.Pixel,
				attr);

			return x + map.CharWidth;
		}

	}
}