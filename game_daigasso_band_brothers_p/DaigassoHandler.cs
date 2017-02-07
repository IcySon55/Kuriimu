using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game_daigasso_band_brothers_p.Properties;
using KuriimuContract;

namespace game_daigasso_band_brothers_p
{
	class DaigassoHandler : IGameHandler
	{
		BCFNT fontBasic;
		BCFNT fontSymbol;

		// Information
		public string Name => "Jam With The Band P";
		public Image Icon => Resources.music;

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
			["\uE077"] = "◎",
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

		public DaigassoHandler()
		{
			var ms = new MemoryStream();
			new GZipStream(new MemoryStream(Resources.Basic_bcfnt), CompressionMode.Decompress).CopyTo(ms);
			ms.Position = 0;
			fontBasic = new BCFNT(ms);
			ms = new MemoryStream();
			new GZipStream(new MemoryStream(Resources.SisterSymbol_bcfnt), CompressionMode.Decompress).CopyTo(ms);
			ms.Position = 0;
			fontSymbol = new BCFNT(ms);
		}

		public string GetKuriimuString(string str)
		{
			str = symbols.Aggregate(str, (s, kvp) => s.Replace(kvp.Key, kvp.Value));
			Func<string, byte[], string> Fix = (id, bytes) =>
			{
				switch (id)
				{
					case "\0\x2":
						return $"s{BitConverter.ToInt16(bytes, 0)}";
					case "\0\x3":
						return "c" + colours[BitConverter.ToUInt32(bytes, 0)];
					case "\x1\x0":
						return "v";
					case "\x1\x7":
						return "v" + Encoding.Unicode.GetString(bytes).Substring(1);
					case "\x2\0":
						return $"w{BitConverter.ToInt16(bytes, 0)}";
					case "\x2\x1":
						return $"h{BitConverter.ToInt16(bytes, 0)}";
					default:
						var data = Encoding.Unicode.GetString(bytes);
						return data.Last() + string.Join(",", id.Skip(1).Concat(data).Take(data.Length).Select(c => (short)c));
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
			return str;
		}

		public string GetRawString(string s) => s;

		public Bitmap GeneratePreview(string s)
		{
			var bmp = new Bitmap(Resources.daigasso_box);
			using (var g = Graphics.FromImage(bmp))
			{
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.InterpolationMode = InterpolationMode.Bicubic;
				float txtOffsetX = 32, txtOffsetY = 12;
				float x = 0, y = 0;
				const float scale = 0.6f;
				foreach (var c in s)
				{
					var font = (c >> 8 == 0xE1) ? fontSymbol : fontBasic; // figure out how to merge fonts

					var char_width = font.GetWidthInfo(c).char_width * scale;
					if (c == '\n' || x + char_width >= 336)
					{
						x = 0;
						y += font.LineFeed * scale;
						if (c == '\n') continue;
					}
					font.DrawCharacter(c, g, x + txtOffsetX, y + txtOffsetY, scale);
					x += char_width;
				}
			}
			return bmp;
		}
	}
}
