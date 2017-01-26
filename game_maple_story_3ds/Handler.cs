using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
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

			// Special
			["…"] = "\x85",
			["‘"] = "\x91",
			["’"] = "\x92",
			["“"] = "\x93",
			["”"] = "\x94",
			["Ⅰ"] = "\x95",
			["Ⅱ"] = "\x96",
			["Ⅲ"] = "\x97",
		};

		BitmapFontHandler bfh = new BitmapFontHandler(Resources.MainFont);

		public string GetKuriimuString(string rawString)
		{
			return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
		}

		public string GetRawString(string kuriimuString)
		{
			return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
		}

		public Bitmap GeneratePreview(string rawString)
		{
			Bitmap background = new Bitmap(Resources.background);

			Bitmap textBox = new Bitmap(Resources.blank);
			int boxes = (int)Math.Ceiling((double)rawString.Split('\n').Length / 3);

			int txtOffsetX = 2, txtOffsetY = 2;
			Bitmap img = new Bitmap(400, Math.Max(textBox.Height * boxes, background.Height) + (boxes * txtOffsetY) + txtOffsetY);

			using (Graphics gfx = Graphics.FromImage(img))
			{
				gfx.SmoothingMode = SmoothingMode.HighQuality;
				gfx.InterpolationMode = InterpolationMode.Bicubic;
				gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

				gfx.DrawImage(new Bitmap(Resources.background), 0, 0);

				for (int i = 0; i < boxes; i++)
					gfx.DrawImage(textBox, 0 + txtOffsetX, (i * textBox.Height) + ((i + 1) * txtOffsetY));

				Rectangle rectText = new Rectangle(8 + txtOffsetX, 19 + txtOffsetY, 400, 80);

				string str = _previewPairs.Aggregate(rawString, (s, pair) => s.Replace(pair.Key, pair.Value));
				float scaleDefault = 0.625f;
				float scaleCurrent = scaleDefault;
				float x = rectText.X, pX = x;
				float y = rectText.Y, pY = y;
				float yAdjust = -2.75f;
				int box = 0, line = 0;
				bool newBox = true;
				Color colorDefault = Color.FromArgb(255, 255, 255, 255);
				Color colorCurrent = colorDefault;

				for (int i = 0; i < str.Length; i++)
				{
					bool notEOS = i + 2 < str.Length;
					char c = str[i];

					char c2 = ' ';
					if (notEOS)
						c2 = str[i + 1];

					BitmapFontCharacter bfc = bfh.GetCharacter(c);

					// Handle control codes
					if (c == '‹') // Player Name
					{
						colorCurrent = Color.FromArgb(255, 254, 254, 149);
						continue;
					}
					else if (c == '<') // Orange
					{
						colorCurrent = Color.FromArgb(255, 255, 150, 0);
						continue;
					}
					else if (c == '{') // Green
					{
						colorCurrent = Color.FromArgb(255, 131, 237, 63);
						continue;
					}
					else if (c == '[') // Purple
					{
						colorCurrent = Color.FromArgb(255, 255, 100, 255);
						continue;
					}
					else if (c == '>' || c == '}' || c == '›' || c == ']') // Reset
					{
						colorCurrent = colorDefault;
						continue;
					}
					else if (c == '\n') // The previewer will not break to a new line when the line hits the end of the text box since the game doesn't do this.
					{
						line++;
						x = rectText.X;
						y += (bfc.Character.Height * scaleCurrent) + yAdjust;
						if (c == '\n')
							continue;
					}

					if (!newBox && line % 3 == 0) // Next TextBox
					{
						box++;
						newBox = true;
						x = rectText.X;
						pX = x;
						y = rectText.Y + textBox.Height * box + (txtOffsetY * box);
						pY += y;
					}
					else if (line % 3 != 0)
						newBox = false;

					// Draw character
					gfx.DrawImage(bfh.GetCharacter(c, colorCurrent).Character, x - bfc.Offset * scaleCurrent, y, bfh.CharacterWidth * scaleCurrent, bfh.CharacterHeight * scaleCurrent);
					x += bfc.Width * scaleCurrent;
				}
			}

			return img;
		}
	}
}