using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using Cetera.Compression;
using Cetera.Font;
using Cetera.Text;
using game_daigasso_band_brothers_p.Properties;
using Kuriimu.Contract;

namespace game_daigasso_band_brothers_p
{
    class Handler : IGameHandler
    {
        static Lazy<BCFNT[]> fontInitializer = new Lazy<BCFNT[]>(() => new[] {
                new BCFNT(new MemoryStream(GZip.Decompress(Resources.Basic_bcfnt))),
                new BCFNT(new MemoryStream(GZip.Decompress(Resources.BasicRim_bcfnt))),
                new BCFNT(new MemoryStream(GZip.Decompress(Resources.SisterSymbol_bcfnt))),
            });
        BCFNT fontBasic => fontInitializer.Value[0];
        BCFNT fontBasicRim => fontInitializer.Value[1];
        BCFNT fontCbfStd => BCFNT.StandardFont;
        BCFNT fontSisterSymbol => fontInitializer.Value[2];

        // Information
        public string Name => "Jam With The Band P";
        public Image Icon => Resources.p;

        // Feature Support
        public bool HandlerHasSettings => false;
        public bool HandlerCanGeneratePreviews => true;

        static Dictionary<uint, string> colours = new Dictionary<uint, string>
        {
            [0xFFFFFFFF] = "W",
            [0xFF000000] = "B",
            [0xFF3A41FF] = "R",
            [0xFF1E1E1E] = "G"
        };

        static Dictionary<string, string> symbols = new Dictionary<string, string>
        {
            ["\uE000"] = "Ⓐ",
            ["\uE001"] = "Ⓑ",
            ["\uE002"] = "Ⓧ",
            ["\uE003"] = "Ⓨ",
            ["\uE004"] = "🄻",
            ["\uE005"] = "🅁",
            ["\uE006"] = "✚",
            ["\uE073"] = "🏠",
            ["\uE077"] = "◉",
            ["\uE07B"] = "⭠",
            ["\uE07D"] = "⭥",
            ["\uE07E"] = "⭤",
            ["\uE100"] = "🖉",
            ["\uE101"] = "🔧",
            ["\uE103"] = "⌫",
            ["\uE106"] = "🎼",
            ["\uE107"] = "⭢",
            ["\uE108"] = "🎤",
            ["\uE10B"] = "🔍",
            ["\uE10C"] = "🔎",
            ["\uE10D"] = "📄",
            ["\uE10E"] = "🍅",
            ["\uE10F"] = "⏵",
            ["\uE110"] = "⏸",
            ["\uE112"] = "⏩",
            ["\uE113"] = "⏪",
            ["\uE114"] = "⏮",
            ["\uE116"] = "⏺",
            ["\uE117"] = "🕮",
            ["\uE118"] = "⌧",
            ["\uE119"] = "🎞",
            ["\uE11A"] = "▶",
            ["\uE11B"] = "◀"
        };

        enum ReplaceCode
        {
            Num, Name, Year, Month, MonthZero, Day, DayZero, Hour, HourZero, Minute, MinuteZero, String
        }

