using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;

namespace image_bclyt
{
    class BclytSupport
    {
        public static Bitmap DrawLYTPart(Bitmap bitmap, int posX, int posY, int width, int height, Color color, Color border)
        {
            int wndWidth = bitmap.Width;
            int wndHeight = bitmap.Height;

            for (int i = posY; i < posY + height; i++)
            {
                for (int j = posX; j < posX + width; j++)
                {
                    if ((i >= 0 && i < wndHeight) && (j >= 0 && j < wndWidth))
                    {
                        bitmap.SetPixel(j, i, color);
                        if (i == posY || i == posY + height - 1 || j == posX || j == posX + width - 1)
                        {
                            bitmap.SetPixel(j, i, border);
                        }
                    }
                }
            }

            return bitmap;
        }
        public static Bitmap DrawBorder(Bitmap bitmap, int posX, int posY, int width, int height, Color border)
        {
            int wndWidth = bitmap.Width;
            int wndHeight = bitmap.Height;

            for (int i = posY; i < posY + height; i++)
            {
                for (int j = posX; j < posX + width; j++)
                {
                    if ((i >= 0 && i < wndHeight) && (j >= 0 && j < wndWidth))
                    {
                        if (i == posY || i == posY + height - 1 || j == posX || j == posX + width - 1)
                        {
                            bitmap.SetPixel(j, i, border);
                        }
                    }
                }
            }

            return bitmap;
        }

        public static NW4CSectionList readSections(BinaryReaderX br)
        {
            NW4CSectionList lst = new NW4CSectionList { Header = new NW4CHeader(br) };
            lst.AddRange(from _ in Enumerable.Range(0, lst.Header.section_count)
                         let magic1 = br.ReadString(4)
                         let data = br.ReadBytes(br.ReadInt32() - 8)
                         select new NW4CSection(magic1, data));
            return lst;
        }

        public class Vector2D
        {
            public Vector2D(BinaryReaderX br)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
            }
            public float x;
            public float y;
        }

