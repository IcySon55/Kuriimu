using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using image_xi;
using KuriimuContract;

namespace game_time_travelers
{
	public class XF
	{
		public Bitmap bmp;

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
			public short unk2;
			public ushort offsetTmp;
			public ushort fileSizeTmp;
			public ushort multiplier;

			public int fileSize => fileSizeTmp + ((multiplier / 256) * 0x10000) + (multiplier % 256);
			public int offset => offsetTmp * 4;
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
				{
					fileEntries.Add(br.ReadStruct<FileEntry>());
				}

				//get xi image
				Stream file = File.OpenWrite("img.bin");
				br.BaseStream.Position = header.dataOffset + fileEntries[0].offset;
				file.Write(br.ReadBytes(fileEntries[0].fileSize), 0, fileEntries[0].fileSize);
				file.Close();

				//convert xi image to bmp
				file = File.OpenRead("img.bin");
				bmp = XI.Load(file);
				bmp.Save("bmp.bmp");
				file.Close(); //File.Delete("img.bin");

				//get fnt.bin
				file = File.OpenWrite("fnt.bin");
				br.BaseStream.Position = header.dataOffset + fileEntries[0].fileSize + 4;
				file.Write(br.ReadBytes(fileEntries[1].fileSize), 0, fileEntries[1].fileSize);
				file.Close();

				//decompress fnt.bin
				file = File.OpenRead("fnt.bin");
				var fnt = new BinaryReaderX(file);
				fnt.BaseStream.Position = 0x28;

				byte[] buf1 = XI.Decomp(fnt);
				while (fnt.BaseStream.Position % 4 != 0) fnt.ReadByte();
				byte[] buf2 = XI.Decomp(fnt);
				while (fnt.BaseStream.Position % 4 != 0) fnt.ReadByte();
				byte[] buf3 = XI.Decomp(fnt);

				file.Close(); //File.Delete("fnt.bin");

				using (var br2 = new BinaryReaderX(new MemoryStream(buf1)))
					lstCharSizeInfo = Enumerable.Range(0, buf1.Length / 4).Select(_ => br2.ReadStruct<CharSizeInfo>()).ToList();
				using (var br2 = new BinaryReaderX(new MemoryStream(buf2)))
					dicGlyphLarge = Enumerable.Range(0, buf2.Length / 8).Select(i => br2.ReadStruct<CharacterMap>()).ToDictionary(x => x.code_point);
				using (var br2 = new BinaryReaderX(new MemoryStream(buf3)))
					dicGlyphSmall = Enumerable.Range(0, buf3.Length / 8).Select(i => br2.ReadStruct<CharacterMap>()).ToDictionary(x => x.code_point);
			}
		}

		public CharacterMap GetCharacterMap(char c, bool small)
		{
			CharacterMap result;
			if (small == false)
			{
				var success = dicGlyphLarge.TryGetValue(c, out result) || dicGlyphLarge.TryGetValue('?', out result);
			}
			else
			{
				var success = dicGlyphSmall.TryGetValue(c, out result) || dicGlyphSmall.TryGetValue('?', out result);
			}
			return result;
		}
		public CharSizeInfo GetCharacterInfo(int offset) => lstCharSizeInfo[offset];

		public float DrawCharacter(char c, Color color, Graphics g, float x, float y, bool small)
		{
			CharacterMap charMap = GetCharacterMap(c, small);
			CharSizeInfo charInfo = GetCharacterInfo(charMap.CharSizeInfoIndex);

			var attr = new ImageAttributes();
			var matrix = Enumerable.Repeat(new float[5], 5).ToArray();
			matrix[charMap.ColorChannel] = new[] { 0, 0, 0, 1f, 0 };
			matrix[4] = new[] { color.R / 255f, color.G / 255f, color.B / 255f, 0, 0 };
			attr.SetColorMatrix(new ColorMatrix(matrix));

			g.DrawImage(bmp,
				new[] { new PointF(x + charInfo.offset_x, y + charInfo.offset_y),
						new PointF(x + charInfo.offset_x + charInfo.glyph_width, y + charInfo.offset_y),
						new PointF(x + charInfo.offset_x, y + charInfo.offset_y + charInfo.glyph_height) },
				new RectangleF(charMap.ImageOffsetX, charMap.ImageOffsetY, charInfo.glyph_width, charInfo.glyph_height),
				GraphicsUnit.Pixel,
				attr);

			return x + charMap.CharWidth;
		}

	}
}