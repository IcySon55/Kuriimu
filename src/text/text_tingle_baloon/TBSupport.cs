using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract.IO;
using System.IO;
using System.Text;
using System;
using System.Globalization;
using System.Linq;

namespace text_tb
{
    #region Entry_Definition
    public sealed class Entry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return EditedLabel.Name; }
            set { }
        }

        public string OriginalText => OriginalLabel.Text;

        public string EditedText
        {
            get { return EditedLabel.Text; }
            set { EditedLabel.Text = value; }
        }

        public int MaxLength { get; set; }

        public TextEntry ParentEntry { get; set; }

        public bool IsSubEntry => ParentEntry != null;

        public bool HasText { get; }

        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Label OriginalLabel { get; }
        public Label EditedLabel { get; set; }

        public Entry()
        {
            OriginalLabel = new Label();
            EditedLabel = new Label();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public Entry(Label editedLabel) : this()
        {
            if (editedLabel != null)
                EditedLabel = editedLabel;
        }

        public Entry(Label editedLabel, Label originalLabel) : this(editedLabel)
        {
            if (originalLabel != null)
                OriginalLabel = originalLabel;
        }

        public override string ToString()
        {
            return Name == string.Empty ? "!NoName!" : Name;
        }
    }
    #endregion

    #region Label_Definition
    public sealed class Label
    {
        public string Name;
        public string Text;
        public int TextID;

        public Label()
        {
            Name = string.Empty;
            Text = string.Empty;
            TextID = 0;
        }
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public short tableInfoCount;
        public int stringCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TableInfo
    {
        public int tableID;
        public int tableOffset;
        public int textOffset;
        public int blockLength;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TableEntry
    {
        public short unk1;
        public short id;
        public short unk2;
        public short length;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Block
    {
        public TableInfo info;
        public List<TableEntry> entries = new List<TableEntry>();
        public List<string> strings = new List<string>();
        public byte[] block;
    }

    public class TBSupport
    {
        public static string GetString(Stream input, int length)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var encoding = new CustomEncoding();

                return encoding.GetString(br.ReadBytes(length));
            }
        }

        public static byte[] GetBytes(string text)
        {
            var encoding = new CustomEncoding();

            var byteCount = encoding.GetByteCount(text);
            var bytes = encoding.GetBytes(text);
            bytes.ToList().RemoveRange(byteCount, bytes.Length - byteCount);

            return bytes;
        }
    }

    public class CustomEncoding
    {
        /*This custom Encoding class was derived and adjusted by onepiecefreak
         The original version was written by DarkNemesis, without him this Encoding weren't possible*/
        private Dictionary<ushort, char> codeToChar = new Dictionary<ushort, char>();
        private Dictionary<char, ushort> charToCode = new Dictionary<char, ushort>();

        public CustomEncoding()
        {
            // UTF-16 punctuation, symbols, and latin alphabet
            // Skip 0x3C and 0x3E (< and >) since they're's used as special characters in the text files.
            // They will be represented by <lt> and <gt> instead.
            GenerateUTF16Range(0x0000, 0x0020, 28);
            GenerateUTF16Range(0x001D, 0x003D, 1);
            GenerateUTF16Range(0x001F, 0x003F, 64);

            // Shift_JIS punctuation and symbols
            GenerateShiftJISRange(0x00E0, 0x8140, 63);
            GenerateShiftJISRange(0x011F, 0x8180, 45);

            // Shift_JIS hiragana
            GenerateShiftJISRange(0x0151, 0x829F, 83);

            // Shift_JIS katakana
            GenerateShiftJISRange(0x01B1, 0x8340, 63);
            GenerateShiftJISRange(0x01F0, 0x8380, 23);
        }

        public string GetString(byte[] bytes)
        {
            string result = "";

            for (int i = 0; i < bytes.Length; i += 2)
            {
                ushort code = BitConverter.ToUInt16(bytes, i);
                if (codeToChar.ContainsKey(code))
                {
                    result += codeToChar[code];
                    continue;
                }
                else if (code == 0xFFFF)
                {
                    continue;
                }
                else if (code >= 0xF000 && code <= 0xF00F)
                {
                    result += "<" + "0123456789ABCDEF"[code & 0xF] + ">";
                }
                else if (code >= 0xF100 && code <= 0xF10F)
                {
                    i += 2;
                    string val = BitConverter.ToUInt16(bytes, i).ToString(CultureInfo.InvariantCulture);
                    result += "<" + "0123456789ABCDEF"[code & 0xF] + ":" + val + ">";
                }
                else if (code == 0x001C)
                {
                    result += "<lt>";
                }
                else if (code == 0x001E)
                {
                    result += "<gt>";
                }
                else if (code == 0x0251 || code == 0x02D4 || code == 0x03BE || code == 0x0500)
                {
                    string val = string.Format(CultureInfo.InvariantCulture, "{0:X4}", code);
                    result += "<h" + val + ">";
                }
                else
                {
                    throw new InvalidDataException("Unsupported character [" + code + "]");
                }
            }

            return result;
        }

        public byte[] GetBytes(string text)
        {
            var result = new byte[(text.Length + 1) * 2];
            var byteIndex = 0;

            int byteCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '<')
                {
                    i++;
                    if (text[i] == 'l' && text[i + 1] == 't')
                    {
                        result[byteIndex++] = 0x1C;
                        result[byteIndex++] = 0x00;
                        byteCount += 2;
                        i += 2;
                    }
                    else if (text[i] == 'g' && text[i + 1] == 't')
                    {
                        result[byteIndex++] = 0x1E;
                        result[byteIndex++] = 0x00;
                        byteCount += 2;
                        i += 2;
                    }
                    else if (text[i] == 'h')
                    {
                        result[byteIndex++] = System.Convert.ToByte(string.Empty + text[i + 3] + text[i + 4], 16);
                        result[byteIndex++] = System.Convert.ToByte(string.Empty + text[i + 1] + text[i + 2], 16);
                        byteCount += 2;
                        i += 5;
                    }
                    else if ("012345CDE".IndexOf(text[i]) != -1)
                    {
                        result[byteIndex++] = (byte)"0123456789ABCDEF".IndexOf(text[i]);
                        result[byteIndex++] = 0xF0;
                        byteCount += 2;
                        i++;
                    }
                    else if ("6789ABF".IndexOf(text[i]) != -1)
                    {
                        result[byteIndex++] = (byte)"0123456789ABCDEF".IndexOf(text[i]);
                        result[byteIndex++] = 0xF1;
                        string val = string.Empty;
                        i += 2;
                        while (text[i] != '>')
                        {
                            val += text[i++];
                        }

                        ushort code = ushort.Parse(val, CultureInfo.InvariantCulture);
                        byte[] data = BitConverter.GetBytes(code);
                        Array.Copy(data, 0, result, byteIndex, 2);
                        byteIndex += 2;
                        byteCount += 4;
                    }
                }
                else if (this.charToCode.ContainsKey(text[i]))
                {
                    ushort code = this.charToCode[text[i]];
                    byte[] data = BitConverter.GetBytes(code);
                    Array.Copy(data, 0, result, byteIndex, 2);
                    byteIndex += 2;
                    byteCount += 2;
                }
                else
                {
                    throw new InvalidDataException("Unsupported character [" + text[i] + "]");
                }
            }

            result[byteIndex++] = 0xFF;
            result[byteIndex++] = 0xFF;

            return result;
        }

        public int GetByteCount(string text)
        {
            int byteCount = 2;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '<')
                {
                    i++;
                    if (text[i] == 'l' && text[i + 1] == 't')
                    {
                        byteCount += 2;
                        i += 2;
                    }
                    else if (text[i] == 'g' && text[i + 1] == 't')
                    {
                        byteCount += 2;
                        i += 2;
                    }
                    else if (text[i] == 'h')
                    {
                        byteCount += 2;
                        i += 5;
                    }
                    else if ("012345CDE".IndexOf(text[i]) != -1)
                    {
                        byteCount += 2;
                        i++;
                    }
                    else if ("6789ABF".IndexOf(text[i]) != -1)
                    {
                        i += 2;
                        while (text[i] != '>')
                        {
                            i++;
                        }

                        byteCount += 4;
                    }
                }
                else if (this.charToCode.ContainsKey(text[i]))
                {
                    byteCount += 2;
                }
                else
                {
                    throw new InvalidDataException("Unsupported character [" + text[i] + "]");
                }
            }

            return byteCount;
        }

        private void GenerateShiftJISRange(ushort codeStart, ushort shiftJisStart, int length)
        {
            byte[] jisCode = new byte[2];
            jisCode[0] = (byte)(shiftJisStart >> 8);
            jisCode[1] = (byte)(shiftJisStart & 0xFF);
            byte[] jisBytes = new byte[length * 2];
            for (int i = 0; i < length * 2; i += 2)
            {
                jisBytes[i] = jisCode[0];
                jisBytes[i + 1] = jisCode[1];
                jisCode[1]++;
            }

            ushort code = codeStart;
            string chars = Encoding.GetEncoding("Shift_JIS").GetString(jisBytes);
            for (int i = 0; i < length; i++)
            {
                this.AddCharacter(code++, chars[i]);
            }
        }

        private void GenerateUTF16Range(ushort codeStart, ushort utfStart, int length)
        {
            ushort code = codeStart;
            char utfChar = (char)utfStart;
            for (int i = 0; i < length; i++)
            {
                this.AddCharacter(code, utfChar);
                code++;
                utfChar++;
            }
        }

        private void AddCharacter(ushort code, char character)
        {
            this.codeToChar.Add(code, character);
            this.charToCode.Add(character, code);
        }
    }
}
