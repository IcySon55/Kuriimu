using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cetera.Font;
using game_maple_story_3ds.Properties;
using Kuriimu.Contract;

namespace game_maple_story_3ds
{
    public class Handler : IGameHandler
    {
        #region Properties

        // Information
        public string Name => "MapleStory Girl of Destiny";
        public Image Icon => Resources.icon;

        // Feature Support
        public bool HandlerHasSettings => true;
        public bool HandlerCanGeneratePreviews => true;

        #endregion

        Dictionary<string, string> _pairs = new Dictionary<string, string>
        {
            // Control
            ["\\["] = "[",
            ["\\]"] = "]",
            ["[NAME:B]"] = "<PlayerName>",
        };

        Dictionary<string, string> _previewPairs = new Dictionary<string, string>
        {
            // Control
            ["\\["] = "[",
            ["\\]"] = "]",
            ["[NAME:B]"] = "‹PlayerName›",
        };

        BCFNT font;

        public Handler()
        {
            var ms = new MemoryStream();
            new GZipStream(new MemoryStream(Resources.MainFont_bcfnt), CompressionMode.Decompress).CopyTo(ms);
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

        Bitmap background = new Bitmap(Resources.background);
        Bitmap textBox = new Bitmap(Resources.blank);

        // Previewer
        public IList<Bitmap> GeneratePreviews(TextEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry == null) return pages;

            // Paging evety 3rd new line
            List<string> strings = new List<string>();
            string[] lines = entry.EditedText.Split('\n');
            string built = string.Empty;
            int current = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                built += lines[i] + (i != lines.Length - 1 ? "\n" : string.Empty);
                current++;

                if (current % 3 == 0)
                {
                    strings.Add(built);
                    built = string.Empty;
                }
            }
            if ((strings.Count == 0 && built != string.Empty) || (strings.Count > 0 && strings.Last().EndsWith("\n")))
                strings.Add(built);

            foreach (string page in strings)
            {
                const int txtOffsetX = 2;
                const int txtOffsetY = 2;
                Bitmap img = new Bitmap(background.Width, background.Height);

                using (Graphics gfx = Graphics.FromImage(img))
                {
                    gfx.SmoothingMode = SmoothingMode.HighQuality;
                    gfx.InterpolationMode = InterpolationMode.Bicubic;
                    gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    gfx.DrawImage(background, 0, 0);

                    gfx.DrawImage(textBox, txtOffsetX, txtOffsetY);

                    float scale = 0.625f;
                    float x = 10, y = 22;
                    string str = _previewPairs.Aggregate(page, (s, pair) => s.Replace(pair.Key, pair.Value)).Replace("‹PlayerName›", "‹" + Properties.Settings.Default.PlayerName + "›");
                    font.SetColor(Color.White);

                    foreach (char c in str)
                    {
                        switch (c)
                        {
                            case '‹': // Player Name
                                font.SetColor(Color.FromArgb(255, 254, 254, 149));
                                continue;
                            case '<': // Orange
                                font.SetColor(Color.FromArgb(255, 255, 150, 0));
                                continue;
                            case '{':  // Green
                                font.SetColor(Color.FromArgb(255, 131, 237, 63));
                                continue;
                            case '[': // Purple
                                font.SetColor(Color.FromArgb(255, 255, 100, 255));
                                continue;
                            case '›': // Reset color
                            case '>':
                            case '}':
                            case ']':
                                font.SetColor(Color.White);
                                continue;
                            case '\n':
                                x = 10;
                                y += 16;
                                continue;
                        }

                        // Otherwise it's a regular drawable character
                        font.Draw(c, gfx, x, y, scale, scale);
                        x += font.GetWidthInfo(c).char_width * scale;
                    }

                }

                pages.Add(img);
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