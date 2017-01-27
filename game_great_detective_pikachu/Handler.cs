using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game_great_detective_pikachu.Properties;
using KuriimuContract;

namespace game_great_detective_pikachu
{
	public class Handler : IGameHandler
	{
		Dictionary<string, string> _pairs = new Dictionary<string, string>
		{
			// Control
			["Ⓐ"] = "\xE\x1\x0\x1\x0",
			["Ⓨ"] = "\xE\x1\x0\x1\x3",
			["◎"] = "\xE\x1\x0\x1\x4",
			["👁️"] = "\xE\x1\x0\x1\x7",

			// Color
			["<color-red>"] = "\xE\x0\x3\x4\xFF\x4B\x4B\xFF",
			["<color-white>"] = "\xE\x0\x3\x4\xFD\xFD\xFD\xFF",

			// Special
			["…"] = "\x85"
		};

		#region Properties

		// Information
		public string Name => "Great Detective Pikachu";

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

			Bitmap background = new Bitmap(Resources.background1);

			Graphics gfx = Graphics.FromImage(background);
			gfx.SmoothingMode = SmoothingMode.HighQuality;
			gfx.InterpolationMode = InterpolationMode.Bicubic;
			gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

			float[][] fadeToFiftyPercentMatrix = {
				new float[] { 1, 0, 0, 0, 0 },
				new float[] { 0, 1, 0, 0, 0 },
				new float[] { 0, 0, 1, 0, 0 },
				new float[] { 0, 0, 0, 0.5f, 0 },
				new float[] { 0, 0, 0, 0, 1 },
			};

			// Textbox
			Bitmap textBox = new Bitmap(Resources.top_speaker_bg);
			ColorMatrix textBoxMatrix = new ColorMatrix(fadeToFiftyPercentMatrix);

			ImageAttributes textBoxAttributes = new ImageAttributes();
			textBoxAttributes.SetColorMatrix(textBoxMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

			Rectangle rectTextBox = new Rectangle(0, background.Height - textBox.Height / 2, background.Width, textBox.Height / 2);
			gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
			gfx.DrawImage(textBox, rectTextBox, 0, 0, textBox.Width, textBox.Height, GraphicsUnit.Pixel, textBoxAttributes);

			// Face
			Bitmap face = Resources.face_icon_pikachu_surprised;
			Rectangle rectFace = new Rectangle(4, background.Height - face.Height - 4, face.Width, face.Height);
			gfx.DrawImageUnscaled(face, rectFace);

			// Text
			gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

			Rectangle rectText = new Rectangle(rectFace.X + rectFace.Width + 8, rectFace.Y + 10, 366, 60);

			string str = rawString;
			float scaleDefault = 0.64f;
			float scaleCurrent = scaleDefault;
			float x = rectText.X, pX = x;
			float y = rectText.Y, pY = y;
			float yAdjust = 3;
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

				//// Handle control codes
				//if (c == 0x001F && c2 == 0x0002) // Start Name
				//{
				//	colorCurrent = Color.White;
				//	scaleCurrent = scaleName;
				//	int width = bfh.MeasureString(str.Substring(i + 2), (char)0x0002, scaleCurrent);
				//	pX = x;
				//	pY = y;
				//	x = rectName.X + (rectName.Width / 2) - (width / 2);
				//	y = rectName.Y;
				//	i++;
				//	continue;
				//}
				//else if (c == 0x001F && (c2 == 0x0000 || c2 == 0x0100 || c2 == 0x0200 || c2 == 0x0103 || c2 == 0x0020 || c2 == 0x0115)) // Unknown/No Render Effect
				//{
				//	i++;
				//	continue;
				//}
				//else if (c == 0x0013 && c2 == 0x0000) // Default
				//{
				//	colorCurrent = colorDefault;
				//	i++;
				//	continue;
				//}
				//else if (c == 0x0013 && c2 == 0x0001) // Red
				//{
				//	colorCurrent = Color.Red;
				//	i++;
				//	continue;
				//}
				//else if (c == 0x0013 && c2 == 0x0003) // Light Blue
				//{
				//	colorCurrent = Color.FromArgb(255, 54, 129, 216);
				//	i++;
				//	continue;
				//}
				//else if (c == 0x001F && c2 == 0x0015) // End Dialog
				//{
				//	i++;
				//	continue;
				//}
				//else if (c == 0x0017) // End End Dialog?
				//	continue;
				//else if (c == 0x0002) // End Name
				//{
				//	colorCurrent = colorDefault;
				//	scaleCurrent = scaleDefault;
				//	x = pX;
				//	y = pY;
				//	continue;
				//}
				if (c == '\n' || x + (bfc.Width * scaleCurrent) - rectText.X > rectText.Width) // New Line/End of Textbox
				{
					x = rectText.X;
					y += (bfc.Character.Height * scaleCurrent) + yAdjust;
					if (c == '\n')
						continue;
				}

				// Draw character
				gfx.DrawImage(bfh.GetCharacter(c, Color.SaddleBrown).Character, x - bfc.Offset * scaleCurrent + 2f, y + 2f, bfh.CharacterWidth * scaleCurrent, bfh.CharacterHeight * scaleCurrent);
				gfx.DrawImage(bfh.GetCharacter(c, colorCurrent).Character, x - bfc.Offset * scaleCurrent, y, bfh.CharacterWidth * scaleCurrent, bfh.CharacterHeight * scaleCurrent);
				x += bfc.Width * scaleCurrent;
			}

			// Cursor
			Bitmap cursor = new Bitmap(Resources.top_speaker);
			Rectangle rectCursor = new Rectangle(background.Width - cursor.Width - 10, background.Height - cursor.Height - 10, cursor.Width, cursor.Height);
			gfx.DrawImage(cursor, rectCursor);

			return background;
		}
	}
}