using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using game_yokai_watch.Properties;
using Kuriimu.Kontract;

namespace game_yokai_watch
{
    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "Yokai Watch";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => false;
        public bool HandlerCanGeneratePreviews => true;

        #endregion
        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            ["\x5b"] = "[",
            ["\x5d"] = "]",
            ["\x2f"] = "/",
            ["\x5c\x6e"] = "\r\n",
            ["<PNAME>"] = "Nate"
        };

        static Lazy<XF> fontInitializer = new Lazy<XF>(() => new XF(new MemoryStream(Resources.ft_nrm)));
        static XF font => fontInitializer.Value;

        public string GetKuriimuString(string rawString)
        {
            return rawString;
        }

        public string GetKuriimuStringReplaced(string rawString)
        {
            return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
        }

        public string GetRawString(string kuriimuString)
        {
            return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        Bitmap textBox = new Bitmap(Resources.blank_top);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry?.EditedText == null) return pages;

            string kuriimuString = GetKuriimuStringReplaced(entry.EditedText);

            Bitmap img = new Bitmap(textBox.Width, textBox.Height);

            using (Graphics gfx = Graphics.FromImage(img))
            {
                gfx.DrawImage(textBox, 0, 0);

                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.Bicubic;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                RectangleF rectText = new RectangleF(40, 43, 370, 100);

                Color colorDefault = Color.FromArgb(255, 0, 0, 0);

                float scaleDefault = 1.0f;
                float scaleCurrent = scaleDefault;
                float x = rectText.X, pX = x;
                float y = rectText.Y, pY = y;
                for (int i = 0; i < kuriimuString.Length; i++)
                {
                    if (kuriimuString[i] == '<')
                    {
                        var tag = "<";
                        while (tag.Last() != '>')
                        {
                            tag += kuriimuString[++i];
                            if (i >= kuriimuString.Length) break;
                        }
                    }
                    else if (kuriimuString[i] == '\n')
                    {
                        y += 26;
                        x = 40;
                        continue;
                    }
                    else if (kuriimuString[i] == '\r')
                    {
                        continue;
                    }
                    else
                    {
                        XF.CharacterMap charMap = font.GetCharacterMap(kuriimuString[i], false);

                        font.DrawCharacter(kuriimuString[i], colorDefault, gfx, x, y, false);
                        x += charMap.CharWidth;
                    }
                }
            }

            pages.Add(img);

            return pages;
        }

        public bool ShowSettings(Icon icon) => false;
    }
}