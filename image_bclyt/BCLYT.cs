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
using Cetera.Font;

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
                            sec.Obj = new BclytSupport.NameList(br2);
                            break;
                        case "fnl1":
                            sec.Obj = new BclytSupport.NameList(br2);
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
                        case "bnd1":
                            sec.Obj = new BclytSupport.Bound(br2);
                            break;
                        case "txt1":
                            sec.Obj = new BclytSupport.Text(br2);
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
            int height = 240;
            int width = 400;
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
                        float wndXPos = (wnd.xorigin == BclytSupport.Window.XOrigin.Left) ? 0 - wndWidth / 2 + wnd.translation.x : (wnd.xorigin == BclytSupport.Window.XOrigin.Right) ? width - wndWidth / 2 + wnd.translation.x : width / 2 - wndWidth / 2 + wnd.translation.x;
                        float wndYPos = (wnd.yorigin == BclytSupport.Window.YOrigin.Top) ? 0 - wndHeight / 2 - wnd.translation.y : (wnd.yorigin == BclytSupport.Window.YOrigin.Bottom) ? height - wndHeight / 2 - wnd.translation.y : height / 2 - wndHeight / 2 - wnd.translation.y;

                        //draw Window
                        BclytSupport.DrawLYTPart(layout, (int)wndXPos, (int)wndYPos, (int)wndWidth, (int)wndHeight, Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 255, 255, 255));
                        break;
                    case "pan1":
                        //create placeholder
                        BclytSupport.Pane pan = (BclytSupport.Pane)sec.Obj;
                        float panWidth = pan.size.x * pan.scale.x;
                        float panHeight = pan.size.y * pan.scale.y;
                        float panXPos = (pan.xorigin == BclytSupport.Pane.XOrigin.Left) ? 0 - panWidth / 2 + pan.translation.x : (pan.xorigin == BclytSupport.Pane.XOrigin.Right) ? width - panWidth / 2 + pan.translation.x : width / 2 - panWidth / 2 + pan.translation.x;
                        float panYPos = (pan.yorigin == BclytSupport.Pane.YOrigin.Top) ? 0 - panHeight / 2 - pan.translation.y : (pan.yorigin == BclytSupport.Pane.YOrigin.Bottom) ? height - panHeight / 2 - pan.translation.y : height / 2 - panHeight / 2 - pan.translation.y;

                        //draw Pane
                        BclytSupport.DrawLYTPart(layout, (int)panXPos, (int)panYPos, (int)panWidth, (int)panHeight, Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 255, 255, 255));
                        break;
                    case "bnd1":
                        //create placeholder
                        BclytSupport.Bound bnd = (BclytSupport.Bound)sec.Obj;
                        float bndWidth = bnd.size.x * bnd.scale.x;
                        float bndHeight = bnd.size.y * bnd.scale.y;
                        float bndXPos = (bnd.xorigin == BclytSupport.Bound.XOrigin.Left) ? 0 - bndWidth / 2 + bnd.translation.x : (bnd.xorigin == BclytSupport.Bound.XOrigin.Right) ? width - bndWidth / 2 + bnd.translation.x : width / 2 - bndWidth / 2 + bnd.translation.x;
                        float bndYPos = (bnd.yorigin == BclytSupport.Bound.YOrigin.Top) ? 0 - bndHeight / 2 - bnd.translation.y : (bnd.yorigin == BclytSupport.Bound.YOrigin.Bottom) ? height - bndHeight / 2 - bnd.translation.y : height / 2 - bndHeight / 2 - bnd.translation.y;

                        //draw Bound
                        BclytSupport.DrawLYTPart(layout, (int)bndXPos, (int)bndYPos, (int)bndWidth, (int)bndHeight, Color.FromArgb(127, 127, 255, 127), Color.FromArgb(255, 127, 255, 127));
                        break;
                    case "txt1":
                        //create placeholder
                        BclytSupport.Text txt = (BclytSupport.Text)sec.Obj;
                        float txtWidth = txt.size.x * txt.scale.x;
                        float txtHeight = txt.size.y * txt.scale.y;
                        float txtXPos = (txt.xorigin == BclytSupport.Text.XOrigin.Left) ? 0 - txtWidth / 2 + txt.translation.x : (txt.xorigin == BclytSupport.Text.XOrigin.Right) ? width - txtWidth / 2 + txt.translation.x : width / 2 - txtWidth / 2 + txt.translation.x;
                        float txtYPos = (txt.yorigin == BclytSupport.Text.YOrigin.Top) ? 0 - txtHeight / 2 - txt.translation.y : (txt.yorigin == BclytSupport.Text.YOrigin.Bottom) ? height - txtHeight / 2 - txt.translation.y : height / 2 - txtHeight / 2 - txt.translation.y;

                        //draw Textbox and Text
                        BclytSupport.DrawLYTPart(layout, (int)txtXPos, (int)txtYPos, (int)txtWidth, (int)txtHeight, Color.FromArgb(63, 127, 127, 127), Color.FromArgb(255, 127, 127, 127));
                        BclytSupport.DrawText(layout, txt.text, (int)txtXPos, (int)txtYPos, Color.FromArgb(255, 255, 255, 255));
                        break;
                    default:
                        break;
                }
            }

            //mark border from rootpane
            BclytSupport.DrawBorder(layout, 0, 0, width - 1, height - 1, Color.FromArgb(255, 0, 255, 0));
            return layout;
        }
    }
}