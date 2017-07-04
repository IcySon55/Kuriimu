using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.IO;

namespace Cetera
{
    static class CommonExtensions
    {
        public static string ToCString(this byte[] bytes) => string.Concat(from b in bytes where b != 0 select (char)b);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct String4
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        byte[] bytes;
        public static implicit operator string(String4 s) => s.ToString();
        public override string ToString() => bytes.ToCString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct String8
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        byte[] bytes;
        public static implicit operator string(String8 s) => s.ToString();
        public override string ToString() => bytes.ToCString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct String16
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        byte[] bytes;
        public static implicit operator string(String16 s) => s.ToString();
        public override string ToString() => bytes.ToCString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct String20
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        byte[] bytes;
        public static implicit operator string(String20 s) => s.ToString();
        public override string ToString() => bytes.ToCString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NW4CHeader
    {
        public String4 magic;
        public ByteOrder byte_order;
        public short header_size;
        public int version;
        public int file_size;
        public short section_count;
        public short padding;
    };

    [DebuggerDisplay("{Magic,nq}: {Data.Length} bytes")]
    public class NW4CSection
    {
        public string Magic { get; }
        public byte[] Data { get; set; }
        public object Object { get; set; }

        public NW4CSection(string magic, byte[] data)
        {
            Magic = magic;
            Data = data;
        }
    }

    public class NW4CSectionList : List<NW4CSection>
    {
        public NW4CHeader Header { get; set; }
    }

    [DebuggerDisplay("{x}, {y}")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vector2D
    {
        public float x, y;
        public override string ToString() => $"{x},{y}";
    }

    [DebuggerDisplay("{x}, {y}, {z}")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vector3D
    {
        public float x, y, z;
        public override string ToString() => $"{x},{y},{z}";
    }

    class Common
    {
    }
}
