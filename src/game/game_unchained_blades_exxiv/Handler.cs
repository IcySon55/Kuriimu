using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Cetera.Font;
using game_unchained_blades_exxiv.Properties;
using Kontract.Interface;

namespace game_unchained_blades_exxiv
{
    public enum BubbleType
    {
        Type1,
        Type2
    }

    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "Unchained Blades EXXiV";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => true;
        public bool HandlerCanGeneratePreviews => true;

        #endregion

        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {

        };

        Dictionary<string, string> _previewPairs = new Dictionary<string, string>
        {
            ["<1>"] = "Player",
        };

        BCFNT font;

        public Handler()
        {
            var ms = new MemoryStream();
            new MemoryStream(Resources.MainFont).CopyTo(ms);
            ms.Position = 0;
            font = new BCFNT(ms);
        }

        public string GetKuriimuString(string rawString)
        {
            return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
        }

        public string GetRawString(string kuriimuString)
        {
            return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        Bitmap bubble001 = new Bitmap(Resources.Bubble001);
        Bitmap bubble002 = new Bitmap(Resources.Bubble002);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry?.EditedText == null) return pages;

            //string kuriimuString = _previewPairs.Aggregate(GetKuriimuString(entry.EditedText), (s, pair) => s.Replace(pair.Key, pair.Value));
            string kuriimuString = _previewPairs.Aggregate(GetKuriimuString(entry.EditedText), (s, pair) => s.Replace(pair.Key, pair.Value));

            Bitmap img = new Bitmap(1, 1);

            switch (Enum.Parse(typeof(BubbleType), Properties.Settings.Default.BubbleType))
            {
                case BubbleType.Type1:
                    img = new Bitmap(bubble001.Width, bubble001.Height);
                    break;

                case BubbleType.Type2:
                    img = new Bitmap(bubble002.Width, bubble002.Height);
                    break;
            }

            using (Graphics gfx = Graphics.FromImage(img))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.Bicubic;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                RectangleF rectText = new RectangleF(0, 0, 0, 0);
                Color colorDefault = Color.FromArgb(255, 0, 0, 0);

                switch (Enum.Parse(typeof(BubbleType), Properties.Settings.Default.BubbleType))
                {
                    case BubbleType.Type1:
                        gfx.DrawImage(bubble001, 0, 0);
                        rectText = new RectangleF(12, 9, 370, 100);
                        break;

                    case BubbleType.Type2:
                        gfx.DrawImage(bubble002, 0, 0);
                        rectText = new RectangleF(20, 11, 370, 100);
                        break;
                }

                float scaleDefault = 1.0f;
                float scaleCurrent = scaleDefault;
                float x = rectText.X, pX = x;
                float y = rectText.Y, pY = y;
                //float fontSize = 3.2f;
                float fontSize = 1.5f;

                for (int i = 0; i < kuriimuString.Length; i++)
                {
                    BCFNT.CharWidthInfo widthInfo = font.GetWidthInfo(kuriimuString[i]);
                    font.SetColor(colorDefault);

                    if (kuriimuString[i] == '\n')
                    {
                        switch (Enum.Parse(typeof(BubbleType), Properties.Settings.Default.BubbleType))
                        {
                            case BubbleType.Type1:
                                y += 15;
                                x = 12;
                                continue;

                            case BubbleType.Type2:
                                y += 13;
                                x = 18;
                                continue;
                        }
                    }

                    font.Draw(kuriimuString[i], gfx, x, y, scaleCurrent, scaleCurrent);
                    x += widthInfo.char_width - fontSize;
                }
            }

            pages.Add(img);

            return pages;
        }

        public bool ShowSettings(Icon icon)
        {
            var settings = new Settings(icon);
            settings.ShowDialog();
            return settings.HasChanges;
        }
    }
}