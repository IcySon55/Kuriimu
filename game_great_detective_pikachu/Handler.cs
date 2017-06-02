using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using Cetera.Font;
using game_great_detective_pikachu.Properties;
using Kuriimu.Contract;

namespace game_great_detective_pikachu
{
    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "Great Detective Pikachu";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => false;
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
            ["…"] = "\x85",
            ["？"] = "?"
        };

        BCFNT font => BCFNT.StandardFont;

        public string GetKuriimuString(string rawString)
        {
            return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        public string GetRawString(string kuriimuString)
        {
            return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Key, pair.Value));
        }

        Bitmap background = new Bitmap(Resources.background);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry?.EditedText == null) return pages;

            string rawString = _pairs.Aggregate(entry.EditedText, (str, pair) => str.Replace(pair.Value, pair.Key));

            foreach (string page in rawString.Split('\n'))
            {
                if (page.Trim() != string.Empty)
                {
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
                            new float[] { 0, 0, 0, 0.6f, 0 },
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
                        Rectangle rectText = new Rectangle(rectFace.X + rectFace.Width + 8, rectFace.Y + 10, 366, 60);

                        float scale = 0.775f;
                        float x = rectText.X, y = rectText.Y;
                        bool control = false;
                        string parameter = string.Empty;
                        Color backgroundBrown = Color.FromArgb(255, 64, 0, 0);
                        Color foregroundRed = Color.FromArgb(255, 255, 75, 75);

                        Color foregroundColor = Color.White;
                        Color backgroundColor = backgroundBrown;
                        gfx.InterpolationMode = InterpolationMode.Bicubic;
                        foreach (char c in page)
                        {
                            if (!control)
                                switch (c)
                                {
                                    case '\n':
                                        x = rectText.X;
                                        y += rectText.Y;
                                        continue;
                                    case '<':
                                        control = true;
                                        parameter += c;
                                        continue;
                                }
                            else
                            {
                                parameter += c;
                                if (c == ' ')
                                {
                                    control = false;
                                    parameter = string.Empty;
                                }
                            }

                            switch (parameter)
                            {
                                case "<color-red>":
                                    foregroundColor = foregroundRed;
                                    control = false;
                                    parameter = string.Empty;
                                    continue;
                                case "<color-white>":
                                    foregroundColor = Color.White;
                                    control = false;
                                    parameter = string.Empty;
                                    continue;
                            }

                            if (!control)
                            {
                                font.SetColor(backgroundColor);
                                font.Draw(c, gfx, x + 1.5f, y + 1f, scale, scale);
                                font.SetColor(foregroundColor);
                                font.Draw(c, gfx, x, y, scale, scale);
                                x += font.GetWidthInfo(c).char_width * scale;
                            }
                        }

                        // Cursor
                        Bitmap cursor = new Bitmap(Resources.top_speaker);
                        RectangleF rectCursor = new RectangleF(381, 227, 13.9f, 10);
                        gfx.DrawImage(cursor, rectCursor);
                    }

                    pages.Add(img);
                }
            }

            return pages;
        }

        public bool ShowSettings(Icon icon) => false;
    }
}
