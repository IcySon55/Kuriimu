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
			["\x5c\x6e"] = "\r\n"
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

		public IList<Bitmap> Pages { get; private set; } = new List<Bitmap>();

		Bitmap textBox = new Bitmap(Resources.blank_top);

		public void GeneratePages(IEntry entry)
		{
			var pages = new List<Bitmap>();

			string kuriimuString = GetKuriimuString(entry.EditedText);
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
				float furiganaLowerWidth = 0;
				float furiganaUpperWidth = 0;
				float lowerOffset = 0;
				float upperOffset = 0;
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

						//getting char length from lower and upper furigana string in px
						furiganaLowerWidth = 0;
						furiganaUpperWidth = 0;
						int tmp = i + 1;
						bool fin = false;
						bool furiganaTmp = false;
						bool draw = false;
						while (fin == false)
						{
							if (tmp >= kuriimuString.Length)
							{
								if (fin == false)
								{
									draw = true;
								}
								break;
							}

							if (kuriimuString[tmp].ToString() == "/")
							{
								furiganaTmp = true;
								tmp++;
							}
							else if (kuriimuString[tmp].ToString() == "]")
							{
								fin = true;
								if (furiganaTmp == false)
								{
									draw = true;
								}
							}
							else
							{
								XF.CharacterMap charMaptmp = font.GetCharacterMap(kuriimuString[tmp++], furiganaTmp);
								var t1 = (furiganaTmp) ? furiganaUpperWidth += charMaptmp.CharWidth : furiganaLowerWidth += charMaptmp.CharWidth;
							}
						}

						//setting furigana offset
						var t2 = (furiganaLowerWidth > furiganaUpperWidth) ? upperOffset = (furiganaLowerWidth - furiganaUpperWidth) / 2 : lowerOffset = (furiganaUpperWidth - furiganaLowerWidth) / 2;
						if (furiganaLowerWidth < furiganaUpperWidth)
						{
							x += lowerOffset;
						}

						if (draw == false)
						{
							continue;
						}
					}
					else if (c.ToString() == "/")
					{
						if (furigana)
						{
							furiganaUpper = true;
							y -= 7;
							var t3 = (furiganaUpperWidth < furiganaLowerWidth) ? x = bkX + upperOffset : x = bkX;
							continue;
						}
					}
					else if (c.ToString() == "]")
					{
						if (furiganaUpper)
						{
							var t4 = (furiganaUpperWidth > furiganaLowerWidth) ? x = bkX + furiganaUpperWidth : x = bkX + furiganaLowerWidth;
							y += 7;
							furiganaUpper = false;
							furigana = false;
							upperOffset = 0;
							lowerOffset = 0;
							continue;
						}
					}

					XF.CharacterMap charMap = font.GetCharacterMap(c, furiganaUpper);

					//draw regular character
					font.DrawCharacter(c, colorDefault, gfx, x, y, furiganaUpper);
					x += charMap.CharWidth;
				}
			}

			pages.Add(img);
			Pages = pages;
		}

		// Settings
		public bool ShowWhitespace
		{
			get { return Settings.Default.ShowWhitespace; }
			set
			{
				Settings.Default.ShowWhitespace = value;
				Settings.Default.Save();
			}
		}
	}
}