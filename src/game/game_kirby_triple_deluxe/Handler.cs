using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Cetera.Font;
using game_kirby_triple_deluxe.Properties;
using Kontract.Compression;
using Kontract.Interface;

namespace game_kirby_triple_deluxe
{
    public enum Scenes
    {
        TopScreen,
        BottomScreen
    }

    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "Kirby Triple Deluxe";
        public Image Icon => new Bitmap(10, 10);//Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => true;
        public bool HandlerCanGeneratePreviews => true;

        #endregion

        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            // Control Codes
            ["\x0e\x00\x02\x02\x3b\x00"] = "<Code ;>",
            ["\x0e\x00\x02\x02\x3c\x00"] = "<Code °>",
            ["\x0e\x00\x02\x02\x3e\x00"] = "<Code °°>",
            ["\x0e\x00\x02\x02\x40\x00"] = "<Code @>",
            ["\x0e\x00\x02\x02\x42\x00"] = "<Code B>",
            ["\x0e\x00\x02\x02\x44\x00"] = "<Code D>",
            ["\x0e\x00\x02\x02\x46\x00"] = "<Code F>",
            ["\x0e\x00\x02\x02\x47\x00"] = "<Code G>",
            ["\x0e\x00\x02\x02\x48\x00"] = "<Code H>",
            ["\x0e\x00\x02\x02\x49\x00"] = "<Code I>",
            ["\x0e\x00\x02\x02\x4a\x00"] = "<Code J>",
            ["\x0e\x00\x02\x02\x4b\x00"] = "<Code K>",
            ["\x0e\x00\x02\x02\x4e\x00"] = "<Code N>",
            ["\x0e\x00\x02\x02\x4f\x00"] = "<Code O>",
            ["\x0e\x00\x02\x02\x50\x00"] = "<Code P>",
            ["\x0e\x00\x02\x02\x51\x00"] = "<Code Q>",
            ["\x0e\x00\x02\x02\x52\x00"] = "<Code R>",
            ["\x0e\x00\x02\x02\x53\x00"] = "<Code S>",
            ["\x0e\x00\x02\x02\x54\x00"] = "<Code T>",
            ["\x0e\x00\x02\x02\x55\x00"] = "<Code U>",
            ["\x0e\x00\x02\x02\x56\x00"] = "<Code V>",
            ["\x0e\x00\x02\x02\x57\x00"] = "<Code W>",
            ["\x0e\x00\x02\x02\x58\x00"] = "<Code X>",
            ["\x0e\x00\x02\x02\x59\x00"] = "<Code Y>",
            ["\x0e\x00\x02\x02\x5a\x00"] = "<Code Z>",
            ["\x0e\x00\x02\x02\x5b\x00"] = "<Code [>",
            ["\x0e\x00\x02\x02\x5c\x00"] = "<Code \\>",
            ["\x0e\x00\x02\x02\x5d\x00"] = "<Code ^>",
            ["\x0e\x00\x02\x02\x5e\x00"] = "<Code ]>",
            ["\x0e\x00\x02\x02\x5f\x00"] = "<Code _>",
            ["\x0e\x00\x02\x02\x60\x00"] = "<Code `>",
            ["\x0e\x00\x02\x02\x61\x00"] = "<Code a>",
            ["\x0e\x00\x02\x02\x62\x00"] = "<Code b>",
            ["\x0e\x00\x02\x02\x63\x00"] = "<Code c>",
            ["\x0e\x00\x02\x02\x64\x00"] = "<Code d>",
            ["\x0e\x00\x02\x02\x68\x00"] = "<Code h>",

            //?
            ["\x0e\x02\x00\x00"] = "<?0>",
            ["\x0e\x02\x01\x00"] = "<?1>",
            ["\x0e\x02\x02\x00"] = "<?2>",

            //Colors RGBA
            ["\x0e\x00\x03\x04"] = "<Color>",

