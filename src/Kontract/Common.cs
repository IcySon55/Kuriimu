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

    public static class Extensions
    {
        public static string SpaceCase(this string str) => Regex.Replace(str, @"([A-Z])", @" $1").Trim();
    }
}