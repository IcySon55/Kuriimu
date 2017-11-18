using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using game_shin_megami_tensei_iv.Properties;
using Kontract.Interface;
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
            ["<D407>"] = "<unk1>",
            ["<D507>"] = "<unk2>",

            ["<E903>"] = "<unk4>",

            ["<F801>"] = "\n",
            ["<F802>"] = "\n",
            ["<F87A>"] = "<unk3>",
            ["<F812>"] = "<name>",
            ["<F813>"] = "<text>",
            ["<E5E3>"] = "<playername>",
            ["A"] = "<A>",
            ["B"] = "<B>",
            ["C"] = "<C>",
            ["D"] = "<D>",
            ["E"] = "<E>",
            //Spanish localization
            ["\u30F3"] = "ñ", //ン
            ["\u30D9"] = "ä", //ベ
            ["\u30E9"] = "á", //ラ　
            ["\u30DA"] = "ï", //ペ
            ["\u30EA"] = "í", //リ
            ["\u30DB"] = "ü", //ホ
            ["\u30EB"] = "ú", //ル
            ["\u30DC"] = "ë", //ボ
            ["\u30EC"] = "é", //レ
            ["\u30DD"] = "ö", //ポ
            ["\u30ED"] = "ó", //ロ
            ["\u30A1"] = "Ä", //ァ
            ["\u30A2"] = "Á", //ア
            ["\u30A3"] = "Ï", //ィ
            ["\u30A4"] = "Í", //イ
            ["\u30A5"] = "Ü", //ゥ
            ["\u30A6"] = "Ú", //ウ
            ["\u30A7"] = "Ë", //ェ
            ["\u30A8"] = "É", //エ
            ["\u30A9"] = "Ö", //ォ
            ["\u30AA"] = "Ó", //オ
            ["\u30D1"] = "¡", //パ
            ["\u30D7"] = "¿", //プ
            //End of spanish localization

            ["\u0000"] = "<!>",
            ["<7000>"] = "<7000>",
            ["<1400>"] = "<1400>"
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
                    var info = font.GetWidthInfo(kuriimuString[i]);
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
                        if (tag == "<text>") // End Name
                        {
                            font.SetColor(Color.FromArgb(255, 80, 0, 0)); // Dark Red
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
                    else if (kuriimuString[i] == '　')
                    {
                        x += 7;
                    }
                    else
                    {
                        font.Draw(kuriimuString[i], gfx, x, y, scaleCurrent, scaleCurrent);
                    }
                    x += info.char_width;
                }
            }

            pages.Add(img);

            return pages;
        }

        public bool ShowSettings(Icon icon) => false;
    }
}