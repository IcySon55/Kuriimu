using System;
using System.Drawing;
using System.Drawing.Imaging;
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

        public static Bitmap Load(Stream input)
        {
            BclytSupport.NW4CSectionList sections = new BclytSupport.NW4CSectionList();
            using (var br = new BinaryReaderX(input)) sections = BclytSupport.readSections(br);

            //map elements
            sections = mapSections(sections);

            //create bmp
            Bitmap layout = createBMP(sections);

            return layout;
        }

        public static BclytSupport.NW4CSectionList mapSections(BclytSupport.NW4CSectionList sections)
        {
            foreach (var sec in sections)
            {
                using (var br2 = new BinaryReaderX(new MemoryStream(sec.Data)))
                {
                    switch (sec.Magic)
                    {
                        //Layout
                        case "lyt1":
                            sec.Obj = new BclytSupport.Layout(br2);
                            break;
                        //Texture List
                        case "txl1":
                            sec.Obj = new BclytSupport.TextureList(br2);
                            break;
                        //Materials
                        case "mat1":
                            sec.Obj = new BclytSupport.Material(br2);
                            break;
                        //Window
                        case "wnd1":
                            sec.Obj = new BclytSupport.Window(br2);
                            break;
                        //Pane
                        case "pan1":
                            sec.Obj = new BclytSupport.Pane(br2);
                            break;
                        //Picture
                        case "pic1":
                            sec.Obj = new BclytSupport.Picture(br2);
                            break;
                        case "pas1":
                        case "pae1":
                        case "grs1":
                        case "gre1":
                            break;
                        //Group
                        case "grp1":
                            sec.Obj = new BclytSupport.Group(br2);
                            break;
                    }
                }
            }

            return sections;
        }

        public static Bitmap createBMP(BclytSupport.NW4CSectionList sections)
        {
            int height = 200;
            int width = 420;
            Bitmap layout = new Bitmap(width, height);

            foreach (var sec in sections)
            {
                switch (sec.Magic)
                {
                    case "wnd1":
                        //create placeholder
                        BclytSupport.Window wnd = (BclytSupport.Window)sec.Obj;
                        float wndWidth = wnd.size.x * wnd.scale.x;
                        float wndHeight = wnd.size.y * wnd.scale.y;
                        float wndXPos = (wnd.translation.x == (float)BclytSupport.Window.XOrigin.Left) ? 0 - wndWidth / 2 : (wnd.translation.x == (float)BclytSupport.Window.XOrigin.Right) ? width - wndWidth / 2 : width / 2 - wndWidth / 2;
                        float wndYPos = (wnd.translation.y == (float)BclytSupport.Window.YOrigin.Top) ? 0 - wndHeight / 2 : (wnd.translation.y == (float)BclytSupport.Window.YOrigin.Bottom) ? height - wndHeight / 2 : height / 2 - wndHeight / 2;

                        //draw Window
                        BclytSupport.DrawLYTPart(layout, (int)wndXPos, (int)wndYPos, (int)wndWidth, (int)wndHeight);
                        break;
                    default:
                        break;
                }
            }

            return layout;
        }
    }
}
