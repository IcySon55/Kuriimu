using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;

namespace image_bclyt
{
    class BCLYT
    {
        public static string ToCString(byte[] bytes) => string.Concat(from b in bytes where b != 0 select (char)b);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct String4
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            byte[] bytes;
            public static implicit operator string(String4 s) => s.ToString();
            public override string ToString() => ToCString(bytes);
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct String8
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            byte[] bytes;
            public static implicit operator string(String8 s) => s.ToString();
            public override string ToString() => ToCString(bytes);
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct String16
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            byte[] bytes;
            public static implicit operator string(String16 s) => s.ToString();
            public override string ToString() => ToCString(bytes);
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct String20
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            byte[] bytes;
            public static implicit operator string(String20 s) => s.ToString();
            public override string ToString() => ToCString(bytes);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class NW4CHeader
        {
            public String4 magic;
            public ByteOrder byte_order;
            public short header_size;
            public int version;
            public int file_size;
            public int section_count;
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

        public static NW4CSectionList readSections(BinaryReaderX br)
        {
            var lst = new NW4CSectionList { Header = br.ReadStruct<NW4CHeader>() };
            lst.AddRange(from _ in Enumerable.Range(0, lst.Header.section_count)
                         let magic1 = br.ReadStruct<String4>()
                         let data = br.ReadBytes(br.ReadInt32() - 8)
                         select new NW4CSection(magic1, data));
            return lst;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Layout
        {
            int origin;
            Vector2D canvas_size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Material
        {
            String20 name;
            int tevColor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            int[] tevColors;
            int flags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Pane
        {
            public byte flags;
            public byte base_position_type;
            public byte alpha;
            public byte padding;
            public String16 name;
            public String8 userdata;
            public Vector3D translation;
            public Vector3D rotation;
            public Vector2D scale;
            public Vector2D size;
        }

        public static List<NW4CSection> sections;
        public static Bitmap Load(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                sections = readSections(br);

                foreach (var sec in sections)
                {
                    using (var br2 = new BinaryReaderX(new MemoryStream(sec.Data)))
                    {
                        switch (sec.Magic)
                        {
                            case "lyt1":
                                sec.Object = br2.ReadStruct<Layout>();
                                break;
                            case "txl1":
                                int txlCount = br2.ReadInt32();
                                br2.ReadMultiple(txlCount, _ => br2.ReadInt32());
                                sec.Object = br2.ReadMultiple(txlCount, _ => br2.ReadCStringA());
                                break;
                            case "mat1":
                                //different materials have more data than others, but have all the same base 'Material' struct
                                int matCount = br2.ReadInt32();
                                List<uint> offsets = new List<uint>();
                                List<Material> mats = new List<Material>();
                                for (int i = 0; i < matCount; i++)
                                {
                                    offsets.Add(br2.ReadUInt32());
                                }
                                for (int i = 0; i < matCount; i++)
                                {
                                    br2.BaseStream.Position = offsets[i];
                                    mats.Add(br.ReadStruct<Material>());

                                }
                                sec.Object = mats;
                                break;
                            case "wnd1":
                            case "pan1":
                                //wnd1 incomplete
                                var pane = br.ReadStruct<Pane>();
                                sec.Object = pane;
                                break;
                            case "pae1":
                            case "pas1":
                            case "grs1":
                            case "gre1":
                                break;
                            case "grp1":
                                int entryCount;
                                List<String16> names = new List<String16>();

                                names.Add(br2.ReadStruct<String16>());
                                entryCount = br2.ReadInt32();
                                for (int i = 0; i < entryCount; i++)
                                {
                                    names.Add(br2.ReadStruct<String16>());
                                }

                                sec.Object = names;
                                break;
                        }
                    }
                }

                return null;
            }
        }
    }
}
