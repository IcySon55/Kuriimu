using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cetera.Font;
using game_maple_story_3ds.Properties;
using KuriimuContract;

namespace game_maple_story_3ds
{
	public class Handler : IGameHandler
	{
		#region Properties

		// Information
		public string Name => "MapleStory Girl of Destiny";

		public Image Icon => Resources.icon;

		// Feature Support
		public bool HandlerCanGeneratePreviews => true;

		#endregion

		Dictionary<string, string> _pairs = new Dictionary<string, string>
		{
			// Control
			["\\["] = "[",
			["\\]"] = "]",
			["[NAME:B]"] = "<PlayerName>",
		};

		Dictionary<string, string> _previewPairs = new Dictionary<string, string>
		{
			// Control
			["\\["] = "[",
			["\\]"] = "]",
			["[NAME:B]"] = "‹NameMugi›",
		};

		BCFNT font;

		public Handler()
		{
			var ms = new MemoryStream();
			new GZipStream(new MemoryStream(Resources.MainFont_bcfnt), CompressionMode.Decompress).CopyTo(ms);
			ms.Position = 0;
			font = new BCFNT(ms);
		}

		public string GetKuriimuString(string rawString)
		{
			return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
		}

		public string GetRawString(string kuriimuString)
		{
			return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
		}

		public int Page { get; set; }
		public int PageCount { get; private set; }

		Bitmap background = new Bitmap(Resources.background);
		Bitmap textBox = new Bitmap(Resources.blank);

		public Bitmap GeneratePreview(IEntry entry)
		{
			string rawString = entry.EditedText;
			int boxes = rawString.Count(c => c == '\n') / 3 + 1;

			const int txtOffsetX = 2;
			const int txtOffsetY = 2;
			Bitmap img = new Bitmap(400, Math.Max((txtOffsetY + textBox.Height) * boxes + txtOffsetY, background.Height));

			using (Graphics gfx = Graphics.FromImage(img))
			{
				gfx.SmoothingMode = SmoothingMode.HighQuality;
				gfx.InterpolationMode = InterpolationMode.Bicubic;
				gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

				gfx.DrawImage(background, 0, 0);

				for (int i = 0; i < boxes; i++)
					gfx.DrawImage(textBox, txtOffsetX, (txtOffsetY + textBox.Height) * i + txtOffsetY);

				float scale = 0.625f;
				float x = 10, y = 22;
				int line = 0;
				string str = _previewPairs.Aggregate(rawString, (s, pair) => s.Replace(pair.Key, pair.Value));
				font.SetColor(Color.White);

				foreach (char c in str)
				{
					switch (c)
					{
						case '‹': // Player Name
							font.SetColor(Color.FromArgb(255, 254, 254, 149));
							continue;
						case '<': // Orange
							font.SetColor(Color.FromArgb(255, 255, 150, 0));
							continue;
						case '{':  // Green
							font.SetColor(Color.FromArgb(255, 131, 237, 63));
							continue;
						case '[': // Purple
							font.SetColor(Color.FromArgb(255, 255, 100, 255));
							continue;
						case '›': // Reset color
						case '>':
						case '}':
						case ']':
							font.SetColor(Color.White);
							continue;
						case '\n':
							x = 10;
							y += 16;
							if (++line % 3 == 0)
								y += 33;
							continue;
					}

					// Otherwise it's a regular drawable character
					font.Draw(c, gfx, x, y, scale, scale);
					x += font.GetWidthInfo(c).char_width * scale;
				}

			}
			return img;
		}
	}
}