        public string GetKuriimuString(string str)
        {
            try
            {
                // [0] System Codes ------------------
                //		0,0 = System.Ruby(Type8 rt)
                //		0,1 = System.Font(Type8 face)
                //		0,2 = System.Size(Type1 percent)
                //		0,3 = System.Color(Type0 r, Type0 g, Type0 b, Type0 a, Type8 name)
                //		0,4 = PageBreak()
                // [1] Cmd Codes ---------------------
                //		1,0 = Cmd.once_stop()
                //		1,1 = Cmd.key()
                //		1,2 = Cmd.event(no)
                //		1,3 = Cmd.wait(frame)
                //		1,4 = Cmd.deprecated_event_ff(Type9<Enum0> Cmd_deprecated_event_ff_id)
                //		1,5 = Cmd.event_ff(Type9<Enum1> Cmd_event_ff_id)
                //		1,6 = Cmd.event_scout(Type9<Enum2> Cmd_event_scout_id)
                //		1,7 = Cmd.voice(Type8 sound_id)
                //		1,8 = Cmd.wait_voice()
                // [2] Style Codes -------------------
                //		2,0 = Style.ScaleX(Type4 scale_x)
                //		2,1 = Style.VSpace(Type4 space_v)
                // [3] Replace Codes -----------------
                //		3,0 = Replace.Num(Type3 id, Type9<Enum3> Replace_Num_char_type, Type3 length)
                //		3,1 = Replace.Name(Type3 id)
                //		3,2 = Replace.Year(Type3 id)
                //		3,3 = Replace.Month(Type3 id)
                //		3,4 = Replace.MonthZero(Type3 id)
                //		3,5 = Replace.Day(Type3 id)
                //		3,6 = Replace.DayZero(Type3 id)
                //		3,7 = Replace.Hour(Type3 id)
                //		3,8 = Replace.HourZero(Type3 id)
                //		3,9 = Replace.Minute(Type3 id)
                //		3,A = Replace.MinuteZero(Type3 id)
                //		3,B = Replace.String(Type3 id, Type9<Enum4> Replace_String_char_type, Type9<Enum5> Replace_String_length_type, x)
                // [Enum values] ---------------------
                //		Enum0[32] = none,SaluteSeq,Profile,Dlg_ProfileOK,MovePaper_Photo,CameraSequence,SwitchBalloon_toLower,SingerOn,SwitchBalloon_toUpper,SingerOff,BarbaraOn,BarbaraOff,FacePreview,Dlg_FaceOK,HairSelect,ColorSelect,VocaloidExteriorMenu,Dlg_IsVoiceRecordable,VoiceRecording,VoiceEstimation,VoiceEstimation_Hamoduo,VoiceEstimation_Result,VoiceEstimation_NoVoice,ToVocaloVoiceSequence,VocaloVoice,VocaloidStyleGamePlay,Signature,ShimobeSequence,Dlg_WaveOut,WaveOut_Tune_Play,Dlg_WaveOut_Really,SceneEnd
                //		Enum1[20] = none,CameraSequence,PlayBGM,NameInput,BarbaraOn,BarbaraOff,BarbaraNormal,BarbaraAngry,BarbaraSmile,DialogKickMe,DialogYes,VoiceRecording,VoiceEstimation,ShowNamePlate,HideNamePlate,Signature,SingerSalute,BlackOut,BlackIn,SceneEnd
                //		Enum2[6]  = none,Dlg_WhoMakeSinger,GoToRegistFlow,CameraSequence,MySingerRemakeConfirm,SceneEnd_WithoutSave
                //		Enum3[2]  = hankaku,zenkaku
                //		Enum4[3]  = plain,hankaku,zenkaku
                //		Enum5[7]  = Default,Specify,ShortInstName,ShortInstName_Num,SongPostID,SongDB_ID,BandName
                // [Other type information]
                //		Type0 = uint8
                //		Type1 = uint16, then divided by 100
                //		Type4 = int16, then divided by 100
                //		Type8 = no length / hidden?
                //		Type9 = Enum

                str = string.Concat(MSBT.ToAtoms(str).Select(atom =>
                {
                    if (atom.type == MSBT.Atom.Type.Char) return atom.ToReadableString();
                    else if (atom.type == MSBT.Atom.Type.EndCode) return "</>";
                    switch (atom.id1 * 10 + atom.id2)
                    {
                        case 02: return $"<s{BitConverter.ToInt16(atom.bytes, 0)}>"; // System.Size
                        case 03: return $"<c{colours[BitConverter.ToUInt32(atom.bytes, 0)]}>"; // System.Color
                        case 10: return $"<v>"; // Cmd.once_stop
                        case 17: return $"<v{Encoding.Unicode.GetString(atom.bytes).Substring(1)}>"; // Cmd.voice
                        case 20: return $"<w{BitConverter.ToInt16(atom.bytes, 0)}>"; // Style.ScaleX
                        case 21: return $"<h{BitConverter.ToInt16(atom.bytes, 0)}>"; // Style.VSpace
                    }
                    return $"<r{(ReplaceCode)atom.id2}:{BitConverter.ToString(atom.bytes)}>"; // Replace.*
                }));
                return symbols.Aggregate(str, (s, kvp) => s.Replace(kvp.Key, kvp.Value));
            }
            catch
            {
                return str;
            }
        }

