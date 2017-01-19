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
		Dictionary<string, string> _pairs = new Dictionary<string, string>
		{
			// Control
			["PlayerName"] = "[NAME:B\\]",

			// Color
			["<color-default>"] = "\x13\x00",
			["<color-red>"] = "\x13\x01",
			["<color-???>"] = "\x13\x02",
			["<color-blue>"] = "\x13\x03",

			// Special
			["…"] = "\x85",
			["©"] = "\x86",
			["♥"] = "\x87"
		};

		#region Properties

		// Information
		public string Name => "MapleStory Girl of Destiny";

		public Image Icon => Resources.icon;

		// Feature Support
		public bool HandlerCanGeneratePreviews => true;

		#endregion

		public string GetKuriimuString(string rawString)
		{
			return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Value, pair.Key));
		}

		public string GetRawString(string kuriimuString)
		{
			return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Key, pair.Value));
		}

		public Bitmap GeneratePreview(string rawString)
		{
			BitmapFontHandler bfh = new BitmapFontHandler(Resources.MainFont);

			Bitmap img = new Bitmap(Resources.blank);

			Graphics gfx = Graphics.FromImage(img);
			gfx.SmoothingMode = SmoothingMode.HighQuality;
			gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

			
			Rectangle rectText = new Rectangle(15, 21, 370, 60);

			string str = rawString.Replace(_pairs["PlayerName"], "NameMugi");
			float scaleDefault = 0.5f;
			float scaleCurrent = scaleDefault;
			float x = rectText.X, pX = x;
			float y = rectText.Y, pY = y;
			float yAdjust = 1;
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
				
				if (c == 0x001F && (c2 == 0x0000 || c2 == 0x0100 || c2 == 0x0200 || c2 == 0x0103 || c2 == 0x0020 || c2 == 0x0115)) // Unknown/No Render Effect
				{
					i++;
					continue;
				}
				else if (c == 0x0013 && c2 == 0x0000) // Default
				{
					colorCurrent = colorDefault;
					i++;
					continue;
				}
				else if (c == 0x0013 && c2 == 0x0001) // Red
				{
					colorCurrent = Color.Red;
					i++;
					continue;
				}
				else if (c == 0x0013 && c2 == 0x0003) // Light Blue
				{
					colorCurrent = Color.FromArgb(255, 54, 129, 216);
					i++;
					continue;
				}
				else if (c == 0x001F && c2 == 0x0015) // End Dialog
				{
					i++;
					continue;
				}
				else if (c == 0x0017) // End End Dialog?
					continue;
				else if (c == '\n' || x + (bfc.Width * scaleCurrent) - rectText.X > rectText.Width) // New Line/End of Textbox
				{
					x = rectText.X;
					y += (bfc.Character.Height * scaleCurrent) + yAdjust;
					if (c == '\n')
						continue;
				}

				// Draw character
				gfx.DrawImage(bfh.GetCharacter(c, colorCurrent).Character, x - bfc.Offset * scaleCurrent, y, bfh.CharacterWidth * scaleCurrent, bfh.CharacterHeight * scaleCurrent);
				x += bfc.Width * scaleCurrent;
			}

			return img;
		}
	}
}