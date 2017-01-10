using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using game_rocket_slime_3ds.Properties;
using KuriimuContract;

namespace game_rocket_slime_3ds
{
	public class Handler : IGameHandler
	{
		Dictionary<string, string> _pairs = new Dictionary<string, string>
		{
			// Control
			["<prompt>"] = "\x1F\x20",
			["<player>"] = "\x1F\x05",
			["<name>"] = "\x1F\x02",
			["</name>"] = "\x02",
			["<top?>"] = "\x1F\x0100",
			["<middle?>"] = "\x1F\x0103",
			["<bottom?>"] = "\x1F\x0200",
			["<u1>"] = "\x1F\x00",
			["<next>"] = "\x1F\x15",
			["<end>"] = "\x1F\x0115",
			["<u3>"] = "\x17",

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
		public string Name => "Rocket Slime 3DS";

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

			Bitmap img = new Bitmap(Resources.blank_top);

			Graphics gfx = Graphics.FromImage(img);
			gfx.SmoothingMode = SmoothingMode.HighQuality;
			gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

			Rectangle rectName = new Rectangle(33, 3, 114, 15);
			Rectangle rectText = new Rectangle(32, 21, 366, 60);

			string str = rawString.Replace(_pairs["<player>"], "Player");
			double scaleDefault = 1.06;
			double scaleName = 0.86;
			double scaleCurrent = scaleDefault;
			int x = rectText.X, pX = x;
			int y = rectText.Y, pY = y;
			int yAdjust = 3;
			Color colorDefault = Color.FromArgb(255, 37, 66, 167);
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
				if (c == 0x001F && c2 == 0x0002) // Start Name
				{
					colorCurrent = Color.White;
					scaleCurrent = scaleName;
					int width = bfh.MeasureString(str.Substring(i + 2), (char)0x0002, scaleCurrent);
					pX = x;
					pY = y;
					x = rectName.X + (rectName.Width / 2) - (width / 2);
					y = rectName.Y;
					i++;
					continue;
				}
				else if (c == 0x001F && (c2 == 0x0000 || c2 == 0x0100 || c2 == 0x0200 || c2 == 0x0103 || c2 == 0x0020 || c2 == 0x0115)) // Unknown/No Render Effect
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
				else if (c == 0x0002) // End Name
				{
					colorCurrent = colorDefault;
					scaleCurrent = scaleDefault;
					x = pX;
					y = pY;
					continue;
				}
				else if (c == '\n' || x + bfc.Width - rectText.X > rectText.Width) // New Line/End of Textbox
				{
					x = rectText.X;
					y += bfc.Character.Height + yAdjust;
					continue;
				}

				// Draw character
				gfx.DrawImage(bfh.GetCharacter(c, colorCurrent).Character, x - bfc.Offset, y, (int)(bfh.CharacterWidth * scaleCurrent), (int)(bfh.CharacterHeight * scaleCurrent));
				x += (int)(bfc.Width * scaleCurrent);
			}

			return img;
		}
	}
}