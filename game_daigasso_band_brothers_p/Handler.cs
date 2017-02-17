using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Cetera.Compression;
using Cetera.Font;
using game_daigasso_band_brothers_p.Properties;
using KuriimuContract;

namespace game_daigasso_band_brothers_p
{
	class Handler : IGameHandler
	{
		static Lazy<BCFNT[]> fontInitializer = new Lazy<BCFNT[]>(() => new[] {
				new BCFNT(new MemoryStream(GZip.Decompress(Resources.Basic_bcfnt))),
				new BCFNT(new MemoryStream(GZip.Decompress(Resources.BasicRim_bcfnt))),
				new BCFNT(new MemoryStream(GZip.Decompress(Resources.cbf_std_bcfnt))),
				new BCFNT(new MemoryStream(GZip.Decompress(Resources.SisterSymbol_bcfnt))),
			});
		BCFNT fontBasic => fontInitializer.Value[0];
		BCFNT fontBasicRim => fontInitializer.Value[1];
		BCFNT fontCbfStd => fontInitializer.Value[2];
		BCFNT fontSisterSymbol => fontInitializer.Value[3];

		// Information
		public string Name => "Jam With The Band P";
		public Image Icon => Resources.p;

		// Feature Support
		public bool HandlerCanGeneratePreviews => true;

		static Dictionary<uint, string> colours = new Dictionary<uint, string>
		{
			[0xFFFFFFFF] = "W",
			[0xFF000000] = "B",
			[0xFF3A41FF] = "R",
			[0xFF1E1E1E] = "G"
		};

		static Dictionary<string, string> symbols = new Dictionary<string, string>
		{
			["\uE000"] = "Ⓐ",
			["\uE001"] = "Ⓑ",
			["\uE002"] = "Ⓧ",
			["\uE003"] = "Ⓨ",
			["\uE004"] = "🄻",
			["\uE005"] = "🅁",
			["\uE006"] = "✚",
			["\uE073"] = "🏠",
			["\uE077"] = "◉",
			["\uE07B"] = "⭠",
			["\uE07D"] = "⭥",
			["\uE07E"] = "⭤",
			["\uE100"] = "🖉",
			["\uE101"] = "🔧",
			["\uE103"] = "⌫",
			["\uE106"] = "🎼",
			["\uE107"] = "⭢",
			["\uE108"] = "🎤",
			["\uE10B"] = "🔍",
			["\uE10C"] = "🔎",
			["\uE10D"] = "📄",
			["\uE10E"] = "🍅",
			["\uE10F"] = "⏵",
			["\uE110"] = "⏸",
			["\uE112"] = "⏩",
			["\uE113"] = "⏪",
			["\uE114"] = "⏮",
			["\uE116"] = "⏺",
			["\uE117"] = "🕮",
			["\uE118"] = "⌧",
			["\uE119"] = "🎞",
			["\uE11A"] = "▶",
			["\uE11B"] = "◀"
		};

		public string GetKuriimuString(string str)
		{
			try
			{
				str = symbols.Aggregate(str, (s, kvp) => s.Replace(kvp.Key, kvp.Value));
				Func<string, byte[], string> Fix = (id, bytes) =>
				{
					switch (id)
					{
						case "\0\x2":
							return "s" + BitConverter.ToInt16(bytes, 0);
						case "\0\x3":
							return "c" + colours[BitConverter.ToUInt32(bytes, 0)];
						case "\x1\x0":
							return "v";
						case "\x1\x7":
							return "v" + Encoding.Unicode.GetString(bytes).Substring(1);
						case "\x2\0":
							return "w" + BitConverter.ToInt16(bytes, 0);
						case "\x2\x1":
							return "h" + BitConverter.ToInt16(bytes, 0);
						default:
							return $"n{(int)id[1]:X2}:" + Convert.ToBase64String(bytes);
					}
				};

				int i;
				str = str.Replace("\xF\x2\0", "</>");
				while ((i = str.IndexOf('\xE')) >= 0)
				{
					var id = str.Substring(i + 1, 2);
					var data = str.Substring(i + 4, str[i + 3]).Select(c => (byte)c).ToArray();
					str = str.Remove(i, data.Length + 4).Insert(i, $"<{Fix(id, data)}>");
				}
			}
			catch
			{

			}
			return str;
		}

