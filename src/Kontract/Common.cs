using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Kontract
{
    public enum LoadResult
    {
        Success,
        Failure,
        TypeMismatch,
        FileNotFound
    }

    public enum SaveResult
    {
        Success,
        Failure
    }

    public enum Applications
    {
        None,
        Kuriimu,
        Kukkii,
        Karameru
    }

    [DebuggerDisplay("{(string)this}")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic
    {
        int value;
        public static implicit operator string(Magic magic) => Encoding.ASCII.GetString(BitConverter.GetBytes(magic.value));
        public static implicit operator Magic(string s) => new Magic { value = BitConverter.ToInt32(Encoding.ASCII.GetBytes(s), 0) };
    }

    [DebuggerDisplay("{(string)this}")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic8
    {
        long value;
        public static implicit operator string(Magic8 magic) => Encoding.ASCII.GetString(BitConverter.GetBytes(magic.value));
    }

    public static class Extensions
    {
        public static string SpaceCase(this string str) => Regex.Replace(str, @"([A-Z])", @" $1").Trim();

        public static byte[] Hexlify(this string hex, int length = -1)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[(length < 0) ? NumberChars / 2 : (length + 1) & ~1];
            for (int i = 0; i < ((length < 0) ? NumberChars : length * 2); i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string Stringify(this byte[] str, int length = -1)
        {
            string result = "";
            for (int i = 0; i < (length <= -1 ? str.Length : length); i++)
                result += str[i].ToString("X2");
            return result;
        }
    }
}