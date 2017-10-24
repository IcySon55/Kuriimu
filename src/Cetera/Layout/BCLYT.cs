using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Cetera.IO;
using Kontract.IO;

namespace Cetera.Layout
{
    public class BCLYT
    {
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

        public enum AlignText : byte
        {
            Sync,
            Left,
            Center,
            Right
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TextBox
        {
            public short buffer_length;
            public short string_length;
            public short materialID;
            public short fontID;
            public byte position_type;
            public AlignText text_align;
            public short padding;
            public int text_offset;
            public int color_top;
            public int color_bottom;
            public Vector2D font_size;
            public Vector2D font_kerning;
        }

        public List<NW4CSection> sections;
        public BCLYT(Stream input)
        {
            using (var sr = new BinaryReaderX(input))
            {
                sections = sr.ReadSections();
                NW4CSection last = null;
                List<string> fnl = null;

                foreach (var sec in sections)
                {
                    using (var br = new BinaryReaderX(new MemoryStream(sec.Data)))
                    {
                        switch (sec.Magic)
                        {
                            case "lyt1":
                                sec.Object = br.ReadStruct<Layout>();
                                break;
                            case "txl1":
                            case "fnl1":
                                int txlCount = br.ReadInt32();
                                br.ReadMultiple(txlCount, _ => br.ReadInt32());
                                sec.Object = br.ReadMultiple(txlCount, _ => br.ReadCStringA());
                                break;
                            case "mat1":
                                // incomplete
                                break;
                            case "pic1":
                            // incomplete
                            case "pan1":
                            // incomplete
                            case "wnd1":
                            // incomplete
                            case "bnd1":
                                // incomplete
                                var pane = br.ReadStruct<Pane>();
                                sec.Object = pane;

                                //if (sec.Magic == "pic1")
                                //Debug.WriteLine($"<{sec.Magic} name='{pane.name}' size='{pane.size.x},{pane.size.y}'>");

                                break;
                            case "txt1":
                                var txtPane = br.ReadStruct<Pane>();
                                var txtBox = br.ReadStruct<TextBox>();
                                var str = "";
                                if (txtBox.string_length != 0) str = br.ReadCStringW();
                                //sec.Object = new { txtPane, txtBox, str };
                                sec.Object = Tuple.Create(txtPane, txtBox, str);
                                break;
                            case "pts1":
                            case "pas1":
                            case "pae1":
                            case "grs1":
                            case "gre1":
                                break;
                            case "grp1":
                                // incomplete
                                break;
                            case "usd1":
                                if (last != null && last.Magic == "txt1")
                                {
                                    var x = string.Concat(sec.Data.Skip(16).Select(b => (char)b)).TrimEnd('\0').Replace("\0", "|");
                                    var t = ((Tuple<Pane, TextBox, string>)last.Object);
                                    //if (t.Item3 != "") throw new Exception();
                                    last.Object = Tuple.Create(t.Item1, t.Item2, x);
                                }
                                // incomplete
                                break;
                            default:
                                throw new NotSupportedException($"Unknown magic {sec.Magic}");
                        }
                    }
                    last = sec;
                }

                var txts = sections.Where(z => z.Magic == "txt1").Select(z => z.Object).ToList();
            }
        }
    }
}