		public string GetRawString(string str)
		{
			try
			{
				str = symbols.Aggregate(str, (s, kvp) => s.Replace(kvp.Value, kvp.Key));
				if (str.Length < 3) return str;

				return string.Concat(str.Split("<>".ToArray()).Select((z, i) =>
				{
					if (i % 2 == 0) return z;
					if (z == "/") return "\xF\x2\0";
					if (z == "v") return "\xE\x1\0\0";

					Func<string, byte[], string> Merge = (id, data) => $"\xE{id}{(char)data.Length}{string.Concat(data.Select(b => (char)b))}";

					var s = z.Substring(1);
					switch (z[0])
					{
						case 's':
							return Merge("\0\x2", BitConverter.GetBytes(short.Parse(s)));
						case 'c':
							return Merge("\0\x3", BitConverter.GetBytes(colours.First(e => e.Value == s).Key));
						case 'v':
							return Merge("\x1\x7", new byte[] { 0x1A, 0 }.Concat(Encoding.Unicode.GetBytes(s)).ToArray());
						case 'w':
							return Merge("\x2\0", BitConverter.GetBytes(short.Parse(s)));
						case 'h':
							return Merge("\x2\x1", BitConverter.GetBytes(short.Parse(s)));
						default:
							return Merge("\x3" + (char)int.Parse(s.Substring(0, 2), NumberStyles.HexNumber), Convert.FromBase64String(s.Substring(3)));
					}
				}));
			}
			catch
			{
				return str;
			}
		}

		public IList<Bitmap> Pages { get; private set; } = new List<Bitmap>();

		public void GeneratePages(IEntry entry)
		{
			var pages = new List<Bitmap>();

			Bitmap bmp;
			float txtOffsetX, txtOffsetY, scale;
			float fullWidth;
			BCFNT font;

			var rawString = entry.EditedText;
			if (string.IsNullOrWhiteSpace(rawString))
				rawString = entry.OriginalText;

			if (entry.Name.StartsWith("EDT_HELP") || entry.Name.StartsWith("VCL_HELP"))
			{
				bmp = new Bitmap(Resources.daigasso_box);
				font = fontBasic;
				font.SetColor(Color.Black);
				fontSisterSymbol.SetColor(Color.Black);
				fullWidth = 336;
				txtOffsetX = 32;
				txtOffsetY = 12;
				scale = 0.6f;
			}
			else if (entry.Name.StartsWith("Tutorial_") || rawString.Contains("\xE\x1"))
			{
				bmp = new Bitmap(410, 70);
				using (var g = Graphics.FromImage(bmp))
				{
					g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
					g.FillRectangle(Brushes.LightYellow, 5, 6, 400, 58);
				}
				font = fontBasicRim;
				font.SetColor(Color.White);
				fullWidth = float.MaxValue;
				txtOffsetX = 5;
				txtOffsetY = 6;
				scale = 0.84f;
			}
			else
			{
				int height = 18 * (entry.OriginalText.Count(c => c == '\n') + 1);
				bmp = new Bitmap(256, height + 10);
				using (var g = Graphics.FromImage(bmp))
				{
					g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
					g.FillRectangle(Brushes.LightYellow, 3, 5, 250, height);
				}
				font = fontCbfStd;
				font.SetColor(Color.Black);
				fontSisterSymbol.SetColor(Color.Black);
				fullWidth = 250;
				txtOffsetX = 3;
				txtOffsetY = 5;
				scale = 0.6f;
			}

			float widthMultiplier = 1;
			BCFNT baseFont = font;
			using (var g = Graphics.FromImage(bmp))
			{
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.InterpolationMode = InterpolationMode.Bicubic;
				float x = 0, y = 0;
				for (int i = 0; i < rawString.Length; i++)
				{
					var c = rawString[i];
					if (c == 0xE)
					{
						if (rawString[i + 1] == 2 && rawString[i + 2] == 0)
						{
							widthMultiplier = (rawString[i + 4] + 256 * rawString[i + 5]) / 100f;
						}
						else if (rawString[i + 1] == 0 && rawString[i + 2] == 3)
						{
							font.SetColor(Color.FromArgb(rawString[i + 7], rawString[i + 4], rawString[i + 5], rawString[i + 6]));
						}
						i += 3 + rawString[i + 3];
						continue;
					}
					if (c == 0xF)
					{
						i += 2;
						widthMultiplier = 1;
						continue;
					}
					font = (c >> 8 == 0xE1) ? fontSisterSymbol : baseFont; // figure out how to merge fonts

					var char_width = font.GetWidthInfo(c).char_width * scale * widthMultiplier;
					if (c == '\n' || x + char_width >= fullWidth)
					{
						x = 0;
						y += font.LineFeed * scale;
						if (c == '\n') continue;
					}
					font.Draw(c, g, x + txtOffsetX, y + txtOffsetY, scale, scale);
					x += char_width;
				}
			}

			pages.Add(bmp);
			Pages = pages;
		}
	}
}