using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Cetera.Font;
using game_etrian_odyssey_v.Properties;
using Kuriimu.Contract;

namespace game_etrian_odyssey_v
{
    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "Etrian Odyssey V";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => true;
        public bool HandlerCanGeneratePreviews => true;

        #endregion

        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {

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

        Bitmap textBox = new Bitmap(Resources.blank_top);

        // Previewer
        public IList<Bitmap> GeneratePreviews(IEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry == null) return pages;

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

                for (int i = 0; i < kuriimuString.Length; i++)
                {
                    /*if (kuriimuString[i] == ' ')
                    {
                        x += 5;
                    }
                    else if (kuriimuString[i] == '\xa')
                    {
                        x = rectText.X;
                        y += 15;
                    }
                    else
                    {*/
                        var info = font.GetWidthInfo(kuriimuString[i]);
                        x += info.left;
                        font.Draw(kuriimuString[i], gfx, x, y, scaleDefault, scaleDefault);
                        x += info.glyph_width - info.left;
                    //}
                }
            }

            pages.Add(img);

            return pages;
        }

        public bool ShowSettings(Icon icon) => false;
    }
}