        public class Vector3D
        {
            public Vector3D(BinaryReaderX br)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
            }
            public float x;
            public float y;
            public float z;
        }

        public class TexCoord
        {
            public TexCoord(BinaryReaderX br)
            {
                TexCoordLT = new Vector2D(br);
                TexCoordRT = new Vector2D(br);
                TexCoordLB = new Vector2D(br);
                TexCoordRB = new Vector2D(br);
            }
            public Vector2D TexCoordLT;
            public Vector2D TexCoordRT;
            public Vector2D TexCoordLB;
            public Vector2D TexCoordRB;
        }

        public class Color4
        {
            public Color4(BinaryReaderX br)
            {
                r = br.ReadByte();
                g = br.ReadByte();
                b = br.ReadByte();
                a = br.ReadByte();
            }
            public byte r;
            public byte g;
            public byte b;
            public byte a;
        }

        public class NW4CHeader
        {
            public NW4CHeader(BinaryReaderX br)
            {
                magic = br.ReadString(4);
                byte_order = (ByteOrder)(br.ReadUInt16());
                header_size = br.ReadInt16();
                version = br.ReadInt32();
                file_size = br.ReadInt32();
                section_count = br.ReadInt32();
            }
            public String magic;
            public ByteOrder byte_order;
            public short header_size;
            public int version;
            public int file_size;
            public int section_count;
        }

        public class NW4CSection
        {
            public NW4CSection(String magic, byte[] data)
            {
                Magic = magic;
                Data = data;
            }
            public String Magic;
            public byte[] Data;
            public Object Obj;
        }

        public class NW4CSectionList : List<NW4CSection>
        {
            public NW4CHeader Header;
        }

        public class Layout
        {
            public Layout(BinaryReaderX br)
            {
                origin = br.ReadInt32();
                canvas_size = new Vector2D(br);
            }
            int origin; //0=Classic, 1=Normal
            Vector2D canvas_size;
        }

        public class TextureList
        {
            public TextureList(BinaryReaderX br)
            {
                txlCount = br.ReadInt32();
                offsetList = br.ReadMultiple(txlCount, _ => br.ReadInt32());
                nameList = br.ReadMultiple(txlCount, _ => br.ReadCStringA());
            }
            public int txlCount;
            public List<int> offsetList;
            public List<String> nameList;
        }

        public class Material
        {
            public Material(BinaryReaderX br)
            {
                name = br.ReadString(20);
                tevColor = br.ReadInt32();
                tevConstColors = new int[6];
                for (int i = 0; i < 6; i++) tevConstColors[i] = br.ReadInt32();
                int tmp = br.ReadInt32();
                texMap_count = (byte)(tmp & 3);
                texMatrix_count = (byte)(tmp >> 2 & 3);
                texCoordGen_count = (byte)(tmp >> 4 & 3);
                tevStage_count = (byte)(tmp >> 6 & 3);
                alphaCompare = (tmp >> 8 & 1) == 1;
                blendMode = (tmp >> 9 & 1) == 1;

                texMaps = new TextureMap[texMap_count];
                for (int i = 0; i < texMap_count; i++) texMaps[i] = new TextureMap(br);

                texMats = new TextureMatrix[texMatrix_count];
                for (int i = 0; i < texMatrix_count; i++) texMats[i] = new TextureMatrix(br);

                texCGens = new TextureCoordGen[texCoordGen_count];
                for (int i = 0; i < texCoordGen_count; i++) texCGens[i] = new TextureCoordGen(br);
            }
            public String name;
            public int tevColor;
            public int[] tevConstColors;
            public byte texMap_count;
            public byte texMatrix_count;
            public byte texCoordGen_count;
            public byte tevStage_count;
            public bool alphaCompare;
            public bool blendMode;

            public TextureMap[] texMaps;
            public TextureMatrix[] texMats;
            public TextureCoordGen[] texCGens;
        }
        public class TextureMap
        {
            public TextureMap(BinaryReaderX br)
            {
                index = br.ReadUInt16();
                byte tmp = br.ReadByte();
                wrapS = (byte)(tmp & 3);
                minFilter = (byte)(tmp >> 2 & 3);
                tmp = br.ReadByte();
                wrapT = (byte)(tmp & 3);
                magFilter = (byte)(tmp >> 2 & 3);
            }
            public ushort index;
            public byte wrapS;
            public byte minFilter;
            public byte wrapT;
            public byte magFilter;
        }
        public class TextureMatrix
        {
            public TextureMatrix(BinaryReaderX br)
            {
                translation = new Vector2D(br);
                rotation = br.ReadSingle();
                scale = new Vector2D(br);
            }
            public Vector2D translation;
            public Single rotation;
            public Vector2D scale;
        }
        public class TextureCoordGen
        {
            public TextureCoordGen(BinaryReaderX br)
            {
                udata = br.ReadUInt32();
            }
            public uint udata;
        }

        public class Pane
        {
            public Pane(BinaryReaderX br)
            {
                flags = br.ReadByte();
                byte origin = br.ReadByte();
                xorigin = (XOrigin)(origin % (byte)3);
                yorigin = (YOrigin)(origin / (byte)3);
                alpha = br.ReadByte();
                magFlags = br.ReadByte();
                paneName = br.ReadString(24);
                translation = new Vector3D(br);
                rotation = new Vector3D(br);
                scale = new Vector2D(br);
                size = new Vector2D(br);
            }
            public byte flags;
            public XOrigin xorigin;
            public YOrigin yorigin;
            public byte alpha;
            public byte magFlags;
            public String paneName;
            public Vector3D translation;
            public Vector3D rotation;
            public Vector2D scale;
            public Vector2D size;

            public enum XOrigin
            {
                Left = 0,
                Center = 1,
                Right = 2
            }
            public enum YOrigin
            {
                Top = 0,
                Center = 1,
                Bottom = 2
            }
        }

        public class Window : Pane
        {
            public Window(BinaryReaderX br) : base(br)
            {
                inflatLeft = (ushort)(br.ReadUInt16() / 16);
                inflatRight = (ushort)(br.ReadUInt16() / 16);
                inflatTop = (ushort)(br.ReadUInt16() / 16);
                inflatBottom = (ushort)(br.ReadUInt16() / 16);
                frameSizeLeft = br.ReadUInt16();
                frameSizeRight = br.ReadUInt16();
                frameSizeTop = br.ReadUInt16();
                frameSizeBottom = br.ReadUInt16();
                nrFrames = br.ReadByte();
                flag = br.ReadByte();
                padding = br.ReadUInt16();
                contOffset = br.ReadUInt32();
                frameTableOffset = br.ReadUInt32();

                br.BaseStream.Position = contOffset - 0x8;
                WindowContent content = new WindowContent(br);

                br.BaseStream.Position = frameTableOffset - 0x8;
                WindowFrame frame = new WindowFrame(br);
            }
            public ushort inflatLeft;
            public ushort inflatRight;
            public ushort inflatTop;
            public ushort inflatBottom;
            public ushort frameSizeLeft;
            public ushort frameSizeRight;
            public ushort frameSizeTop;
            public ushort frameSizeBottom;
            public byte nrFrames;
            public byte flag;
            public ushort padding;
            public uint contOffset;
            public uint frameTableOffset;
        }
        public class WindowContent
        {
            public WindowContent(BinaryReaderX br)
            {
                vertexColorLT = new Color4(br);
                vertexColorRT = new Color4(br);
                vertexColorLB = new Color4(br);
                vertexColorRB = new Color4(br);
                matID = br.ReadUInt16();
                texCoordEntryCount = br.ReadUInt16();
                TexCoordEntries = new TexCoord[texCoordEntryCount];
                for (int i = 0; i < texCoordEntryCount; i++)
                {
                    TexCoordEntries[i] = new TexCoord(br);
                }
            }
            public Color4 vertexColorLT;
            public Color4 vertexColorRT;
            public Color4 vertexColorLB;
            public Color4 vertexColorRB;
            public ushort matID;
            public ushort texCoordEntryCount;
            public TexCoord[] TexCoordEntries;
        }
        public class WindowFrame
        {
            public WindowFrame(BinaryReaderX br)
            {
                matID = br.ReadUInt16();
                textureFlip = br.ReadByte();
                padding = br.ReadByte();
            }
            public ushort matID;
            public byte textureFlip;
            public byte padding;
        }

        public class Picture
        {
            public Picture(BinaryReaderX br)
            {
                vertexColorLT = new Color4(br);
                vertexColorRT = new Color4(br);
                vertexColorLB = new Color4(br);
                vertexColorRB = new Color4(br);
                matID = br.ReadUInt16();
                texCoordEntryCount = br.ReadUInt16();
                TexCoordEntries = new TexCoord[texCoordEntryCount];
                for (int i = 0; i < texCoordEntryCount; i++)
                {
                    TexCoordEntries[i] = new TexCoord(br);
                }
            }
            public Color4 vertexColorLT;
            public Color4 vertexColorRT;
            public Color4 vertexColorLB;
            public Color4 vertexColorRB;
            public ushort matID;
            public ushort texCoordEntryCount;
            public TexCoord[] TexCoordEntries;
        }

        public class Bound : Pane
        {
            public Bound(BinaryReaderX br) : base(br)
            {

            }
        }

        public class Group
        {
            public Group(BinaryReaderX br)
            {
                groupName = br.ReadString(16);
                paneReferenceCount = br.ReadUInt32();
                paneReferences = new String[paneReferenceCount];
                for (int i = 0; i < paneReferenceCount; i++)
                {
                    paneReferences[i] = br.ReadString(16);
                }
            }
            public String groupName;
            public uint paneReferenceCount;
            public String[] paneReferences;
        }
    }
}
