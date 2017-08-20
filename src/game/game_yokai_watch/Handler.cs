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
    public enum Scenes
    {
        Dialog,
        CutsceneBottom
    }

    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "Yokai Watch";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => true;
        public bool HandlerCanGeneratePreviews => true;

        #endregion
        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            ["\x5b"] = "[",
            ["\x5d"] = "]",
            ["\x2f"] = "/",
            ["\x5c\x6e"] = "\r\n",
        };

        // Old Preview Dictionary
        //Dictionary<string, string> _previewPairs = new Dictionary<string, string>
        //{
        //["<PNAME>"] = Properties.Settings.Default.PlayerName
        //};

        static Lazy<XF> fontInitializer = new Lazy<XF>(() => new XF(new MemoryStream(Resources.ft_nrm)));
        static XF font => fontInitializer.Value;

        public string GetKuriimuString(string rawString)
        {
            return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value));
        }

        public string GetRawString(string kuriimuString)
        {
            return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        Bitmap textBox = new Bitmap(Resources.blank_top);
        Bitmap csBTM = new Bitmap(Resources.cutscene_bottom);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry?.EditedText == null) return pages;

            //string kuriimuString = _previewPairs.Aggregate(GetKuriimuString(entry.EditedText), (s, pair) => s.Replace(pair.Key, pair.Value));

            // Replaces the need for Preview Dictionary for the time being.
            string kuriimuString = GetKuriimuString(entry.EditedText).Replace("<PNAME>", Properties.Settings.Default.PlayerName == string.Empty ? "Nate" : Properties.Settings.Default.PlayerName);

            //Bitmap img = new Bitmap(textBox.Width, textBox.Height);
            Bitmap img = new Bitmap(1, 1);

            switch (Enum.Parse(typeof(Scenes), Properties.Settings.Default.Scene))
            {
                case Scenes.Dialog:
                    img = new Bitmap(textBox.Width, textBox.Height);
                    break;

                case Scenes.CutsceneBottom:
                    img = new Bitmap(csBTM.Width, csBTM.Height);
                    break;
            }

            using (Graphics gfx = Graphics.FromImage(img))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.Bicubic;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                //RectangleF rectText = new RectangleF(40, 43, 370, 100);
                RectangleF rectText = new RectangleF(0, 0, 0, 0);
                Color colorDefault = Color.FromArgb(255, 0, 0, 0);

                switch (Enum.Parse(typeof(Scenes), Properties.Settings.Default.Scene))
                {
                    case Scenes.Dialog:
                        gfx.DrawImage(textBox, 0, 0);
                        rectText = new RectangleF(40, 43, 370, 100);
                        colorDefault = Color.FromArgb(255, 0, 0, 0); // Black
                        break;

                    case Scenes.CutsceneBottom:
                        gfx.DrawImage(csBTM, 0, 0);
                        rectText = new RectangleF(25, 85, 370, 100);
                        colorDefault = Color.FromArgb(255, 255, 255, 255); // White
                        break;
                }

                float scaleDefault = 1.0f;
                float scaleCurrent = scaleDefault;
                float x = rectText.X, pX = x;
                float y = rectText.Y, pY = y;
                float fontSize = 0.85f;

                bool furigana = false;
                bool furiganaUpper = false;
                float furiganaLowerWidth = 0;
                float furiganaUpperWidth = 0;
                float lowerOffset = 0;
                float upperOffset = 0;
                float bkX = 0;

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
                    else if (kuriimuString[i].ToString() == "[")
                    {
                        bkX = x;
                        furigana = true;

                        //getting char length from lower and upper furigana string in px
                        furiganaLowerWidth = 0;
                        furiganaUpperWidth = 0;
                        int tmp = i + 1;
                        bool fin = false;
                        bool furiganaTmp = false;
                        bool draw = false;
                        while (fin == false)
                        {
                            if (tmp >= kuriimuString.Length)
                            {
                                if (fin == false)
                                {
                                    draw = true;
                                }
                                break;
                            }

                            if (kuriimuString[tmp].ToString() == "/")
                            {
                                furiganaTmp = true;
                                tmp++;
                            }
                            else if (kuriimuString[tmp].ToString() == "]")
                            {
                                fin = true;
                                if (furiganaTmp == false)
                                {
                                    draw = true;
                                }
                            }
                            else
                            {
                                XF.CharacterMap charMaptmp = font.GetCharacterMap(kuriimuString[tmp++], furiganaTmp);
                                var t1 = (furiganaTmp) ? furiganaUpperWidth += charMaptmp.CharWidth : furiganaLowerWidth += charMaptmp.CharWidth;
                            }
                        }

                        //setting furigana offset
                        var t2 = (furiganaLowerWidth > furiganaUpperWidth) ? upperOffset = (furiganaLowerWidth - furiganaUpperWidth) / 2 : lowerOffset = (furiganaUpperWidth - furiganaLowerWidth) / 2;
                        if (furiganaLowerWidth < furiganaUpperWidth)
                        {
                            x += lowerOffset;
                        }

                        if (draw == false)
                        {
                            continue;
                        }
                    }
                    else if (kuriimuString[i].ToString() == "/")
                    {
                        if (furigana)
                        {
                            furiganaUpper = true;
                            y -= 7;
                            var t3 = (furiganaUpperWidth < furiganaLowerWidth) ? x = bkX + upperOffset : x = bkX;
                            continue;
                        }
                    }
                    else if (kuriimuString[i].ToString() == "]")
                    {
                        if (furiganaUpper)
                        {
                            var t4 = (furiganaUpperWidth > furiganaLowerWidth) ? x = bkX + furiganaUpperWidth : x = bkX + furiganaLowerWidth;
                            y += 7;
                            furiganaUpper = false;
                            furigana = false;
                            upperOffset = 0;
                            lowerOffset = 0;
                            continue;
                        }
                    }

                    XF.CharacterMap charMap = font.GetCharacterMap(kuriimuString[i], furiganaUpper);

                    //draw regular character
                    font.DrawCharacter(kuriimuString[i], colorDefault, gfx, x, y, furiganaUpper);
                    x += charMap.CharWidth-fontSize;
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