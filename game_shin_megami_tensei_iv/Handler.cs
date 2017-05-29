using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using game_shin_megami_tensei_iv.Properties;
using Kuriimu.Contract;

namespace game_shin_megami_tensei_iv
{
    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "Shin Megami Tensei IV";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => true;
        public bool HandlerCanGeneratePreviews => false;

        #endregion

        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            // Control
            ["\uE61A"] = "<unk1>",
            ["\x14\u0000"] = "<unk2>",

            ["\x0\x0"] = "<!>",

            ["\u30FB"] = "\r\n"

            // Color

            // Special
            //["\x2026"] = "…",
            //["\x2020"] = "©",
            //["\x2021"] = "♥"
        };

        //static Lazy<BCFNT> fontInitializer = new Lazy<BCFNT>(() => new BCFNT(new MemoryStream(GZip.Decompress(new MemoryStream(Resources.MainFont_bcfnt)))));
        //BCFNT font => fontInitializer.Value;

        public string GetKuriimuString(string rawString)
        {
            return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
        }

        public string GetRawString(string kuriimuString)
        {
            return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        //Bitmap background = new Bitmap(Resources.background);
        //Bitmap nameBox = new Bitmap(Resources.namebox_top);
        //Bitmap textBox = new Bitmap(Resources.textbox_top);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry) => null;
        //{
        //    var pages = new List<Bitmap>();
        //    if (entry == null) return pages;

        //    foreach (string page in entry.EditedText.Split('\x17'))
        //    {
        //        if (page.Trim() != string.Empty)
        //        {
        //            Bitmap img = new Bitmap(background.Width, background.Height);

        //            using (Graphics gfx = Graphics.FromImage(img))
        //            {
        //                gfx.SmoothingMode = SmoothingMode.HighQuality;
        //                gfx.InterpolationMode = InterpolationMode.Bilinear;
        //                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

        //                gfx.DrawImage(background, 0, 0);

        //                gfx.DrawImage(textBox, 0, 0);
        //                gfx.DrawImage(nameBox, 0, 0);

        //                RectangleF rectName = new RectangleF(16, 2, 128, 17);
        //                RectangleF rectText = new RectangleF(30, 18, 340, 37);

        //                Color colorDefault = Color.FromArgb(255, 37, 66, 167);

        //                float scaleDefaultX = 1.0f;
        //                float scaleDefaultY = 1.05f;
        //                float scaleName = 0.8f;
        //                float scaleCurrentX = scaleDefaultX;
        //                float scaleCurrentY = scaleDefaultY;
        //                float x = rectText.X, pX = x;
        //                float y = rectText.Y, pY = y;
        //                float lineSpacing = 4;

        //                string str = page.Replace("\x1F\x05", Properties.Settings.Default.PlayerName == string.Empty ? "Player" : Properties.Settings.Default.PlayerName);
        //                font.SetColor(colorDefault);

        //                for (int i = 0; i < str.Length; i++)
        //                {
        //                    bool notEOS = i + 2 < str.Length;
        //                    char c = str[i];
        //                    char c2 = ' ';
        //                    if (notEOS)
        //                        c2 = str[i + 1];

        //                    BCFNT.CharWidthInfo widthInfo = font.GetWidthInfo(c);

        //                    // Handle control codes
        //                    if (c == 0x001F && c2 == 0x0002) // Start Name
        //                    {
        //                        font.SetColor(Color.White);
        //                        scaleCurrentX = scaleName;
        //                        scaleCurrentY = scaleName;
        //                        float width = font.MeasureString(str.Substring(i + 2), (char)0x0002, scaleCurrentX);
        //                        pX = x;
        //                        pY = y;
        //                        x = rectName.X + (rectName.Width / 2) - (width / 2);
        //                        y = rectName.Y;
        //                        i++;
        //                        continue;
        //                    }
        //                    else if (c == 0x001F && (c2 == 0x0000 || c2 == 0x0100 || c2 == 0x0200 || c2 == 0x0103 || c2 == 0x0020 || c2 == 0x0115)) // Unknown/No Render Effect
        //                    {
        //                        i++;
        //                        continue;
        //                    }
        //                    else if (c == 0x0013 && c2 == 0x0000) // Default
        //                    {
        //                        font.SetColor(colorDefault);
        //                        i++;
        //                        continue;
        //                    }
        //                    else if (c == 0x0013 && c2 == 0x0001) // Red
        //                    {
        //                        font.SetColor(Color.Red);
        //                        i++;
        //                        continue;
        //                    }
        //                    else if (c == 0x0013 && c2 == 0x0003) // Light Blue
        //                    {
        //                        font.SetColor(Color.FromArgb(255, 54, 129, 216));
        //                        i++;
        //                        continue;
        //                    }
        //                    else if (c == 0x001F && c2 == 0x0015) // End Dialog
        //                    {
        //                        i++;
        //                        continue;
        //                    }
        //                    else if (c == 0x0017) // Next TextBox
        //                    {
        //                        //x = rectText.X;
        //                        //pX = x;
        //                        //y = rectText.Y + textBox.Height;
        //                        //pY += y;
        //                        continue;
        //                    }
        //                    else if (c == 0x0002) // End Name
        //                    {
        //                        font.SetColor(colorDefault);
        //                        scaleCurrentX = scaleDefaultX;
        //                        scaleCurrentY = scaleDefaultY;
        //                        x = pX;
        //                        y = pY;
        //                        continue;
        //                    }
        //                    else if (c == '\n' || x + (widthInfo.char_width * scaleCurrentX) - rectText.X > rectText.Width) // New Line/End of Textbox
        //                    {
        //                        x = rectText.X;
        //                        y += (16 * scaleCurrentX) + lineSpacing;
        //                        if (c == '\n')
        //                            continue;
        //                    }

        //                    // Otherwise it's a regular drawable character
        //                    font.Draw(c, gfx, x, y, scaleCurrentX, scaleCurrentY);
        //                    x += widthInfo.char_width * scaleCurrentX;
        //                }
        //            }

        //            pages.Add(img);
        //        }
        //    }

        //    return pages;
        //}

        public bool ShowSettings(Icon icon) => false;
    }
}