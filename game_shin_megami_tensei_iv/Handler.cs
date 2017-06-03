using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using game_shin_megami_tensei_iv.Properties;
using Kuriimu.Contract;
using Cetera.Font;
using System;
using System.Diagnostics;

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
        public bool HandlerCanGeneratePreviews => true;

        #endregion
        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            // Control
            ["\uD407"] = "<unk1>",
            ["\uD507"] = "<unk2>",

            ["\uE903"] = "<unk4>",

            ["\uF801"] = "\n",
            ["\uF812"] = "<name>",
            ["\uF813"] = "</name>",
            ["\uF87A"] = "<unk3>",

            ["\u0000"] = "<0000>",
            ["\u7000"] = "<7000>",
            ["\u1400"] = "<1400>"
        };

        static Lazy<BCFNT> fontInitializer = new Lazy<BCFNT>(() => new BCFNT(new MemoryStream(Resources.MainFont)));
        BCFNT font => fontInitializer.Value;

        public string GetKuriimuString(string rawString)
        {
            return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
        }

        public string GetRawString(string kuriimuString)
        {
            return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        Bitmap background = new Bitmap(Resources.background);
        Bitmap nameBox = new Bitmap(Resources.namebox);
        Bitmap textBox = new Bitmap(Resources.textbox);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry?.EditedText == null) return pages;

            string kuriimuString = GetKuriimuString(entry.EditedText);

            Bitmap img = new Bitmap(background.Width, background.Height);

            Encoding sjis = Encoding.GetEncoding("SJIS");

            using (Graphics gfx = Graphics.FromImage(img))
            {
                gfx.DrawImage(background, 0, 0);
                gfx.DrawImage(textBox, 15, 173);
                gfx.DrawImage(nameBox, 50, 165);

                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.Bicubic;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                RectangleF rectText = new RectangleF(20, 183, 370, 125);

                Color colorDefault = Color.FromArgb(255, 0, 0, 0);

                float scaleDefault = 1.0f;
                //float scaleName = 0.86f;
                float scaleCurrent = scaleDefault;
                float x = rectText.X, pX = x;
                float y = rectText.Y, pY = y;
                //float yAdjust = 4;
                //int box = 0;
                for (int i = 0; i < kuriimuString.Length; i++)
                {
                    var info = font.GetWidthInfo(kuriimuString[i].ToString().Normalize(NormalizationForm.FormKD)[0]);
                    x += info.left;
                    x += info.glyph_width - info.left;
                    if (kuriimuString[i] == '<')
                    {
                        var tag = "<";
                        while (tag.Last() != '>')
                        {
                            tag += kuriimuString[++i];
                            if (i >= kuriimuString.Length) break;
                        }

                        if (tag == "<name>") // Start Name
                        {
                            font.SetColor(Color.FromArgb(255, 255, 255, 255)); // White
                            y -= 15;
                            x = rectText.X + 25;
                        }
                        if (tag == "</name>") // End Name
                        {
                            font.SetColor(colorDefault); // Black
                            y += 15;
                            x = 20;
                        }
                    }
                    else if (kuriimuString[i] == '\n')
                    {
                        x = rectText.X;
                        y += 17;
                    }
                    else if (kuriimuString[i] == ' ')
                    {
                        x += 7;
                    }
                    else
                    {
                        font.Draw(kuriimuString[i].ToString().Normalize(NormalizationForm.FormKD)[0], gfx, x, y, scaleCurrent, scaleCurrent);
                    }
                }
            }

            pages.Add(img);

            return pages;
        }

        public bool ShowSettings(Icon icon) => false;
    }
}