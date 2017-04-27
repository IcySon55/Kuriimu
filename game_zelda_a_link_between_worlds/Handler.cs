using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Cetera.Font;
using game_zelda_a_link_between_worlds.Properties;
using Kuriimu.Compression;
using Kuriimu.Contract;

namespace game_zelda_a_link_between_worlds
{
    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "A Link Between Worlds";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => true;
        public bool HandlerCanGeneratePreviews => true;

        #endregion

        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            // Commands
            ["<sel>"] = "\xE\x1\x6\x2",
            ["<unkcmd1>"] = "\xE\x1\x10\x0",
            ["<shake>"] = "\xE\x1\xF\x0",
            ["<pause1>"] = "\xE\x1\x7\x2",
            ["<pause2>"] = "\xE\x1\x8\x2",
            ["<size>"] = "\xE\x0\x2\x2",
            ["<instant>"] = "\xE\x1\xE\x0",
            ["<padding>"] = "\xE\x1\x11\x4",

            // Color
            ["<c-blue>"] = "\xE\x0\x3\x2\x9\x0",
            ["<c-red>"] = "\xE\x0\x3\x2\xA\x0",
            ["<c-cyan>"] = "\xE\x0\x3\x2\xB\x0",
            ["<c-default>"] = "\xE\x0\x3\x2\xFF\xFF",

            // Variables
            ["<player>"] = "\xE\x1\x0\x0",
            ["<unkvar1>"] = "\xE\x1\x2",
            ["<unkvar2>"] = "\xE\x1\x3",
            ["<unkvar3>"] = "\xE\x1\x4",
            ["<unkvar4>"] = "\xE\x1\xA",
            ["<npc>"] = "\xE\x2\x0\x2",
            ["<map>"] = "\xE\x2\x1\x4",
            ["<item>"] = "\xE\x2\x2\x4",
            ["<value>"] = "\xE\x1\x5\x6",

