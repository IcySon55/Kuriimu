using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using game_time_travelers.Properties;
using KuriimuContract;

namespace game_time_travelers
{
	public class Handler : IGameHandler
	{
		#region Properties

		// Information
		public string Name => "Time Travelers";

		public Image Icon => Resources.icon;

		// Feature Support
		public bool HandlerCanGeneratePreviews => true;

		#endregion

		Dictionary<string, string> _pairs = new Dictionary<string, string>
		{
			["\x5b"] = "[",
			["\x5d"] = "]",
			["\x2f"] = "/",
			["\x5c\x6e"] = "\r\n",
			["\x00"] = " ",
		};

		// Loading the font takes up some time so we defer it to a lazy initializer
		static Lazy<XF> fontInitializer = new Lazy<XF>(() => new XF(new MemoryStream(Resources.MainFont_xf)));
		static XF font => fontInitializer.Value;

		public string GetKuriimuString(string rawString)
		{
			return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
		}

		public string GetRawString(string kuriimuString)
		{
			return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
		}

		Bitmap textBox = new Bitmap(Resources.blank_top);

		public Bitmap GeneratePreview(string rawString)
		{
			string kuriimuString = GetKuriimuString(rawString);
			int boxes = kuriimuString.Count(c => c == (char)0x17) + 1;

			Bitmap img = new Bitmap(textBox.Width, textBox.Height * boxes);

			using (Graphics gfx = Graphics.FromImage(img))
			{
				gfx.SmoothingMode = SmoothingMode.HighQuality;
				gfx.InterpolationMode = InterpolationMode.Bicubic;
				gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

				for (int i = 0; i < boxes; i++)
					gfx.DrawImage(textBox, 0, i * textBox.Height);

				RectangleF rectText = new RectangleF(10, 10, 370, 100);

				Color colorDefault = Color.FromArgb(255, 255, 255, 255);

				float scaleDefault = 1.0f;
				//float scaleName = 0.86f;
				float scaleCurrent = scaleDefault;
				float x = rectText.X, pX = x;
				float y = rectText.Y, pY = y;
				//float yAdjust = 4;
				//int box = 0;

				//font.SetTextColor(colorDefault);

				bool furigana = false;
				bool furiganaUpper = false;
				float bkX = 0;
				for (int i = 0; i < kuriimuString.Length; i++)
				{
					bool notEOS = i + 2 < kuriimuString.Length;
					char c = kuriimuString[i];
					char c2 = ' ';
					if (notEOS)
						c2 = kuriimuString[i + 1];

					//handle non-character codes
					if (c.ToString() == "\n")
					{
						y += 27;
						x = 10;
						continue;
					}
					else if (c.ToString() == "\r")
					{
						continue;
					}
					else if (c.ToString() == "[")
					{
						bkX = x;
						furigana = true;
						continue;
					}
					else if (c.ToString() == "/")
					{
						if (furigana)
						{
							furiganaUpper = true;
							y -= 7;
							x = bkX;
							continue;
						}
					}
					else if (c.ToString() == "]")
					{
						furiganaUpper = false;
						furigana = false;
						y += 7;
						continue;
					}

					XF.CharacterMap charMap = font.GetCharacterMap(c, furiganaUpper);

					//draw regular character
					font.DrawCharacter(c, colorDefault, gfx, x, y, furiganaUpper);
					x += charMap.CharWidth;
				}
			}

			return img;
		}
	}
}