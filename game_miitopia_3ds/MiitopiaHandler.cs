using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cetera.Font;
using game_miitopia_3ds.Properties;
using KuriimuContract;

/*
    ***********************************************
    Codes
    ***********************************************
    <n0.0:> - General text and menu text stuff
    
    <n3.0:00-CD> - Mii's name
    <n9.0:00-CD> - Weapon
    <n10.0:00-CD> - Armour

*/

namespace game_miitopia_3ds
{
    using uint8 = System.Byte;
    using uint16 = System.UInt16;

    public class MiitopiaHandler : IGameHandler
    {
        Dictionary<string, string> codeLabelPair = new Dictionary<string, string>
        {
            ["<n3.0:00-CD>"] = "<miiname>",
            ["<n9.0:00-CD>"] = "<weapon>",
            ["<n10.0:00-CD>"] = "<armor>",
            ["<n2.0:00-06>"] = "<#gold>",
            ["<n2.0:01-06>"] = "<#needgold>",
            ["<n14.0:1E-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-4B-00-69-00-6E-00-67-00-30-00-30-00>"] = "<king>",
            ["<n14.0:26-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-4E-00-6F-00-62-00-6C-00-65-00-4B-00-69-00-64-00-30-00-30-00>"] = "<noble>",
            ["<n14.0:26-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-50-00-72-00-69-00-6E-00-63-00-65-00-73-00-73-00-30-00-30-00>"] = "<princess>",
            ["<n14.0:22-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-50-00-72-00-69-00-6E-00-63-00-65-00-30-00-30-00>"] = "<prince>",
            ["<n14.0:18-00-54-00-6F-00-77-00-6E-00-30-00-31-00-5F-00-47-00-65-00-6E-00-69-00-65-00>"] = "<genie>",
            ["<n14.0:24-00-54-00-6F-00-77-00-6E-00-30-00-32-00-5F-00-53-00-69-00-73-00-74-00-65-00-72-00-46-00-69-00-72-00-73-00-74-00>"] = "<yellowelf>",
            ["<n14.0:26-00-54-00-6F-00-77-00-6E-00-30-00-32-00-5F-00-53-00-69-00-73-00-74-00-65-00-72-00-53-00-65-00-63-00-6F-00-6E-00-64-00>"] = "<redelf>",
            ["<n14.0:24-00-54-00-6F-00-77-00-6E-00-30-00-32-00-5F-00-53-00-69-00-73-00-74-00-65-00-72-00-54-00-68-00-69-00-72-00-64-00>"] = "<purpleelf>",
            ["<n14.0:0A-00-53-00-61-00-74-00-61-00-6E-00>"] = "<satan>",
            ["<n14.0:08-00-53-00-61-00-67-00-65-00>"] = "<sage>",
            ["<n14.0:1A-00-52-00-65-00-69-00-6E-00-63-00-61-00-72-00-6E-00-61-00-74-00-69-00-6F-00-6E-00>"] = "<reincarn>",
        };

        public MiitopiaHandler()
        {
        }

        public string Name { get; } = "Miitopia";

        // Displaying the text
        public string GetKuriimuString(string str)
        {
            try
            {
                Func<string, byte[], string> Fix = (id, bytes) =>
                {
                    return $"n{(int)id[0]}.{(int)id[1]}:" + BitConverter.ToString(bytes);
                };

                int i;
                while ((i = str.IndexOf('\xE')) >= 0)
                {
                    var id = str.Substring(i + 1, 2);
                    var data = str.Substring(i + 4, str[i + 3]).Select(c => (byte)c).ToArray();
                    str = str.Remove(i, data.Length + 4).Insert(i, $"<{Fix(id, data)}>");
                }
            }
            catch
            {

            }
            str = codeLabelPair.Aggregate(str, (s, pair) => s.Replace(pair.Key, pair.Value));

            return str;
        }

        public string GetRawString(string str)
        {
            try
            {
                if (str.Length < 3)
                {
                    return str;
                }

                str = codeLabelPair.Aggregate(str, (s, pair) => s.Replace(pair.Value, pair.Key));
                string result = string.Empty;
                result = string.Concat(str.Split("<>".ToArray()).Select((codeString, i) =>
                {
                    // codeString = "n00:code"
                    if (i % 2 == 0)
                    {
                        return codeString;
                    }

                    // "0.0:code" part, the identyfier ("n") infront of 0.0 is stripped
                    var codeStringRaw = codeString.Substring(1);

                    // separate the code id "00.00" and the hex code "00-00""
                    string[] codeStringArray = codeStringRaw.Split(':');

                    // get the ID part
                    string[] idString = codeStringArray[0].Split('.');

                    // get the hex string with the ID ("X.X") part stripped
                    string hexString = codeStringArray[1];
                    int hexStringLen = hexString.Length;
                    if (hexStringLen > 0)
                    {
                        Func<string, byte[], string> Merge = (id, data) => $"\xE{id}{(char)data.Length}{string.Concat(data.Select(b => (char)b))}";

                        byte[] byteArray = hexString.Split('-').Select(piece => Convert.ToByte(piece, 16)).ToArray();

                        string idHex = "" + (char)int.Parse(idString[0]) +
                                            (char)int.Parse(idString[1]);
                        return Merge(idHex, byteArray);
                    }
                    else
                    {
                        Func<string, int, string> MergeEmpty = (id, length) => $"\xE{id}{(char)length}";
                        
                        string idHex = "" + (char)int.Parse(idString[0]) +
                                            (char)int.Parse(idString[1]);

                        return MergeEmpty(idHex, 0);
                    }
                    
                }));

                return result;
            }
            catch
            {
                return str;
            }
        }

        public bool HandlerCanGeneratePreviews { get; } = true;

        public bool HandlerHasSettings { get; } = false;

        // Show the settings dialog
        public bool ShowSettings(Icon icon)
        {
            return false;
        }

        public IList<Bitmap> GeneratePreviews(IEntry entry)
        {
            return new List<Bitmap>();
        }

        public Image Icon
        {
            get { return Resources.icon_2; }
        }

        public Bitmap GeneratePreview(IEntry entry)
        {
            return new Bitmap(Resources.blank_top);
        }
    }
}
