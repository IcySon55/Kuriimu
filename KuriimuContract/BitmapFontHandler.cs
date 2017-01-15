using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace KuriimuContract
{
	// This font handler expects to reveice an image (PNG) that is 16 characters across by 
	public sealed class BitmapFontHandler
	{
		private Dictionary<char, BitmapFontCharacter> _characters = null;

		public int Baseline { get; private set; }
		public int CharacterWidth { get; private set; }
		public int CharacterHeight { get; private set; }

		public BitmapFontHandler(Bitmap fontImage)
		{
			Bitmap fontMap = new Bitmap(fontImage);
			Baseline = 0;
			CharacterWidth = 0;
			CharacterHeight = 0;

			int xIndent = 0, yIndent = 0;
			int fullWidth = 0, fullHeight = 0;

			// Read Width Data
			bool readingWidth = false;
			for (int i = 0; i < fontMap.Width; i++)
			{
				Color result = fontMap.GetPixel(i, 0);

				if (!readingWidth && EqualRGB(result, Color.White))
					xIndent++;
				else if (!readingWidth && EqualRGB(result, Color.Black))
					readingWidth = true;
				else if (readingWidth && EqualRGB(result, Color.White))
					break;

				if (readingWidth && EqualRGB(result, Color.Black))
					CharacterWidth++;
			}

			// Read Height Data
			bool readingBaseline = false;
			bool readingHeight = false;
			for (int i = 0; i < fontMap.Height; i++)
			{
				Color result = fontMap.GetPixel(0, i);

				if (!readingBaseline && !readingHeight && EqualRGB(result, Color.White))
					yIndent++;
				else if (!readingBaseline && !readingHeight && EqualRGB(result, Color.Black))
					readingBaseline = true;
				else if (readingBaseline && !readingHeight && EqualRGB(result, Color.White))
					readingHeight = true;
				else if (readingBaseline && readingHeight && EqualRGB(result, Color.White))
					break;

				if (readingBaseline && !readingHeight && EqualRGB(result, Color.Black))
				{
					Baseline++;
					CharacterHeight++;
				}
				else if (readingBaseline && readingHeight)
					CharacterHeight++;
			}

			// Update Character Dimensions
			fullWidth = xIndent + CharacterWidth + xIndent;
			fullHeight = yIndent + CharacterHeight + yIndent + yIndent;

			_characters = new Dictionary<char, BitmapFontCharacter>();
			for (int i = ' '; i < 256; i++)
			{
				int x = ((i - 32) % 16) * fullWidth;
				int y = ((i - 32) / 16) * fullHeight;
				int kerningOffset = yIndent + yIndent + CharacterHeight - 1;

				Bitmap character = new Bitmap(CharacterWidth, CharacterHeight, PixelFormat.Format32bppArgb);
				int offset = 0;
				int width = 0;

				// Read Kerning Data
				for (int j = x + xIndent; j < x + xIndent + CharacterWidth; j++)
				{
					Color result = fontMap.GetPixel(j, y + kerningOffset);

					if (width == 0 && EqualRGB(result, Color.White))
						offset++;
					if (EqualRGB(result, Color.Red))
						width++;
				}

				// Copy Character Bitmap
				Graphics cgfx = Graphics.FromImage(character);
				cgfx.DrawImage(fontMap, 0, 0, new Rectangle(x + xIndent, y + yIndent, CharacterWidth, CharacterHeight), GraphicsUnit.Pixel);

				_characters.Add((char)i, new BitmapFontCharacter(character, offset, width));
			}
		}

		public BitmapFontCharacter GetCharacter(char character)
		{
			if (_characters.ContainsKey(character))
				return _characters[character];
			else
				return _characters['?'];
		}

		public BitmapFontCharacter GetCharacter(char character, Color color)
		{
			BitmapFontCharacter bfc = null;

			if (_characters.ContainsKey(character))
				bfc = _characters[character];
			else
				bfc = _characters['?'];

			Bitmap colorized = new Bitmap(bfc.Character);
			for (int i = 0; i < colorized.Width * colorized.Height; i++)
			{
				int cx = i % colorized.Width;
				int cy = i / colorized.Width;

				Color c = colorized.GetPixel(cx, cy);

				colorized.SetPixel(cx, cy, Color.FromArgb((c.R - 255) * -1, color.R, color.G, color.B));
			}

			return new BitmapFontCharacter(colorized, bfc.Offset, bfc.Width);
		}

		public int MeasureString(string text, char stopChar, double scale = 1.0)
		{
			int width = 0;

			foreach (char c in text)
			{
				BitmapFontCharacter bfc = GetCharacter(c);
				if (c == stopChar || bfc == null)
					break;
				width += bfc.Width;
			}

			return (int)Math.Round(width * scale);
		}

		private bool EqualRGB(Color lhs, Color rhs)
		{
			return (lhs.R == rhs.R && lhs.G == rhs.G && lhs.B == rhs.B);
		}
	}

	public sealed class BitmapFontCharacter
	{
		public Bitmap Character { get; }
		public int Offset { get; }
		public int Width { get; }

		public BitmapFontCharacter(Bitmap character, int offset, int width)
		{
			Character = character;
			Offset = offset;
			Width = width;
		}
	}
}