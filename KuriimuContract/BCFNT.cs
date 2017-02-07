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
	public class BCFNT
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		[DebuggerDisplay("[{left}, {glyph_width}, {char_width}]")]
		public struct CharWidthInfo
		{
			public sbyte left;
			public byte glyph_width;
			public byte char_width;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CFNT
		{
			public uint magic;
			public ushort endianness;
			public short header_size;
			public int version;
			public int file_size;
			public int num_blocks;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct FINF
		{
			public uint magic;
			public int section_size;
			public byte font_type;
			public byte line_feed;
			public short alter_char_index;
			public CharWidthInfo default_width;
			public byte encoding;
			public int tglp_offset;
			public int cwdh_offset;
			public int cmap_offset;
			public byte height;
			public byte width;
			public byte ascent;
			public byte reserved;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct TGLP
		{
			public uint magic;
			public int section_size;
			public byte cell_width;
			public byte cell_height;
			public byte baseline_position;
			public byte max_character_width;
			public int sheet_size;
			public short num_sheets;
			public short sheet_image_format;
			public short num_columns;
			public short num_rows;
			public short sheet_width;
			public short sheet_height;
			public int sheet_data_offset;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
		public struct CMAP
		{
			public uint magic;
			public int section_size;
			public char code_begin;
			public char code_end;
			public short mapping_method;
			public short reserved;
			public int next_offset;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CWDH
		{
			public uint magic;
			public int section_size;
			public short start_index;
			public short end_index;
			public int next_offset;
		};

		FINF finf;
		TGLP tglp;
		public Bitmap bmp;
		ImageAttributes attr = new ImageAttributes();
		List<CharWidthInfo> lstCWDH = new List<CharWidthInfo>();
		Dictionary<char, int> dicCMAP = new Dictionary<char, int>();

		public CharWidthInfo GetWidthInfo(char c) => lstCWDH[GetIndex(c)];
		public int LineFeed => finf.line_feed;

		int GetIndex(char c)
		{
			int result;
			bool success = dicCMAP.TryGetValue(c, out result) || dicCMAP.TryGetValue('?', out result);
			return result;
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

		public float DrawCharacter(char c, Graphics g, float x, float y, float scale)
		{
			var index = GetIndex(c);
			var widthInfo = lstCWDH[index];

			int cellsPerSheet = tglp.num_rows * tglp.num_columns;
			int sheetNum = index / cellsPerSheet;
			int cellRow = (index % cellsPerSheet) / tglp.num_columns;
			int cellCol = index % tglp.num_columns;
			int xOffset = cellCol * (tglp.cell_width + 1) + 1;
			int yOffset = sheetNum * tglp.sheet_height + cellRow * (tglp.cell_height + 1) + 1;

			g.DrawImage(bmp,
				 new[] { new PointF(x + widthInfo.left * scale, y),
						  new PointF(x + (widthInfo.left + widthInfo.glyph_width) * scale, y),
						  new PointF(x + widthInfo.left * scale, y + tglp.cell_height * scale)},
				 new RectangleF(xOffset, yOffset, widthInfo.glyph_width, tglp.cell_height),
				 GraphicsUnit.Pixel,
				 attr);
			return x + widthInfo.char_width;
		}

		public float MeasureString(string text, char stopChar, float scale = 1.0f)
		{
			return text.TakeWhile(c => c != stopChar).Sum(c => GetWidthInfo(c).char_width) * scale;
		}

		public BCFNT(Stream input)
		{
			using (var br = new BinaryReaderX(input, Encoding.Unicode))
			{
				var cfnt = br.ReadStruct<CFNT>();
				finf = br.ReadStruct<FINF>();

				// read TGLP
				br.BaseStream.Position = finf.tglp_offset - 8;
				tglp = br.ReadStruct<TGLP>();

				// read image data
				br.BaseStream.Position = tglp.sheet_data_offset;
				int width = tglp.sheet_width;
				int height = tglp.sheet_height * tglp.num_sheets;
				var bytes = br.ReadBytes(tglp.sheet_size * tglp.num_sheets);
				bmp = new Bitmap(width, height);
				var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				unsafe
				{
					var ptr = (int*)data.Scan0;
					for (int i = 0; i < width * height; i++)
					{
						int x = (i / 64 % (width / 8)) * 8 + (i / 4 & 4) | (i / 2 & 2) | (i & 1);
						int y = (i / 64 / (width / 8)) * 8 + (i / 8 & 4) | (i / 4 & 2) | (i / 2 & 1);
						int a;
						if (tglp.sheet_image_format == 8) // A8
							a = bytes[i];
						else if (tglp.sheet_image_format == 11) // A4
							a = (bytes[i / 2] >> (i % 2 * 4)) % 16 * 17;
						else
							throw new NotSupportedException("Only supports A4 and A8 for now");
						ptr[width * y + x] = a << 24;
					}
				}
				bmp.UnlockBits(data);

				// read CWDH
				for (int offset = finf.cwdh_offset; offset != 0;)
				{
					br.BaseStream.Position = offset - 8;
					var cwdh = br.ReadStruct<CWDH>();
					for (int i = cwdh.start_index; i <= cwdh.end_index; i++)
						lstCWDH.Add(br.ReadStruct<CharWidthInfo>());
					offset = cwdh.next_offset;
				}

				// read CMAP
				for (int offset = finf.cmap_offset; offset != 0;)
				{
					br.BaseStream.Position = offset - 8;
					var cmap = br.ReadStruct<CMAP>();
					switch (cmap.mapping_method)
					{
						case 0:
							var charOffset = br.ReadUInt16();
							for (char i = cmap.code_begin; i <= cmap.code_end; i++)
							{
								int idx = i - cmap.code_begin + charOffset;
								dicCMAP[i] = idx < UInt16.MaxValue ? idx : 0;
							}
							break;
						case 1:
							for (char i = cmap.code_begin; i <= cmap.code_end; i++)
							{
								int idx = br.ReadUInt16();
								dicCMAP[i] = idx < UInt16.MaxValue ? idx : 0;
							}
							break;
						case 2:
							var n = br.ReadUInt16();
							for (int i = 0; i < n; i++)
							{
								char c = br.ReadChar();
								int idx = br.ReadUInt16();
								dicCMAP[c] = idx < UInt16.MaxValue ? idx : 0;
							}
							break;
						default:
							throw new Exception("Unsupported mapping method");
					}
					offset = cmap.next_offset;
				}
			}
		}
	}
}
