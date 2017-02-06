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
			["\x1F\x02"] = "<name>",
		};

		XF font;

		public Handler()
		{
			var ms = new MemoryStream(Resources.MainFont_xf);
			ms.Position = 0;
			font = new XF(ms);
		}

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
			int boxes = rawString.Count(c => c == (char)0x17) + 1;

			Bitmap img = new Bitmap(textBox.Width, textBox.Height * boxes);

			using (Graphics gfx = Graphics.FromImage(img))
			{
				gfx.SmoothingMode = SmoothingMode.HighQuality;
				gfx.InterpolationMode = InterpolationMode.Bicubic;
				gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

				for (int i = 0; i < boxes; i++)
					gfx.DrawImage(textBox, 0, i * textBox.Height);

				RectangleF rectName = new RectangleF(15, 3, 114, 15);
				RectangleF rectText = new RectangleF(15, 22, 370, 60);

				Color colorDefault = Color.FromArgb(255, 37, 66, 167);

				float scaleDefault = 1.0f;
				float scaleName = 0.86f;
				float scaleCurrent = scaleDefault;
				float x = rectText.X, pX = x;
				float y = rectText.Y, pY = y;
				float yAdjust = 4;
				int box = 0;

				string str = rawString.Replace("\x1F\x05", "Player");
				font.SetTextColor(colorDefault);

				for (int i = 0; i < str.Length; i++)
				{
					bool notEOS = i + 2 < str.Length;
					char c = str[i];
					char c2 = ' ';
					if (notEOS)
						c2 = str[i + 1];

					BCFNT.CharWidthInfo widthInfo = font.GetWidthInfo(c);

					// Handle control codes
					if (c == 0x001F && c2 == 0x0002) // Start Name
					{
						font.SetTextColor(Color.White);
						scaleCurrent = scaleName;
						float width = font.MeasureString(str.Substring(i + 2), (char)0x0002, scaleCurrent);
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
						font.SetTextColor(colorDefault);
						i++;
						continue;
					}
					else if (c == 0x0013 && c2 == 0x0001) // Red
					{
						font.SetTextColor(Color.Red);
						i++;
						continue;
					}
					else if (c == 0x0013 && c2 == 0x0003) // Light Blue
					{
						font.SetTextColor(Color.FromArgb(255, 54, 129, 216));
						i++;
						continue;
					}
					else if (c == 0x001F && c2 == 0x0015) // End Dialog
					{
						i++;
						continue;
					}
					else if (c == 0x0017) // Next TextBox
					{
						box++;
						x = rectText.X;
						pX = x;
						y = rectText.Y + textBox.Height * box;
						pY += y;
						continue;
					}
					else if (c == 0x0002) // End Name
					{
						font.SetTextColor(colorDefault);
						scaleCurrent = scaleDefault;
						x = pX;
						y = pY;
						continue;
					}
					else if (c == '\n' || x + (widthInfo.char_width * scaleCurrent) - rectText.X > rectText.Width) // New Line/End of Textbox
					{
						x = rectText.X;
						y += (16 * scaleCurrent) + yAdjust;
						if (c == '\n')
							continue;
					}

					// Otherwise it's a regular drawable character
					font.DrawCharacter(c, gfx, x, y, scaleCurrent);
					x += widthInfo.char_width * scaleCurrent;
				}
			}

			return img;
		}
	}
}