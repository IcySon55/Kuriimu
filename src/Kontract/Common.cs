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
    }
}