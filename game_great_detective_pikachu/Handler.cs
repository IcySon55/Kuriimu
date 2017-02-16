using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cetera.Font;
using game_great_detective_pikachu.Properties;
using KuriimuContract;

namespace game_great_detective_pikachu
{
	public class Handler : IGameHandler
	{
		#region Properties

		// Information
		public string Name => "Great Detective Pikachu";

		public Image Icon => Resources.icon;

		// Feature Support
		public bool HandlerCanGeneratePreviews => true;

		#endregion

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
			return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Value, pair.Key));
		}

		public string GetRawString(string kuriimuString)
		{
			return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Key, pair.Value));
		}

		public int Page { get; set; }
		public int PageCount { get; private set; }

		Bitmap background = new Bitmap(Resources.background1);

		public Bitmap GeneratePreview(IEntry entry)
		{
			string rawString = entry.EditedText;
			Bitmap img = new Bitmap(background.Width, background.Height);

			using (Graphics gfx = Graphics.FromImage(img))
			{
				gfx.SmoothingMode = SmoothingMode.HighQuality;
				gfx.InterpolationMode = InterpolationMode.Bicubic;
				gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

				gfx.DrawImage(background, 0, 0);

				float[][] fadeToFiftyPercentMatrix = {
					new float[] { 1, 0, 0, 0, 0 },
					new float[] { 0, 1, 0, 0, 0 },
					new float[] { 0, 0, 1, 0, 0 },
					new float[] { 0, 0, 0, 0.45f, 0 },
					new float[] { 0, 0, 0, 0, 1 },
				};

				// Textbox
				Bitmap textBox = new Bitmap(Resources.top_speaker_bg);
				ColorMatrix textBoxMatrix = new ColorMatrix(fadeToFiftyPercentMatrix);

				ImageAttributes textBoxAttributes = new ImageAttributes();
				textBoxAttributes.SetColorMatrix(textBoxMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				Rectangle rectTextBox = new Rectangle(0, img.Height - textBox.Height / 2, img.Width, textBox.Height / 2);
				gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
				gfx.DrawImage(textBox, rectTextBox, 0, 0, textBox.Width, textBox.Height, GraphicsUnit.Pixel, textBoxAttributes);

				// Face
				Bitmap face = Resources.face_icon_pikachu_surprised;
				Rectangle rectFace = new Rectangle(5, img.Height - face.Height - 4, face.Width, face.Height);
				gfx.DrawImageUnscaled(face, rectFace);

				// Text
				Rectangle rectText = new Rectangle(rectFace.X + rectFace.Width + 9, rectFace.Y + 12, 366, 60);

				float scale = 1.0f;
				float x = rectText.X, y = rectText.Y;
				int line = 0;

				string str = rawString;
				font.SetColor(Color.White);

				gfx.InterpolationMode = InterpolationMode.Bicubic;
				foreach (char c in str)
				{
					switch (c)
					{
						case '\n':
							x = rectText.X;
							y += rectText.Y;
							if (++line % 3 == 0)
								y += 33;
							continue;
					}

					font.SetColor(Color.FromArgb(255, 63, 3, 3));
					font.Draw(c, gfx, x + 2f, y + 2f, scale, scale);
					font.SetColor(Color.White);
					font.Draw(c, gfx, x, y, scale, scale);
					x += font.GetWidthInfo(c).char_width * scale;
				}

				// Cursor
				Bitmap cursor = new Bitmap(Resources.top_speaker);
				RectangleF rectCursor = new RectangleF(381, 225, 13.9f, 10);
				gfx.DrawImage(cursor, rectCursor);
			}

			return img;
		}
	}
}