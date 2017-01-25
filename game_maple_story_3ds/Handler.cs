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
			["\\[NAME:B\\]"] = "PlayerName"
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
			Bitmap textBox = new Bitmap(Resources.blank);
			int boxes = (int)Math.Ceiling((double)rawString.Split('\n').Length / 3);

			Bitmap img = new Bitmap(textBox.Width, textBox.Height * boxes);

			using (Graphics gfx = Graphics.FromImage(img))
			{
				gfx.SmoothingMode = SmoothingMode.HighQuality;
				gfx.InterpolationMode = InterpolationMode.Bicubic;

				for (int i = 0; i < boxes; i++)
					gfx.DrawImage(textBox, 0, i * textBox.Height);

				Rectangle rectText = new Rectangle(8, 19, 400, 80);

				string str = rawString.Replace("\\[NAME:B\\]", "‹NameMugi›");
				float scaleDefault = 0.625f;
				float scaleCurrent = scaleDefault;
				float x = rectText.X, pX = x;
				float y = rectText.Y, pY = y;
				float yAdjust = -2;
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
					if (c == '<') // Orange
					{
						colorCurrent = Color.FromArgb(255, 255, 150, 0);
						continue;
					}
					else if (c == '{') // Green
					{
						colorCurrent = Color.FromArgb(255, 131, 237, 63);
						continue;
					}
					else if (c == '\\' && c2 == '[') // Purple
					{
						colorCurrent = Color.FromArgb(255, 255, 100, 255);
						i++;
						continue;
					}
					else if (c == '>' || c == '}' || c == '›') // Reset
					{
						colorCurrent = colorDefault;
						continue;
					}
					else if (c == '\\' && c2 == ']') //Reset
					{
						colorCurrent = colorDefault;
						i++;
						continue;
					}
					else if (c == '‹') //Player name
					{
						colorCurrent = Color.FromArgb(255, 254, 254, 149);
						continue;
					}
					else if (c == '\n' || x + (bfc.Width * scaleCurrent) - rectText.X > rectText.Width) // New Line/End of Textbox (does this exist in maple? oh well leaving it in)
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
						y = rectText.Y + textBox.Height * box;
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