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
    
    <n00:> - General text and menu text stuff
    

    ***********************************************
    Names of NPCs
    ***********************************************
    // Castle_King
    <n02:1E-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-4B-00-69-00-6E-00-67-00-30-00-30-00>
    <0x0E><0x0E><0x02><0x1E><0x00>C<0x00>a<0x00>s<0x00>t<0x00>l<0x00>e<0x00>0<0x00>0<0x00>_<0x00>K<0x00>i<0x00>n<0x00>g<0x00>0<0x00>0<0x00>
    // Castle_King (name)
    <n00:1E-00-43-00-61-00-73-00-74-00-6C-00-65-00-30-00-30-00-5F-00-4B-00-69-00-6E-00-67-00-30-00-30-00>
    <0x0E><0x0E><0x00><0x1E><0x00>C<0x00>a<0x00>s<0x00>t<0x00>l<0x00>e<0x00>0<0x00>0<0x00>_<0x00>K<0x00>i<0x00>n<0x00>g<0x00>0<0x00>0<0x00>

    And so on...


    ***********************************************
    Names of Items
    ***********************************************
    

*/

namespace game_miitopia_3ds
{
    using uint8 = System.Byte;
    using uint16 = System.UInt16;

    

    public class MiitopiaHandler : IGameHandler
    {
        class ControlCodeHandler
        {
            uint8 startCode;
            uint16 codeType;
            uint8 controlCodeSize; // in bytes
            uint16 baseCharLength;
            uint16 rubyTextLength;
            uint8[] rubyText; //with size of rubyTextLength
        };

        ControlCodeHandler codeHandler;

        class HexIndexPair
        {
            public HexIndexPair()
            {

            }

            private string hexCode;
            public string HexCode
            {
                get
                {
                    return hexCode;
                }
                set
                {
                    hexCode = value;
                }
            }

            private int startingIndex;
            public int StartingIndex
            {
                get
                {
                    return startingIndex;
                }
                set
                {
                    startingIndex = value;
                }
            }

            private int size;
            public int Size
            {
                get
                {
                    return size;
                }
                set
                {
                    size = value;
                }
            }
        };

        HexIndexPair[] hexIndexPairs;
        int numHexIndexPairs;
         
        string originalText;

        Dictionary<string, string> hexStringPairs = new Dictionary<string, string>
        {
            ["\x0000"] = "<0x00>", // NULL
            ["\x0001"] = "<0x01>", // Start of Heading
            ["\x0002"] = "<0x02>", // Start of Text
            ["\x0003"] = "<0x03>", // End of Text
            ["\x0004"] = "<0x04>", // End of Transmission
            ["\x0005"] = "<0x05>", // Enquiry
            ["\x0006"] = "<0x06>", // Acknowledge
            ["\x0007"] = "<0x07>", // Bell
            ["\x0008"] = "<0x08>",  // Backspace
            ["\x0009"] = "<0x09>",  // Horizontal Tabulation
            ["\x000A"] = "<0x0A_LF>",  // New Line, it looks like this is always used as a new line
            ["\x000B"] = "<0x0B>",  // Vertical Tabulation
            ["\x000C"] = "<0x0C>",  // Form Feed
            ["\x000D"] = "<0x0D>",  // Carriage Return
            ["\x000E"] = "<0x0E>",  // Shift Out
            ["\x000F"] = "<0x0F>",  // Shift In
            ["\x0010"] = "<0x10>", // Data link Escape
            ["\x0011"] = "<0x11>", // Device Control 1
            ["\x0012"] = "<0x12>", // Device Control 2
            ["\x0013"] = "<0x13>", // Device Control 3
            ["\x0014"] = "<0x14>", // Device Control 4
            ["\x0015"] = "<0x15>", // Negative Acknowledge
            ["\x0016"] = "<0x16>", // Synchronous Idle
            ["\x0017"] = "<0x17>", // End of Transmission Block
            ["\x0018"] = "<0x18>", // Cancel
            ["\x0019"] = "<0x19>",  // End of Medium
            ["\x001A"] = "<0x1A>", // Substitute
            ["\x001B"] = "<0x1B>", // Escape
            ["\x001C"] = "<0x1C>",  // File Separator
            ["\x001D"] = "<0x1D>",  // Group Separator
            ["\x001E"] = "<0x1E>",  // Record Separator
            ["\x001F"] = "<0x1F>",  // Unit Separator
            
            // still not sure about the meaning of this one
            ["\x007F"] = "<0x7F>",
        };

        public MiitopiaHandler()
        {
        }

        public bool HandlerCanGeneratePreviews
        {
            get { return true; }
        }

        public Image Icon
        {
            get { return Resources.icon_2; }
        }

        public string Name
        {
            get { return "Miitopia"; }
        }

        public Bitmap GeneratePreview(IEntry entry)
        {
            return new Bitmap(Resources.blank_top);
        }

        // Displaying the text
        public string GetKuriimuString(string str)
        {
            try
            {
                Func<string, byte[], string> Fix = (id, bytes) =>
                {
                    return $"n{(int)id[0]}{(int)id[1]}:" + BitConverter.ToString(bytes);
                };

                int i;
                //str = str.Replace("\xF\x2\0", "</>");
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
            return str;
        }

        public string GetRawString(string str)
        {
            try
            {
                if (str.Length < 3) return str;

                string result = string.Empty;
                result = string.Concat(str.Split("<>".ToArray()).Select((z, i) =>
                {
                    if (i % 2 == 0) return z;
                    //if (z == "/") return "\xF\x2\0";
                    //if (z == "v") return "\xE\x1\0\0";

                    var s = z.Substring(1);
                    string hexString = s.Substring(3);
                    int hexStringLen = hexString.Length;
                    if (hexStringLen > 0)
                    {
                        Func<string, byte[], string> Merge = (id, data) => $"\xE{id}{(char)data.Length}{string.Concat(data.Select(b => (char)b))}";

                        string[] hexStringArray = hexString.Split('-');
                        byte[] byteArray = new byte[hexStringArray.Length];

                        for (int ii = 0; ii < hexStringArray.Length; ii++)
                        {
                            byteArray[ii] = Convert.ToByte(hexStringArray[ii], 16);
                        }

                        string idHex = "" + (char)int.Parse(s.Substring(0, 1), NumberStyles.HexNumber) +
                                            (char)int.Parse(s.Substring(1, 1), NumberStyles.HexNumber);
                        return Merge(idHex, byteArray);
                    }
                    else
                    {
                        Func<string, byte[], string> MergeEmpty = (id, data) => $"\xE{id}{(char)data.Length}";
                        
                        byte[] byteArray = { };

                        string idHex = "" + (char)int.Parse(s.Substring(0, 1), NumberStyles.HexNumber) +
                                            (char)int.Parse(s.Substring(1, 1), NumberStyles.HexNumber);

                        return MergeEmpty(idHex, byteArray);
                    }
                    
                }));

                return result;
            }
            catch
            {
                return str;
            }
        }
    }
}