            // Special
            ["Ⓐ"] = "\xE000",
            ["Ⓑ"] = "\xE001",
            ["Ⓧ"] = "\xE002",
            ["Ⓨ"] = "\xE003",
            ["🄻"] = "\xE004",
            ["🅁"] = "\xE005",
            ["✚"] = "\xE006",
            ["◀"] = "\xE036",
            ["▶"] = "\xE037",
            [":ravio:"] = "\xE05E",
            [":bow:"] = "\xE06C",
            [":bomb:"] = "\xE06D",
            [":rod:"] = "\xE06E",
            ["←"] = "\x2190",
            ["↑"] = "\x2191",
            ["→"] = "\x2192",
            ["↓"] = "\x2193"
        };

        static Lazy<BCFNT> fontInitializer = new Lazy<BCFNT>(() => new BCFNT(new MemoryStream(GZip.Decompress(Resources.MainFont_bcfnt))));
        BCFNT font => fontInitializer.Value;
        string[] it_name = Resources.item.Split('\n');
        string[] loc_name = Resources.location.Split('\n');
        string[] npc_name = Resources.npc.Split('\n');

        public string GetKuriimuString(string rawString)
        {
            return _pairs.Aggregate(rawString, (str, pair) => str.Replace(pair.Value, pair.Key));
        }

        public string GetRawString(string kuriimuString)
        {
            return _pairs.Aggregate(kuriimuString, (str, pair) => str.Replace(pair.Key, pair.Value));
        }

        Bitmap background = new Bitmap(Resources.background);
        Bitmap textBox = new Bitmap(Resources.textbox);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry == null) return pages;

            string rawString = GetKuriimuString(entry.EditedText);
            var pagestr = new List<string>();
            int curline = 0;
            string curstr = "";

            foreach (string line in rawString.Split('\n'))
            {
                curstr += line + '\n';
                curline++;
                if (curline % 3 == 0)
                {
                    pagestr.Add(curstr);
                    curstr = "";
                }
            }
            if (curline % 3 != 0)
                pagestr.Add(curstr);

            foreach (string page in pagestr)
            {
                string p = GetRawString(page).Replace("\xE\x1\x0\x0", Properties.Settings.Default.PlayerName == string.Empty ? "Player" : Properties.Settings.Default.PlayerName);

                if (p.Trim() != string.Empty)
                {
                    Bitmap img = new Bitmap(400, 240);

                    using (Graphics gfx = Graphics.FromImage(img))
                    {
                        gfx.SmoothingMode = SmoothingMode.HighQuality;
                        gfx.InterpolationMode = InterpolationMode.Bicubic;
                        gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        gfx.DrawImage(background, 0, 0);

                        gfx.DrawImage(textBox, 0, 240 - textBox.Height);

                        // Text
                        Rectangle rectText = new Rectangle(32, 20, 336, 44);

                        float scale = 1.0f;
                        float x = rectText.X, y = 240 - textBox.Height + 20;
                        bool cmd = false;
                        string param = "", temp = "";
                        int skip = 0, j = 0, padding = 0;

                        font.SetColor(Color.FromArgb(255, 80, 80, 80));

                        gfx.InterpolationMode = InterpolationMode.Bicubic;
                        foreach (char c in p)
                        {
                            switch (param)
                            {
                                case "\x0\x3\x2\x9\x0":
                                    font.SetColor(Color.Blue);
                                    goto case "cleanup";
                                case "\x0\x3\x2\xA\x0":
                                    font.SetColor(Color.Red);
                                    goto case "cleanup";
                                case "\x0\x3\x2\xB\x0":
                                    font.SetColor(Color.Cyan);
                                    goto case "cleanup";
                                case "\x0\x3\x2\xFF\xFF":
                                    font.SetColor(Color.FromArgb(255, 80, 80, 80));
                                    goto case "cleanup";
                                case "\x1\x2":
                                case "\x1\x3":
                                case "\x1\x4":
                                case "\x1\xA":
                                    temp = "UNKNOWN";
                                    font.SetColor(Color.Blue);
                                    skip = 1;
                                    goto case "placeholder";
                                case "\x0\x2\x2": // font size
                                    scale = (float)c / 100.0f;
                                    skip = 2;
                                    goto case "cleanup";
                                case "\x1\x8\x2": // useless
                                case "\x1\x7\x2":
                                case "\x1\xE":
                                case "\x1\xF":
                                case "\x1\x10":
                                    skip = 2;
                                    goto case "cleanup";
                                case "\x1\x11\x4": // text padding
                                    x += c;
                                    padding = p[j + 2] / 2 - 4;
                                    y += padding;
                                    skip = 4;
                                    goto case "cleanup";
                                case "\x1\x5\x6": // value
                                    temp = "00";
                                    skip = 6;
                                    goto case "placeholder";
                                case "\x2\x0\x2": // npc
                                    temp = npc_name[c];
                                    font.SetColor(Color.Blue);
                                    skip = 2;
                                    goto case "placeholder";
                                case "\x2\x1\x4": // map
                                    temp = loc_name[c];
                                    font.SetColor(Color.Blue);
                                    skip = 4;
                                    goto case "placeholder";
                                case "\x2\x2\x4": // item
                                    temp = it_name[c];
                                    font.SetColor(Color.Blue);
                                    skip = 4;
                                    goto case "placeholder";
                                case "placeholder":
                                    foreach (char t in temp)
                                    {
                                        font.Draw(t, gfx, x, y, scale, scale);
                                        x += font.GetWidthInfo(t).char_width * scale;
                                    }
                                    font.SetColor(Color.FromArgb(255, 80, 80, 80));
                                    goto case "cleanup";
                                case "cleanup":
                                    param = "";
                                    cmd = false;
                                    break;
                            }

                            j++;

                            if (!cmd) switch (c)
                                {
                                    case '\n':
                                        x = rectText.X;
                                        y += rectText.Y;
                                        continue;
                                    case '\xE':
                                        cmd = true;
                                        param = "";
                                        continue;
                                }

                            if (cmd)
                                param += c;
                            else if (skip > 0)
                                skip--;
                            else
                            {
                                font.Draw(c, gfx, x, y, scale, scale);
                                x += font.GetWidthInfo(c).char_width * scale;
                            }
                        }
                    }

                    pages.Add(img);
                }
            }

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