            //Icon codes
            ["\x0e\x01\x00\x00"] = "<Icon Jump Button>",
            ["\x0e\x01\x01\x00"] = "<Icon 1>",
            ["\x0e\x01\x02\x00"] = "<Icon 2>",
            ["\x0e\x01\x03\x00"] = "<Icon 3>",
            ["\x0e\x01\x04\x00"] = "<Icon 4>",
            ["\x0e\x01\x05\x00"] = "<Icon 5>",
            ["\x0e\x01\x06\x00"] = "<Icon 6>",
            ["\x0e\x01\x07\x00"] = "<Icon 7>",
            ["\x0e\x01\x08\x00"] = "<Icon 8>",
            ["\x0e\x01\x09\x00"] = "<Icon D-Pad Button>",
            ["\x0e\x01\x0b\x00"] = "<Icon 11>",
            ["\x0e\x01\x0c\x00"] = "<Icon 12>",
            ["\x0e\x01\x0d\x00"] = "<Icon 13>",
            ["\x0e\x01\x0e\x00"] = "<Icon 14>",
            ["\x0e\x01\x0f\x00"] = "<Icon Home Button>",
            ["\x0e\x01\x10\x00"] = "<Icon 16>",
            ["\x0e\x01\x11\x00"] = "<Icon 17>",
            ["\x0e\x01\x12\x00"] = "<Icon 18>",
            ["\x0e\x01\x13\x00"] = "<Icon 19>"
        };

        BCFNT font => BCFNT.StandardFont;
        //static Lazy<BCFNT> fontInitializer = new Lazy<BCFNT>(() => new BCFNT(new MemoryStream(GZip.Decompress(new MemoryStream(Resources.MainFont_bcfnt)))));
        //BCFNT font => fontInitializer.Value;

        public string GetKuriimuString(string rawString)
        {
            var colorParts = _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Key, pair.Value)).Split(new string[] { "<Color>" }, StringSplitOptions.None);

            var finalString = "";
            for (var i = 0; i < colorParts.Length; i++)
                if (i == 0)
                    finalString = colorParts[0];
                else
                {
                    var colorHex = ((byte)colorParts[i][0] << 24) | ((byte)colorParts[i][1] << 16) | ((byte)colorParts[i][2] << 8) | ((byte)colorParts[i][3]);
                    finalString += $"<Color 0x{colorHex:X8}>";
                    var tmp = colorParts[i].ToList();
                    tmp.RemoveRange(0, 4);
                    finalString += tmp.Aggregate("", (output, inp) => output + inp);
                }

            return finalString;
        }

        public string GetRawString(string kuriimuString)
        {
            var colorParts = kuriimuString.Split(new string[] { "<Color 0x" }, StringSplitOptions.None);

            var finalString = "";
            for (int i = 0; i < colorParts.Length; i++)
            {
                if (i == 0)
                    finalString = colorParts[0];
                else
                {
                    if (colorParts[i].Length > 8)
                        if (colorParts[i][8] != '>')
                            finalString += colorParts[i];
                        else
                            try
                            {
                                var colorHex = UInt32.Parse(colorParts[i].Where((c, index) => index < 8).Aggregate("", (output, c) => output + c), System.Globalization.NumberStyles.AllowHexSpecifier);
                                finalString += "<Color>" +
                                    (char)(colorHex >> 24) + (char)(colorHex >> 16) + (char)(colorHex >> 8) + (char)(colorHex) +
                                    colorParts[i].Where((c, index) => index > 8).Aggregate("", (output, c) => output + c);
                            }
                            catch
                            {
                                finalString += "<Color>\x00\x00\x00\xff" + colorParts[i].Where((c, index) => index > 8).Aggregate("", (output, c) => output + c);
                            }
                    else
                        finalString += colorParts[i];
                }
            }

            return _pairs.Aggregate(finalString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        Bitmap background = new Bitmap(Resource.blank_top);
        //Bitmap bottom = new Bitmap(Resources.bottom);
        //Bitmap nameBox = new Bitmap(Resources.namebox_top);
        //Bitmap textBox = new Bitmap(Resources.textbox_top);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry?.EditedText == null) return pages;

            string kuriimuString = GetKuriimuString(entry.EditedText);
            //int boxes = 1;//kuriimuString.Count(c => c == (char)0x17) + 1;

            Bitmap img = new Bitmap(400, 240);

            using (Graphics gfx = Graphics.FromImage(img))
            {
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.InterpolationMode = InterpolationMode.Bicubic;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gfx.DrawImage(background, new Point(0, 0));

                //for (int i = 0; i < boxes; i++)
                //    gfx.DrawImage(textBox, 0, i * textBox.Height);

                RectangleF rectText = new RectangleF(10, 10, 370, 100);

                Color colorDefault = Color.FromArgb(255, 0, 0, 0);

                float scaleDefault = 0.6f;
                //float scaleName = 0.86f;
                float scaleCurrent = scaleDefault;
                float x = rectText.X, pX = x;
                float y = rectText.Y, pY = y;
                //float yAdjust = 4;
                //int box = 0;

                font.SetColor(colorDefault);

                for (int i = 0; i < kuriimuString.Length; i++)
                {
                    switch (kuriimuString[i])
                    {
                        case '<':
                            //Filter for Colortag
                            font.SetColor(GetColorFromTag(kuriimuString, i));

                            while (kuriimuString[i] != '>')
                                if (i + 1 < kuriimuString.Length)
                                    i++;
                                else
                                    break;
                            break;
                        case ' ':
                            x += 10;
                            break;
                        case '\xa':
                            x = rectText.X;
                            y += 20;
                            break;
                        default:
                            var info = font.GetWidthInfo(kuriimuString[i]);
                            x += info.left;
                            font.Draw(kuriimuString[i], gfx, x, y, scaleDefault, scaleDefault);
                            x += info.glyph_width - info.left;
                            break;
                    }
                }

                pages.Add(img);

                return pages;
            }
        }

        static List<char> hexVals = new List<char>
        {
            '1','2','3','4','5','6','7','8','9','0','a','b','c','d','e','f','A','B','C','D','E','F'
        };
        Color GetColorFromTag(string input, int i)
        {
            if (i + 16 < input.Length)
            {
                var tag = input.ToList().Where((c, index) => index >= i && index <= i + 8).Aggregate("", (output, c) => output + c);
                if (tag == "<Color 0x")
                {
                    var color = input.ToList().Where((c, index) => index >= i + 9 && index <= i + 16).ToArray();
                    bool valid = true;
                    foreach (var c in color)
                        if (!hexVals.Contains(c))
                        {
                            valid = false;
                            break;
                        }
                    if (valid)
                        return Color.FromArgb(
                            (Byte.Parse(color[6].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier) << 4) | Byte.Parse(color[7].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier),
                            (Byte.Parse(color[0].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier) << 4) | Byte.Parse(color[1].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier),
                            (Byte.Parse(color[2].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier) << 4) | Byte.Parse(color[3].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier),
                            (Byte.Parse(color[4].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier) << 4) | Byte.Parse(color[5].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier));
                }
            }

            return Color.FromArgb(255, 0, 0, 0);
        }

        public bool ShowSettings(Icon icon)
        {
            return false;
            /*var settings = new Settings(icon);
            settings.ShowDialog();
            return settings.HasChanges;*/
        }
    }
}