        public string GetRawString(string str)
        {
            try
            {
                str = symbols.Aggregate(str, (s, kvp) => s.Replace(kvp.Value, kvp.Key));
                if (str.Length < 3) return str;

                return string.Concat(str.Split("<>".ToArray()).Select((z, i) =>
                {
                    if (i % 2 == 0) return z;
                    if (z == "/") return new MSBT.Atom(2, 0).ToRawString();
                    if (z == "v") return new MSBT.Atom(1, 0, new byte[0]).ToRawString();

                    var s = z.Substring(1); // shorthand
                    switch (z[0])
                    {
                        case 's': return new MSBT.Atom(0, 2, BitConverter.GetBytes(short.Parse(s))).ToRawString();
                        case 'c': return new MSBT.Atom(0, 3, BitConverter.GetBytes(colours.First(e => e.Value == s).Key)).ToRawString();
                        case 'v': return new MSBT.Atom(1, 7, new byte[] { 0x1A, 0 }.Concat(Encoding.Unicode.GetBytes(s)).ToArray()).ToRawString();
                        case 'w': return new MSBT.Atom(2, 0, BitConverter.GetBytes(short.Parse(s))).ToRawString();
                        case 'h': return new MSBT.Atom(2, 1, BitConverter.GetBytes(short.Parse(s))).ToRawString();
                    }
                    var spl = z.Split(':');
                    int id2 = (int)Enum.Parse(typeof(ReplaceCode), spl[0].Substring(1));
                    return new MSBT.Atom(3, id2, spl[1].Split('-').Select(b => Convert.ToByte(b, 16)).ToArray()).ToRawString();
                }));
            }
            catch
            {
                return str;
            }
        }

        // Previewer
        public IList<Bitmap> GeneratePreviews(IEntry entry)
        {
            var pages = new List<Bitmap>();
            if (entry == null) return pages;

            Bitmap img;
            float txtOffsetX, txtOffsetY, scale;
            float fullWidth;
            BCFNT font;

            var rawString = entry.EditedText;
            if (string.IsNullOrWhiteSpace(rawString))
                rawString = entry.OriginalText;

            if (entry.Name.StartsWith("EDT_HELP") || entry.Name.StartsWith("VCL_HELP"))
            {
                img = new Bitmap(Resources.daigasso_box);
                font = fontBasic;
                font.SetColor(Color.Black);
                fontSisterSymbol.SetColor(Color.Black);
                fullWidth = 336;
                txtOffsetX = 32;
                txtOffsetY = 12;
                scale = 0.6f;
            }
            else if (entry.Name.StartsWith("Tutorial_") || rawString.Contains("\xE\x1"))
            {
                img = new Bitmap(410, 70);
                using (var g = Graphics.FromImage(img))
                {
                    g.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);
                    g.FillRectangle(Brushes.LightYellow, 5, 6, 400, 58);
                }
                font = fontBasicRim;
                font.SetColor(Color.White);
                fullWidth = float.MaxValue;
                txtOffsetX = 5;
                txtOffsetY = 6;
                scale = 0.84f;
            }
            else
            {
                int height = 18 * (entry.OriginalText.Count(c => c == '\n') + 1);
                img = new Bitmap(226, height + 10);
                using (var g = Graphics.FromImage(img))
                {
                    g.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);
                    g.FillRectangle(Brushes.LightYellow, 3, 5, 220, height);
                }
                font = fontCbfStd;
                font.SetColor(Color.Black);
                fontSisterSymbol.SetColor(Color.Black);
                fullWidth = 220;
                txtOffsetX = 3;
                txtOffsetY = 5;
                scale = 0.6f;
            }

            float widthMultiplier = 1;
            BCFNT baseFont = font;
            using (var g = Graphics.FromImage(img))
            {
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.Bicubic;
                float x = 0, y = 0;
                for (int i = 0; i < rawString.Length; i++)
                {
                    var c = rawString[i];
                    if (c == 0xE)
                    {
                        if (rawString[i + 1] == 2 && rawString[i + 2] == 0)
                        {
                            widthMultiplier = (rawString[i + 4] + 256 * rawString[i + 5]) / 100f;
                        }
                        else if (rawString[i + 1] == 0 && rawString[i + 2] == 3)
                        {
                            font.SetColor(Color.FromArgb(rawString[i + 7], rawString[i + 4], rawString[i + 5], rawString[i + 6]));
                        }
                        i += 3 + rawString[i + 3];
                        continue;
                    }
                    if (c == 0xF)
                    {
                        i += 2;
                        widthMultiplier = 1;
                        continue;
                    }
                    font = (c >> 8 == 0xE1) ? fontSisterSymbol : baseFont; // figure out how to merge fonts

                    var char_width = font.GetWidthInfo(c).char_width * scale * widthMultiplier;
                    if (c == '\n' || x + char_width >= fullWidth)
                    {
                        x = 0;
                        y += font.LineFeed * scale;
                        if (c == '\n') continue;
                    }
                    font.Draw(c, g, x + txtOffsetX, y + txtOffsetY, scale * widthMultiplier, scale);
                    x += char_width;
                }
            }

            pages.Add(img);

            return pages;
        }

        public bool ShowSettings(Icon icon) => false;
    }
}