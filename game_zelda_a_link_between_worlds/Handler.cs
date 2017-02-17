using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cetera.Font;
using game_zelda_a_link_between_worlds.Properties;
using KuriimuContract;

namespace game_zelda_a_link_between_worlds
{
	public class Handler : IGameHandler
	{
		#region Properties

		// Information
		public string Name => "A Link Between Worlds";

		public Image Icon => Resources.icon;

		// Feature Support
		public bool HandlerCanGeneratePreviews => true;

        #endregion

        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            // Commands
            ["<pause>"] = "\xE\x0\x1\x0\x8\x0\x2\x0",
            ["<selection>"] = "\xE\x0\x1\x0\x6\x0\x2\x0",

            // Control
            ["Ⓐ"] = "\x0\xE0",
            ["Ⓑ"] = "\x1\xE0",
            ["Ⓧ"] = "\x2\xE0",
            ["Ⓨ"] = "\x3\xE0",
            ["Ⓛ"] = "\x4\xE0",
            ["Ⓡ"] = "\x5\xE0",
            ["DPAD"] = "\x6\xE0",

            // Color
            ["<c-black>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x0\x0",
            ["<c-dkred>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x1\x0",
            ["<c-dkgreen>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x2\x0",
            ["<c-dkyellow>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x3\x0",
            ["<c-dkblue>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x4\x0",
            ["<c-dkmagenta>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x5\x0",
            ["<c-dkcyan>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x6\x0",
            ["<c-ltgray>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x7\x0",
            ["<c-dkgray>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x8\x0",
            ["<c-red>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\x9\x0",
            ["<c-green>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\xA\x0",
            ["<c-yellow>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\xB\x0",
            ["<c-blue>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\xC\x0",
            ["<c-magenta>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\xD\x0",
            ["<c-cyan>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\xE\x0",
            ["<c-white>"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\xF\x0",
            ["<c-default"] = "\xE\x0\x0\x0\x3\x0\x2\x0\x0\xFF\xFF",

            // Variables
            ["<player>"] = "\xE\x0\x1\x0\x0\x0\x0",
            ["<playerb>"] = "\xE\x0\x1\x0\x2\x0\x0",
            ["<username>"] = "\xE\x0\x1\x0\x3\x0\x0",
            ["<message>"] = "\xE\x0\x1\x0\x4\x0\x0",
            ["<medals>"] = "\xE\x0\x1\x0\xA\x0\x0",
            ["<npc>"] = "\xE\x0\x2\x0\x0\x0\x2\x0",
            ["<map>"] = "\xE\x0\x2\x0\x1\x0\x4\x0",
            ["<item>"] = "\xE\x0\x2\x0\x2\x0\x4\x0",
            ["<measure>"] = "\xE\x0\x1\x0\x5\x0\x6\x0",

            // Special
            ["◀"] = "\x36\xE0",
            ["▶"] = "\x37\xE0",
            ["RAVIO"] = "\x5E\xE0",
            ["ROD"] = "\x6B\xE0",
            ["BOW"] = "\x6C\xE0",
            ["BOMB"] = "\x6D\xE0",
            ["←"] = "\x90\x21",
            ["↑"] = "\x91\x21",
            ["→"] = "\x92\x21",
            ["↓"] = "\x93\x21"
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

		Bitmap background = new Bitmap(Resources.template);

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
				// Bitmap textBox = new Bitmap(Resources.top_speaker_bg);
				// ColorMatrix textBoxMatrix = new ColorMatrix(fadeToFiftyPercentMatrix);

				// ImageAttributes textBoxAttributes = new ImageAttributes();
				// textBoxAttributes.SetColorMatrix(textBoxMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

				// Rectangle rectTextBox = new Rectangle(0, img.Height - textBox.Height / 2, img.Width, textBox.Height / 2);
				// gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
				// gfx.DrawImage(textBox, rectTextBox, 0, 0, textBox.Width, textBox.Height, GraphicsUnit.Pixel, textBoxAttributes);

				// Face
				Bitmap face = Resources.icon;
				Rectangle rectFace = new Rectangle(5, img.Height - face.Height - 4, face.Width, face.Height);
				// gfx.DrawImageUnscaled(face, rectFace);

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
				// Bitmap cursor = new Bitmap(Resources.top_speaker);
				// RectangleF rectCursor = new RectangleF(381, 225, 13.9f, 10);
				// gfx.DrawImage(cursor, rectCursor);
			}

			return img;
		}
	}
}