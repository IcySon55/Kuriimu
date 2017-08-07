using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using LibMobiclip.Utils;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_mods
{
    public unsafe class ModsDecoder
    {
        public uint Width;
        public uint Height;

        public byte[] Data;
        public int Offset = 0;
        public byte[][] Y = new byte[6][];
        public byte[][] UV = new byte[6][];
        public ushort[] Table0A;
        public byte[] Table0B;
        public ushort[] Table1A;
        public byte[] Table1B;
        public byte[] MinMaxTable;
        public uint Quantizer = 0;
        public uint YuvFormat;
        public uint[] Internal = new uint[392];

        public int Stride = 512;

        public ModsDecoder(uint Width, uint Height)
        {
            this.Width = Width;
            this.Height = Height;

            Table0A = ModsConst.Vx2Table0_A;
            Table0B = ModsConst.Vx2Table0_B;
            Table1A = ModsConst.Vx2Table1_A;
            Table1B = ModsConst.Vx2Table1_B;
            MinMaxTable = ModsConst.Vx2MinMaxTable;

            if (Width <= 256) Stride = 256;
            else if (Width <= 512) Stride = 512;
            else Stride = 1024;
        }

        public Bitmap DecodeFrame()
        {
            Bitmap bb4 = null;
            try
            {
                for (int i = 5; i > 0; i--)
                {
                    Y[i] = Y[i - 1];
                    UV[i] = UV[i - 1];
                }
                Y[0] = new byte[Stride * Height];
                UV[0] = new byte[Stride * Height / 2];
                int nrBitsRemaining = 0;
                uint r3 = IOUtil.ReadU16LE(Data, Offset);
                Offset += 2;
                r3 <<= 16;
                bool Iframe = (((ulong)r3 + (ulong)r3) >> 32) == 1;
                r3 += r3;
                if (!Iframe)
                {
                    if (--nrBitsRemaining < 0)
                        FillBits(ref nrBitsRemaining, ref r3);

                    int r6 = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
                    if (r6 != 0)
                    {
                        SetupQuantizationTables((uint)(Quantizer + r6));
                    }

                    Internal[218] = 0;//use table 0
                    int internaloffset = 221;
                    int w = (int)Width + 0x20;
                    while (true)
                    {
                        Internal[internaloffset] = 0;
                        Internal[internaloffset + 1] = 0;
                        internaloffset += 2;
                        w -= 0x10;
                        if (w <= 0) break;
                    }
                    int r11 = 0;
                    int h = (int)Height;
                    while (true)
                    {
                        w = (int)Width;
                        internaloffset = 221;
                        while (true)
                        {
                            int[] vals = new int[6];
                            vals[0] = (int)Internal[internaloffset];
                            vals[1] = (int)Internal[internaloffset + 1];
                            vals[2] = (int)Internal[internaloffset + 2];
                            vals[3] = (int)Internal[internaloffset + 3];
                            vals[4] = (int)Internal[internaloffset + 4];
                            vals[5] = (int)Internal[internaloffset + 5];
                            internaloffset += 2;
                            if (vals[0] > vals[2])
                            {
                                int tmp = vals[0];
                                vals[0] = vals[2];
                                vals[2] = tmp;
                            }
                            if (vals[2] > vals[4])
                            {
                                int tmp = vals[2];
                                vals[2] = vals[4];
                                vals[4] = tmp;
                            }
                            if (vals[0] > vals[2])
                            {
                                int tmp = vals[0];
                                vals[0] = vals[2];
                                vals[2] = tmp;
                            }
                            if (vals[1] > vals[3])
                            {
                                int tmp = vals[1];
                                vals[1] = vals[3];
                                vals[3] = tmp;
                            }
                            if (vals[3] > vals[5])
                            {
                                int tmp = vals[3];
                                vals[3] = vals[5];
                                vals[5] = tmp;
                            }
                            if (vals[1] > vals[3])
                            {
                                int tmp = vals[1];
                                vals[1] = vals[3];
                                vals[3] = tmp;
                            }
                            Internal[219] = (uint)vals[2];
                            Internal[219 + 1] = (uint)vals[3];
                            Internal[internaloffset] = 0;
                            Internal[internaloffset + 1] = 0;
                            ReadPBlock16x16(ref nrBitsRemaining, ref r3, internaloffset, r11);
                            r11 += 0x10;
                            w -= 0x10;
                            if (w <= 0) break;
                        }
                        r11 += Stride * 16;//0x1000;
                        r11 -= (int)Width;
                        h -= 0x10;
                        if (h <= 0) break;
                    }
                }
                else
                {
                    YuvFormat = (uint)(((ulong)r3 + (ulong)r3) >> 32) & 1;
                    r3 += r3;
                    uint Table = (uint)(((ulong)r3 + (ulong)r3) >> 32) & 1;
                    Internal[218] = Table;
                    r3 += r3;
                    nrBitsRemaining -= 3;
                    if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                    uint quantizer = r3 >> 26;
                    r3 <<= 6;
                    nrBitsRemaining -= 6;
                    if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                    if (Quantizer != quantizer)
                        SetupQuantizationTables(quantizer);
                    int r11 = 0;
                    int h = (int)Height;
                    while (true)
                    {
                        int w = (int)Width;
                        while (true)
                        {
                            bool PredictPMode = (((ulong)r3 + (ulong)r3) >> 32) == 1;
                            r3 += r3;
                            nrBitsRemaining--;
                            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                            if (PredictPMode) DecIntraSubBlockPMode(ref nrBitsRemaining, ref r3, r11);
                            else DecIntraFullBlockPMode(ref nrBitsRemaining, ref r3, r11);
                            r11 += 0x10;
                            w -= 0x10;
                            if (w <= 0) break;
                        }
                        r11 += Stride * 16;//0x1000;
                        r11 -= (int)Width;
                        h -= 0x10;
                        if (h <= 0) break;
                    }
                }

                bb4 = new Bitmap((int)Width, (int)Height);
                BitmapData d = bb4.LockBits(new Rectangle(0, 0, bb4.Width, bb4.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        float Y2 = Y[0][y * Stride + x];
                        float U = UV[0][y / 2 * Stride + x / 2] - 128f;
                        float V = UV[0][y / 2 * Stride + x / 2 + Stride / 2] - 128f;
                        if (x != Width - 1 && y != Height - 1)
                        {
                            switch ((x & 1) | ((y & 1) << 1))
                            {
                                case 1:
                                    U += UV[0][y / 2 * Stride + x / 2 + 1] - 128f;
                                    V += UV[0][y / 2 * Stride + x / 2 + 1 + Stride / 2] - 128f;
                                    U /= 2f;
                                    V /= 2f;
                                    break;
                                case 2:
                                    U += UV[0][y / 2 * Stride + x / 2 + Stride] - 128f;
                                    V += UV[0][y / 2 * Stride + x / 2 + Stride + Stride / 2] - 128f;
                                    U /= 2f;
                                    V /= 2f;
                                    break;
                                case 3:
                                    U += UV[0][y / 2 * Stride + x / 2 + 1] - 128f;
                                    V += UV[0][y / 2 * Stride + x / 2 + 1 + Stride / 2] - 128f;
                                    U += UV[0][y / 2 * Stride + x / 2 + Stride] - 128f;
                                    V += UV[0][y / 2 * Stride + x / 2 + Stride + Stride / 2] - 128f;
                                    U += UV[0][y / 2 * Stride + x / 2 + 1 + Stride] - 128f;
                                    V += UV[0][y / 2 * Stride + x / 2 + 1 + Stride + Stride / 2] - 128f;
                                    U /= 4f;
                                    V /= 4f;
                                    break;
                            }
                        }
                        float R, G, B;


                        R = /*Vx2BlitTable[0x100 + */(int)Y2 + (int)U - (int)V;//];//Y2 - U - V;
                        G = /*Vx2BlitTable[0x100 + */(int)Y2 + (int)V;//];
                        B = /*Vx2BlitTable[0x100 + */(int)Y2 - (int)U - (int)V;//];

                        if (R < 0) R = 0;
                        if (R > 255) R = 255;
                        if (G < 0) G = 0;
                        if (G > 255) G = 255;
                        if (B < 0) B = 0;
                        if (B > 255) B = 255;
                        ((int*)(((byte*)d.Scan0) + y * d.Stride + x * 4))[0] = Color.FromArgb((int)(R), (int)(G), (int)(B)).ToArgb();
                    }
                }
                bb4.UnlockBits(d);
            }
            catch
            { }
            end:
            return bb4;
        }

        private void sub_114790(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int Offset)
        {
            int dx = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            int dy = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            dx += (int)Internal[219];
            dy += (int)Internal[220];
            loc_1147B0(ref nrBitsRemaining, ref r3, internaloffset, srcFrame, height, dx, dy, Offset);
        }

        private void loc_1147B0(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int dx, int dy, int Offset)
        {
            Internal[internaloffset] = (uint)dx;
            Internal[internaloffset + 1] = (uint)dy;
            CopyBlock(Y[srcFrame / 4], dx, dy, 16, height, Y[0], Offset);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 8, height >> 1, UV[0], Offset / 2);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 8, height >> 1, UV[0], Offset / 2 + Stride / 2);
        }

        private void CopyBlock(byte[] Src, int Dx, int Dy, uint Width, uint Height, byte[] Dst, int Offset)
        {
            for (int i = 0; i < Height; i++)
            {
                byte[] pixels = new byte[Width];
                int pos = Offset + (((Dy >> 1) + i) * Stride) + (Dx >> 1);
                switch ((Dx & 1) | ((Dy & 1) << 1))
                {
                    case 0:
                        Array.Copy(Src, pos, pixels, 0, Width);
                        break;
                    case 1:
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                pixels[j] = (byte)((Src[pos + j] >> 1) + (Src[pos + j + 1] >> 1));
                            }
                            break;
                        }
                    case 2:
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                pixels[j] = (byte)((Src[pos + j] >> 1) + (Src[pos + j + Stride] >> 1));
                            }
                            break;
                        }
                    case 3:
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                pixels[j] = (byte)((((Src[pos + j] >> 1) + (Src[pos + j + 1] >> 1)) >> 1) + (((Src[pos + j + Stride] >> 1) + (Src[pos + j + 1 + Stride] >> 1)) >> 1));
                            }
                            break;
                        }
                }
                Array.Copy(pixels, 0, Dst, Offset + i * Stride, Width);
            }
        }

        private byte[] byte_114884 =
        {
            0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x02, 0x02, 0x02, 0x02,
            0x03, 0x03, 0x06, 0x06, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x07, 0x07, 0x05, 0x04, 0x09, 0x09, 0x09, 0x09, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        };

        //SwitchDword_1148C4
        private void SwitchPBlock16x16(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint r5, int Offset)
        {
            switch (r5)
            {
                case 0:
                    {
                        loc_1147B0(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, (int)Internal[219], (int)Internal[220], Offset);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                case 1:
                    {
                        sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, Offset);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                case 2:
                    {
                        sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 8, 0x10, Offset);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                case 3:
                    {
                        sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 0x10, Offset);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                case 4:
                    {
                        sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 0x10, Offset);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                case 5:
                    {
                        sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 0x10, Offset);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                case 6:
                    {
                        DecIntraFullBlockPMode(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                case 7:
                    {
                        DecIntraSubBlockPMode(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                //|__|
                //|  |
                case 8:
                    {
                        ReadPBlock16x8(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock16x8(ref nrBitsRemaining, ref r3, internaloffset, Offset + Stride * 8);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                //| | |
                //| | |
                case 9:
                    {
                        ReadPBlock8x16(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock8x16(ref nrBitsRemaining, ref r3, internaloffset, Offset + 8);
                        loc_1161A0(ref nrBitsRemaining, ref r3, Offset);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_1148EC =
        {
            0x01, 0x03, 0x04, 0x05, 0x06, 0x06, 0x05, 0x05, 0x03, 0x04
        };

        private byte[] PBlock16x16HuffmanTableModsDS =
        {
           1, 1, 1, 1, 1, 1, 1, 1, 8, 8, 8, 8, 9, 9, 9, 9, 4,
            3, 2, 2, 7, 7, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private byte[] PBlock16x16BitCountTableModsDS =
        {
           2, 2, 4, 5, 5, 5, 5, 4, 3, 3
        };

        //sub_1149D0
        private void ReadPBlock16x16(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock16x16HuffmanTableModsDS[r3 >> 27];
            uint r6 = PBlock16x16BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0)
                FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock16x16(ref nrBitsRemaining, ref r3, internaloffset, r5, Offset);
        }

        private void sub_114A44(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int Offset)
        {
            int dx = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            int dy = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            dx += (int)Internal[219];
            dy += (int)Internal[220];
            loc_114A64(ref nrBitsRemaining, ref r3, internaloffset, srcFrame, height, dx, dy, Offset);
        }

        private void loc_114A64(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int dx, int dy, int Offset)
        {
            Internal[internaloffset] = (uint)dx;
            Internal[internaloffset + 1] = (uint)dy;
            CopyBlock(Y[srcFrame / 4], dx, dy, 8, height, Y[0], Offset);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 4, height >> 1, UV[0], Offset / 2);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 4, height >> 1, UV[0], Offset / 2 + Stride / 2);
            // CopyBlock8(((dx + Offset * 2) & 7) + ((dy & 1) << 3), Y[srcFrame / 4], Offset + ((dy >> 1) * Stride) + (dx >> 1), Y[0], Offset, height);
            // CopyBlock4((((dx + Offset * 2) >> 1) & 7) + (((dy >> 1) & 1) << 3), UV[srcFrame / 4], Offset / 2 + ((dy >> 2) * Stride) + (dx >> 2), UV[0], Offset / 2, height >> 1);
            //CopyBlock4((((dx + Offset * 2) >> 1) & 7) + (((dy >> 1) & 1) << 3), UV[srcFrame / 4], Offset / 2 + ((dy >> 2) * Stride) + (dx >> 2) + Stride / 2, UV[0], Offset / 2 + Stride / 2, height >> 1);     
        }

        private byte[] PBlock8x16HuffmanTableMoflex3DS =
        {
            0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x02, 0x02, 0x02, 0x02,
            0x03, 0x03, 0x05, 0x04, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x08, 0x08, 0x08, 0x08, 0x00, 0x00, 0x00, 0x00
        };

        //Switch_114B58
        private void SwitchPBlock8x16(uint Idx, ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114A64(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, Offset); break;
                case 2: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 8, 0x10, Offset); break;
                case 3: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 0x10, Offset); break;
                case 4: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 0x10, Offset); break;
                case 5: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 0x10, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock8x8(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock8x8(ref nrBitsRemaining, ref r3, internaloffset, Offset + Stride * 8);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock4x16(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock4x16(ref nrBitsRemaining, ref r3, internaloffset, Offset + 4);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] PBlock8x16BitCountTableMoflex3DS =
        {
            3,2,3,4,5,5,0,0,3,2
        };

        private byte[] PBlock8x16HuffmanTableModsDS =
        {
            0, 0, 0, 0, 9, 9, 5, 4, 2, 2, 3, 8, 1, 1, 1, 1
        };

        private byte[] PBlock8x16BitCountTableModsDS =
        {
            2, 2, 3, 4, 4, 4, 0, 0, 4, 3
        };

        //sub_114C38
        private void ReadPBlock8x16(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock8x16HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock8x16BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock8x16(r5, ref nrBitsRemaining, ref r3, internaloffset, Offset);
        }



        private void sub_114C8C(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int Offset)
        {
            int dx = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            int dy = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            dx += (int)Internal[219];
            dy += (int)Internal[220];
            loc_114CAC(ref nrBitsRemaining, ref r3, internaloffset, srcFrame, height, dx, dy, Offset);
        }

        private void loc_114CAC(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int dx, int dy, int Offset)
        {
            Internal[internaloffset] = (uint)dx;
            Internal[internaloffset + 1] = (uint)dy;
            CopyBlock(Y[srcFrame / 4], dx, dy, 4, height, Y[0], Offset);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 2, height >> 1, UV[0], Offset / 2);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 2, height >> 1, UV[0], Offset / 2 + Stride / 2);
            // byte test1 = Y[0][Offset];
            // byte test2 = UV[0][Offset/2];
            // byte test3 = UV[0][Offset/2 + Stride / 2];
            //CopyBlock4(((dx + Offset * 2) & 7) + ((dy & 1) << 3), Y[srcFrame / 4], Offset + ((dy >> 1) * Stride) + (dx >> 1), Y[0], Offset, height);
            //CopyBlock2((((dx + Offset * 2) >> 1) & 3) + (((dy >> 1) & 1) << 2), UV[srcFrame / 4], Offset / 2 + ((dy >> 2) * Stride) + (dx >> 2), UV[0], Offset / 2, height >> 1);
            //CopyBlock2((((dx + Offset * 2) >> 1) & 3) + (((dy >> 1) & 1) << 2), UV[srcFrame / 4], Offset / 2 + ((dy >> 2) * Stride) + (dx >> 2) + Stride / 2, UV[0], Offset / 2 + Stride / 2, height >> 1);
            // if (Y[0][Offset] != test1 || UV[0][Offset / 2] != test2 || UV[0][Offset / 2 + Stride / 2] != test3)
            //  {

            //  }
        }

        private byte[] byte_114D80 =
        {
            0,0,0,0,5,4,2,2,9,9,3,8,1,1,1,1
        };

        //Switch_114D90
        private void SwitchPBlock4x16(uint Idx, ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114CAC(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, Offset); break;
                case 2: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 8, 0x10, Offset); break;
                case 3: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 0x10, Offset); break;
                case 4: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 0x10, Offset); break;
                case 5: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 0x10, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock4x8(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock4x8(ref nrBitsRemaining, ref r3, internaloffset, Offset + Stride * 8);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock2x16(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock2x16(ref nrBitsRemaining, ref r3, internaloffset, Offset + 2);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_114DB8 =
        {
            2,2,3,4,4,4,0,0,4,3
        };

        private byte[] PBlock4x16HuffmanTableModsDS =
        {
             3, 3, 9, 5, 0, 0, 0, 0, 4, 8, 2, 2, 1, 1, 1, 1
        };

        private byte[] PBlock4x16BitCountTableModsDS =
        {
            2, 2, 3, 3, 4, 4, 0, 0, 4, 4
        };

        //sub_114E70
        private void ReadPBlock4x16(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock4x16HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock4x16BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock4x16(r5, ref nrBitsRemaining, ref r3, internaloffset, Offset);
        }

        private void sub_114EB4(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int Offset)
        {
            int dx = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            int dy = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            dx += (int)Internal[219];
            dy += (int)Internal[220];
            loc_114ED4(ref nrBitsRemaining, ref r3, internaloffset, srcFrame, height, dx, dy, Offset);
        }

        private void loc_114ED4(ref int nrBitsRemaining, ref uint r3, int internaloffset, uint srcFrame, uint height, int dx, int dy, int Offset)
        {
            Internal[internaloffset] = (uint)dx;
            Internal[internaloffset + 1] = (uint)dy;
            CopyBlock(Y[srcFrame / 4], dx, dy, 2, height, Y[0], Offset);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 1, height >> 1, UV[0], Offset / 2);
            CopyBlock(UV[srcFrame / 4], dx >> 1, dy >> 1, 1, height >> 1, UV[0], Offset / 2 + Stride / 2);
            //CopyBlock2(((dx + Offset * 2) & 3) + ((dy & 1) << 2), Y[srcFrame / 4], Offset + ((dy >> 1) * Stride) + (dx >> 1), Y[0], Offset, height);
            //CopyBlock1((((dx + Offset * 2) >> 1) & 1) + (((dy >> 1) & 1) << 1), UV[srcFrame / 4], Offset / 2 + ((dy >> 2) * Stride) + (dx >> 2), UV[0], Offset / 2, height >> 1);
            //CopyBlock1((((dx + Offset * 2) >> 1) & 1) + (((dy >> 1) & 1) << 1), UV[srcFrame / 4], Offset / 2 + ((dy >> 2) * Stride) + (dx >> 2) + Stride / 2, UV[0], Offset / 2 + Stride / 2, height >> 1);
        }

        private byte[] byte_114FA8 =
        {
            1,1,1,1,1,1,1,1,5,4,2,2,8,3,0,0
        };

        //Switch_114FB8
        private void SwitchPBlock2x16(uint Idx, ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114ED4(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 4, 0x10, Offset); break;
                case 2: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 8, 0x10, Offset); break;
                case 3: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 0x10, Offset); break;
                case 4: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 0x10, Offset); break;
                case 5: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 0x10, Offset); break;
                case 6:
                case 7:
                case 9:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock2x8(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock2x8(ref nrBitsRemaining, ref r3, internaloffset, Offset + 8 * Stride);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_114FE0 =
        {
            3,1,3,4,4,4,0,0,4
        };

        private byte[] PBlock2x16HuffmanTableModsDS =
        {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 3,
            3, 3, 3, 4, 4, 8, 5, 2, 2, 2, 2, 0, 0, 0, 0
        };

        private byte[] PBlock2x16BitCountTableModsDS =
        {
            3, 1, 3, 3, 4, 5, 0, 0, 5
        };

        //sub_115080
        private void ReadPBlock2x16(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock2x16HuffmanTableModsDS[r3 >> 27];
            uint r6 = PBlock2x16BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock2x16(r5, ref nrBitsRemaining, ref r3, internaloffset, Offset);
        }

        private byte[] byte_1150B4 =
        {
            2, 2, 2, 2, 9, 9, 9, 9,
            8, 8, 8, 8, 8, 8, 8, 8,
            3, 3, 5, 4, 0, 0, 0, 0,
            1, 1, 1, 1, 1, 1, 1, 1
        };

        //Switch_1150D4
        private void SwitchPBlock16x8(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_1147B0(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, Offset); break;
                case 2: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 8, 8, Offset); break;
                case 3: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 8, Offset); break;
                case 4: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 8, Offset); break;
                case 5: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 8, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock16x4(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock16x4(ref nrBitsRemaining, ref r3, internaloffset, Offset + Stride * 4);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock8x8(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock8x8(ref nrBitsRemaining, ref r3, internaloffset, Offset + 8);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_1150FC =
        {
            3,2,3,4,5,5,0,0,2,3,0,0
        };

        private byte[] PBlock16x8HuffmanTableModsDS =
        {
            0, 0, 0, 0, 5, 4, 8, 8, 2, 2, 3, 9, 1, 1, 1, 1
        };

        private byte[] PBlock16x8BitCountTableModsDS =
        {
            2, 2, 3, 4, 4, 4, 0, 0, 3, 4
        };

        //sub_1151B4
        private void ReadPBlock16x8(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock16x8HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock16x8BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock16x8(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_1151E8 =
        {
            0, 0, 0, 0, 5, 4, 2, 2, 8, 8, 3, 9, 1, 1, 1, 1
        };

        //Switch_1151F8
        private void SwitchPBlock16x4(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_1147B0(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, Offset); break;
                case 2: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 8, 4, Offset); break;
                case 3: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 4, Offset); break;
                case 4: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 4, Offset); break;
                case 5: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 4, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock16x2(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock16x2(ref nrBitsRemaining, ref r3, internaloffset, Offset + 2 * Stride);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock8x4(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock8x4(ref nrBitsRemaining, ref r3, internaloffset, Offset + 8);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_115220 =
        {
            2,2,3,4,4,4,0,0,3,4
        };

        private byte[] PBlock16x4HuffmanTableModsDS =
        {
             0, 0, 0, 0, 3, 3, 8, 4, 2, 2, 5, 9, 1, 1, 1, 1
        };

        private byte[] PBlock16x4BitCountTableModsDS =
        {
            2, 2, 3, 3, 4, 4, 0, 0, 4, 4
        };

        //sub_1152D8
        private void ReadPBlock16x4(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock16x4HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock16x4BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock16x4(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_11530C =
        {
            1,1,1,1,1,1,1,1,5,4,2,2,0,0,9,3
        };

        //Switch_11531C
        private void SwitchPBlock16x2(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_1147B0(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, Offset); break;
                case 2: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 8, 2, Offset); break;
                case 3: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 2, Offset); break;
                case 4: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 2, Offset); break;
                case 5: sub_114790(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 2, Offset); break;
                case 6:
                case 7:
                case 8:
                    //error?
                    throw new Exception();
                case 9:
                    {
                        ReadPBlock8x2(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock8x2(ref nrBitsRemaining, ref r3, internaloffset, Offset + 8);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_115344 =
        {
            3,1,3,4,4,4,0,0,0,4
        };

        private byte[] PBlock16x2HuffmanTableModsDS =
        {
            1, 1, 1, 1, 1, 1, 1, 1, 9, 4, 2, 2, 0, 0, 5, 3
        };

        private byte[] PBlock16x2BitCountTableModsDS =
        {
             3, 1, 3, 4, 4, 4, 0, 0, 0, 4
        };

        //sub_1153E4
        private void ReadPBlock16x2(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock16x2HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock16x2BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock16x2(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_115418 =
        {
            3,3,5,4,2,2,9,9,8,8,0,0,1,1,1,1
        };

        //Switch_115428
        private void SwitchPBlock8x8(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114A64(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, Offset); break;
                case 2: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 8, 8, Offset); break;
                case 3: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 8, Offset); break;
                case 4: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 8, Offset); break;
                case 5: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 8, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock8x4(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock8x4(ref nrBitsRemaining, ref r3, internaloffset, Offset + 4 * Stride);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock4x8(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock4x8(ref nrBitsRemaining, ref r3, internaloffset, Offset + 4);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_115450 =
        {
            3,2,3,3,4,4,0,0,3,3
        };

        private byte[] PBlock8x8HuffmanTableModsDS =
        {
            0, 0, 0, 0, 3, 3, 5, 9, 4, 8, 2, 2, 1, 1, 1, 1
        };

        private byte[] PBlock8x8BitCountTableModsDS =
        {
            2, 2, 3, 3, 4, 4, 0, 0, 4, 4
        };

        //sub_115508
        private void ReadPBlock8x8(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock8x8HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock8x8BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock8x8(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_11553C =
        {
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,0,0,0,0,9,9,8,8,3,3,5,4
        };

        //Switch_11555C
        private void SwitchPBlock8x4(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114A64(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, Offset); break;
                case 2: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 8, 4, Offset); break;
                case 3: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 4, Offset); break;
                case 4: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 4, Offset); break;
                case 5: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 4, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock8x2(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock8x2(ref nrBitsRemaining, ref r3, internaloffset, Offset + Stride * 2);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock4x4(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock4x4(ref nrBitsRemaining, ref r3, internaloffset, Offset + 4);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_115584 =
        {
            3,1,3,4,5,5,0,0,4,4
        };

        private byte[] PBlock8x4HuffmanTableModsDS =
        {
            2, 2, 2, 2, 8, 9, 3, 3, 5, 4, 0, 0, 1, 1, 1, 1
        };

        private byte[] PBlock8x4BitCountTableModsDS =
        {
             3, 2, 2, 3, 4, 4, 0, 0, 4, 4
        };

        //sub_11563C
        private void ReadPBlock8x4(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock8x4HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock8x4BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock8x4(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_115670 =
        {
            1,1,1,1,1,1,1,1,9,5,2,2,0,0,4,3
        };

        //Switch_115680
        private void SwitchPBlock8x2(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114A64(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, Offset); break;
                case 2: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 8, 2, Offset); break;
                case 3: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 2, Offset); break;
                case 4: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 2, Offset); break;
                case 5: sub_114A44(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 2, Offset); break;
                case 6:
                case 7:
                case 8:
                    //error?
                    throw new Exception();
                case 9:
                    {
                        ReadPBlock4x2(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock4x2(ref nrBitsRemaining, ref r3, internaloffset, Offset + 4);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_1156A8 =
        {
            3,1,3,4,4,4,0,0,0,4
        };

        private byte[] PBlock8x2HuffmanTableModsDS =
        {
            2, 2, 2, 2, 4, 4, 9, 5, 3, 3, 0, 0, 1, 1, 1, 1
        };

        private byte[] PBlock8x2BitCountTableModsDS =
        {
             3, 2, 2, 3, 3, 4, 0, 0, 0, 4
        };

        //sub_115748
        private void ReadPBlock8x2(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock8x2HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock8x2BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock8x2(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_11577C =
        {
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,0,0,0,0,9,9,8,8,3,3,5,4
        };

        //Switch_11579C
        private void SwitchPBlock4x8(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114CAC(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, Offset); break;
                case 2: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 8, 8, Offset); break;
                case 3: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 8, Offset); break;
                case 4: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 8, Offset); break;
                case 5: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 8, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock4x4(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock4x4(ref nrBitsRemaining, ref r3, internaloffset, Offset + 4 * Stride);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock2x8(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock2x8(ref nrBitsRemaining, ref r3, internaloffset, Offset + 2);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_1157C4 =
        {
            3,1,3,4,5,5,0,0,4,4
        };

        private byte[] PBlock4x8HuffmanTableModsDS =
        {
            0, 0, 0, 0, 3, 3, 9, 5, 8, 4, 2, 2, 1, 1, 1, 1
        };

        private byte[] PBlock4x8BitCountTableModsDS =
        {
             2, 2, 3, 3, 4, 4, 0, 0, 4, 4
        };

        //sub_11587C
        private void ReadPBlock4x8(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock4x8HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock4x8BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock4x8(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_1158B0 =
        {
            0, 0, 0, 0, 3,3,9,8,5,4,2,2,1,1,1,1
        };

        //Switch_1158C0
        private void SwitchPBlock4x4(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114CAC(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, Offset); break;
                case 2: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 8, 4, Offset); break;
                case 3: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 4, Offset); break;
                case 4: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 4, Offset); break;
                case 5: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 4, Offset); break;
                case 6:
                case 7:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock4x2(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock4x2(ref nrBitsRemaining, ref r3, internaloffset, Offset + 2 * Stride);
                        break;
                    }
                case 9:
                    {
                        ReadPBlock2x4(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock2x4(ref nrBitsRemaining, ref r3, internaloffset, Offset + 2);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_1158E8 =
        {
            2,2,3,3,4,4,0,0,4,4
        };

        private byte[] PBlock4x4HuffmanTableModsDS =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 4, 4, 3, 3, 3, 3, 8,
            9, 5, 5, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1
        };

        private byte[] PBlock4x4BitCountTableModsDS =
        {
            2, 2, 3, 3, 3, 4, 0, 0, 5, 5
        };

        //sub_1159A0
        private void ReadPBlock4x4(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock4x4HuffmanTableModsDS[r3 >> 27];
            uint r6 = PBlock4x4BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock4x4(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_1159D4 =
        {
            0,0,0,0,5,5,3,3,9,4,2,2,1,1,1,1
        };

        //Switch_1159E4
        private void SwitchPBlock4x2(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114CAC(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, Offset); break;
                case 2: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 8, 2, Offset); break;
                case 3: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 2, Offset); break;
                case 4: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 2, Offset); break;
                case 5: sub_114C8C(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 2, Offset); break;
                case 6:
                case 7:
                case 8:
                    //error?
                    throw new Exception();
                case 9:
                    {
                        ReadPBlock2x2(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock2x2(ref nrBitsRemaining, ref r3, internaloffset, Offset + 2);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_115A0C =
        {
            2,2,3,3,4,3,0,0,0,4
        };

        private byte[] PBlock4x2HuffmanTableModsDS =
        {
             0, 0, 0, 0, 4, 4, 9, 5, 3, 3, 2, 2, 1, 1, 1, 1
        };

        private byte[] PBlock4x2BitCountTableModsDS =
        {
             2, 2, 3, 3, 3, 4, 0, 0, 0, 4
        };

        //sub_115AAC
        private void ReadPBlock4x2(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock4x2HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock4x2BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock4x2(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_115AE0 =
        {
            1,1,1,1,1,1,1,1,8,5,2,2,0,0,4,3
        };

        //Switch_115AF0
        private void SwitchPBlock2x8(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114ED4(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 4, 8, Offset); break;
                case 2: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 8, 8, Offset); break;
                case 3: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 8, Offset); break;
                case 4: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 8, Offset); break;
                case 5: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 8, Offset); break;
                case 6:
                case 7:
                case 9:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock2x4(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock2x4(ref nrBitsRemaining, ref r3, internaloffset, Offset + 4 * Stride);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_115B18 =
        {
            3,1,3,4,4,4,0,0,4
        };

        private byte[] PBlock2x8HuffmanTableModsDS =
        {
             0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 3,
             3, 3, 3, 4, 4, 8, 5, 1, 1, 1, 1, 1, 1, 1, 1
        };

        private byte[] PBlock2x8BitCountTableModsDS =
        {
            2, 2, 2, 3, 4, 5, 0, 0, 5
        };

        //sub_115BB8
        private void ReadPBlock2x8(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock2x8HuffmanTableModsDS[r3 >> 27];
            uint r6 = PBlock2x8BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock2x8(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_115BEC =
        {
            0,0,0,0,4,4,3,3,8,5,2,2,1,1,1,1
        };

        //Switch_115BFC
        private void SwitchPBlock2x4(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114ED4(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 4, 4, Offset); break;
                case 2: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 8, 4, Offset); break;
                case 3: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 4, Offset); break;
                case 4: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 4, Offset); break;
                case 5: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 4, Offset); break;
                case 6:
                case 7:
                case 9:
                    //error?
                    throw new Exception();
                case 8:
                    {
                        ReadPBlock2x2(ref nrBitsRemaining, ref r3, internaloffset, Offset);
                        ReadPBlock2x2(ref nrBitsRemaining, ref r3, internaloffset, Offset + Stride * 2);
                        break;
                    }
                default:
                    break;
            }
        }

        private byte[] byte_115C24 =
        {
            2,2,3,3,3,4,0,0,4,0
        };

        private byte[] PBlock2x4HuffmanTableModsDS =
        {
            0, 0, 0, 0, 4, 4, 8, 5, 3, 3, 2, 2, 1, 1, 1, 1
        };

        private byte[] PBlock2x4BitCountTableModsDS =
        {
             2, 2, 3, 3, 3, 4, 0, 0, 4
        };

        //sub_115CC4
        private void ReadPBlock2x4(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock2x4HuffmanTableModsDS[r3 >> 28];
            uint r6 = PBlock2x4BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock2x4(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_115CF8 =
        {
            0, 0, 4, 5, 3, 2, 1, 1
        };

        //Switch_115D00
        private void SwitchPBlock2x2(uint Idx, ref int nrBitsRemaining, int internaloffset, ref uint r3, int Offset)
        {
            switch (Idx)
            {
                case 0: loc_114ED4(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, (int)Internal[219], (int)Internal[220], Offset); break;
                case 1: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 4, 2, Offset); break;
                case 2: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 8, 2, Offset); break;
                case 3: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0xC, 2, Offset); break;
                case 4: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x10, 2, Offset); break;
                case 5: sub_114EB4(ref nrBitsRemaining, ref r3, internaloffset, 0x14, 2, Offset); break;
                case 6:
                case 7:
                case 8:
                case 9:
                    //error?
                    throw new Exception();
                default:
                    break;
            }
        }

        private byte[] byte_115D28 =
        {
            2,2,3,3,3,3
        };

        private byte[] PBlock2x2HuffmanTableModsDS =
        {
             5, 4, 1, 1, 0, 0, 3, 2
        };

        private byte[] PBlock2x2BitCountTableModsDS =
        {
             2, 2, 3, 3, 3, 3
        };

        //sub_115DB0
        private void ReadPBlock2x2(ref int nrBitsRemaining, ref uint r3, int internaloffset, int Offset)
        {
            uint r5 = PBlock2x2HuffmanTableModsDS[r3 >> 29];
            uint r6 = PBlock2x2BitCountTableModsDS[r5];
            r3 <<= (int)r6;
            nrBitsRemaining -= (int)r6;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            SwitchPBlock2x2(r5, ref nrBitsRemaining, internaloffset, ref r3, Offset);
        }

        private byte[] byte_115FC4 =
        {
            0x00, 0x1F, 0x3F, 0x0F, 0x08, 0x04, 0x02, 0x01, 0x0B, 0x0E, 0x1B, 0x0D,
            0x03, 0x07, 0x0C, 0x17, 0x1D, 0x0A, 0x1E, 0x05, 0x10, 0x2F, 0x37, 0x3B,
            0x13, 0x3D, 0x3E, 0x09, 0x1C, 0x06, 0x15, 0x1A, 0x33, 0x11, 0x12, 0x14,
            0x18, 0x20, 0x3C, 0x35, 0x19, 0x16, 0x3A, 0x30, 0x31, 0x32, 0x27, 0x34,
            0x2B, 0x2D, 0x39, 0x38, 0x23, 0x36, 0x2E, 0x21, 0x25, 0x22, 0x24, 0x2C,
            0x2A, 0x28, 0x29, 0x26
        };

        //sub_116004
        private void DecIntraFullBlockPMode(ref int nrBitsRemaining, ref uint r3, int Offset)
        {
            uint r4 = byte_115FC4[ReadVarIntUnsigned(ref nrBitsRemaining, ref r3)];
            uint r12 = r3 >> 29;
            r3 <<= 3;
            nrBitsRemaining -= 3;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            if (r12 == 2)
            {
                r12 = 9;
                sub_1167BC(Y[0], Offset, ref nrBitsRemaining, ref r3);
            }
            if ((r4 & 1) == 1) sub_116508(ref nrBitsRemaining, ref r3, Y[0], Offset, r12);
            else PredictIntra(ref nrBitsRemaining, ref r3, r12, Y[0], Offset);
            Offset += 8;
            if (((r4 >> 1) & 1) == 1) sub_116508(ref nrBitsRemaining, ref r3, Y[0], Offset, r12);
            else PredictIntra(ref nrBitsRemaining, ref r3, r12, Y[0], Offset);
            Offset += Stride * 8;//0x800;
            Offset -= 8;
            if (((r4 >> 2) & 1) == 1) sub_116508(ref nrBitsRemaining, ref r3, Y[0], Offset, r12);
            else PredictIntra(ref nrBitsRemaining, ref r3, r12, Y[0], Offset);
            Offset += 8;
            if (((r4 >> 3) & 1) == 1) sub_116508(ref nrBitsRemaining, ref r3, Y[0], Offset, r12);
            else PredictIntra(ref nrBitsRemaining, ref r3, r12, Y[0], Offset);
            Offset -= Stride * 8;//0x800;
            Offset -= 8;
            loc_116290(ref nrBitsRemaining, ref r3, r4, Offset);
        }

        //sub_1160C8
        private void DecIntraSubBlockPMode(ref int nrBitsRemaining, ref uint r3, int Offset)
        {
            uint r4 = byte_115FC4[ReadVarIntUnsigned(ref nrBitsRemaining, ref r3)];
            if ((r4 & 1) == 0) loc_116220(ref nrBitsRemaining, ref r3, 9, Y[0], Offset);
            else loc_116368(ref nrBitsRemaining, ref r3, 9, Y[0], Offset);
            Offset += 8;
            if (((r4 >> 1) & 1) == 0) loc_116220(ref nrBitsRemaining, ref r3, 0xB, Y[0], Offset);
            else loc_116368(ref nrBitsRemaining, ref r3, 0xB, Y[0], Offset);
            Offset += Stride * 8; //0x800;
            Offset -= 8;
            if (((r4 >> 2) & 1) == 0) loc_116220(ref nrBitsRemaining, ref r3, 0x19, Y[0], Offset);
            else loc_116368(ref nrBitsRemaining, ref r3, 0x19, Y[0], Offset);
            Offset += 8;
            if (((r4 >> 3) & 1) == 0) loc_116220(ref nrBitsRemaining, ref r3, 0x1B, Y[0], Offset);
            else loc_116368(ref nrBitsRemaining, ref r3, 0x1B, Y[0], Offset);
            Offset -= Stride * 8; //0x800;
            Offset -= 8;
            loc_116290(ref nrBitsRemaining, ref r3, r4, Offset);
        }

        private byte[] byte_116160 = {
            0x00, 0x0F, 0x04, 0x01, 0x08, 0x02, 0x0C, 0x03, 0x05, 0x0A, 0x0D, 0x07,
            0x0E, 0x0B, 0x1F, 0x09, 0x06, 0x10, 0x3F, 0x1E, 0x17, 0x1D, 0x1B, 0x1C,
            0x13, 0x18, 0x1A, 0x12, 0x11, 0x14, 0x15, 0x20, 0x2F, 0x16, 0x19, 0x37,
            0x3D, 0x3E, 0x3B, 0x3C, 0x33, 0x35, 0x21, 0x24, 0x22, 0x28, 0x23, 0x2C,
            0x30, 0x27, 0x2D, 0x25, 0x3A, 0x2B, 0x2E, 0x2A, 0x31, 0x34, 0x38, 0x32,
            0x29, 0x26, 0x39, 0x36
        };

        private void loc_1161A0(ref int nrBitsRemaining, ref uint r3, int Offset)
        {
            uint r12 = byte_116160[ReadVarIntUnsigned(ref nrBitsRemaining, ref r3)];
            if ((r12 & 1) != 0) loc_11652C(ref nrBitsRemaining, ref r3, Y[0], Offset);
            Offset += 8;
            if (((r12 >> 1) & 1) != 0) loc_11652C(ref nrBitsRemaining, ref r3, Y[0], Offset);
            Offset += Stride * 8;
            Offset -= 8;
            if (((r12 >> 2) & 1) != 0) loc_11652C(ref nrBitsRemaining, ref r3, Y[0], Offset);
            Offset += 8;
            if (((r12 >> 3) & 1) != 0) loc_11652C(ref nrBitsRemaining, ref r3, Y[0], Offset);
            Offset -= Stride * 8;
            Offset -= 8;
            if (((r12 >> 4) & 1) != 0) loc_11652C(ref nrBitsRemaining, ref r3, UV[0], Offset / 2);
            if (((r12 >> 5) & 1) != 0) loc_11652C(ref nrBitsRemaining, ref r3, UV[0], Offset / 2 + Stride / 2);
        }

        private void loc_116220(ref int nrBitsRemaining, ref uint r3, int r5, byte[] Dst, int Offset)
        {
            fixed (uint* InternalPtr = &Internal[0])
            {
                byte* InternalByte = (byte*)InternalPtr;
                uint r12 = InternalByte[r5 - 8];
                uint r6 = InternalByte[r5 - 1];
                if (r12 > r6) r12 = r6;
                if (r12 == 9) r12 = 3;
                r6 = r3 >> 28;
                if (r6 >= r12) r6++;
                int r7;
                if (r6 < 9)
                {
                    r12 = r6;
                    r7 = 4;
                }
                else r7 = 1;
                InternalByte[r5] = (byte)r12;
                InternalByte[r5 + 1] = (byte)r12;
                InternalByte[r5 + 8] = (byte)r12;
                InternalByte[r5 + 9] = (byte)r12;
                r3 <<= r7;
                nrBitsRemaining -= r7;
                if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
            }
        }

        private void loc_116290(ref int nrBitsRemaining, ref uint r3, uint r4, int Offset)
        {
            uint r12 = r3 >> 29;
            r3 <<= 3;
            nrBitsRemaining -= 3;
            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            if (r12 == 2)
            {
                r12 = 9;
                sub_116CCC(ref nrBitsRemaining, ref r3, UV[0], Offset / 2);
                sub_116CCC(ref nrBitsRemaining, ref r3, UV[0], Offset / 2 + (Stride / 2));
            }
            if (((r4 >> 4) & 1) == 1) sub_116508(ref nrBitsRemaining, ref r3, UV[0], Offset / 2, r12);
            else PredictIntra(ref nrBitsRemaining, ref r3, r12, UV[0], Offset / 2);
            if (((r4 >> 5) & 1) == 1) sub_116508(ref nrBitsRemaining, ref r3, UV[0], Offset / 2 + (Stride / 2), r12);
            else PredictIntra(ref nrBitsRemaining, ref r3, r12, UV[0], Offset / 2 + (Stride / 2));
        }

        //SwitchDword_116318
        private void PredictIntra(ref int nrBitsRemaining, ref uint r3, uint r12, byte[] Dst, int Offset)
        {
            bool VOffsetfix = false;
            if (Dst == UV[0] && (Offset % Stride) >= Stride / 2)
                VOffsetfix = true;
            switch (r12)
            {
                case 0://00116BB8
                    {
                        byte[] vals = new byte[8];
                        Array.Copy(Dst, Offset - Stride, vals, 0, 8);
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                Dst[Offset + i * Stride + j] = vals[j];
                            }
                        }
                        break;
                    }
                case 1://00116C04
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            byte r6 = Dst[Offset + i * Stride - 1];
                            for (int j = 0; j < 8; j++)
                            {
                                Dst[Offset + i * Stride + j] = r6;
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        sub_116CCC(ref nrBitsRemaining, ref r3, Dst, Offset);
                        break;
                    }
                case 3://00117148
                    {
                        int r8 = 0;
                        if (((Offset - (VOffsetfix ? (Stride / 2) : 0)) % Stride) != 0) r8 += 8;
                        if (Offset >= Stride) r8 += 4;
                        switch (r8 / 4)
                        {
                            case 0://001170E4
                                {
                                    for (int i = 0; i < 8; i++)
                                    {
                                        Dst[Offset + i * Stride] = 0x80;
                                        Dst[Offset + i * Stride + 1] = 0x80;
                                        Dst[Offset + i * Stride + 2] = 0x80;
                                        Dst[Offset + i * Stride + 3] = 0x80;
                                        Dst[Offset + i * Stride + 4] = 0x80;
                                        Dst[Offset + i * Stride + 5] = 0x80;
                                        Dst[Offset + i * Stride + 6] = 0x80;
                                        Dst[Offset + i * Stride + 7] = 0x80;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    uint r6 = 0;
                                    uint r4 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                                    uint r5 = IOUtil.ReadU32LE(Dst, Offset - Stride + 4);
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r6 += r5 >> 24;
                                    r5 <<= 8;
                                    r6 += r5 >> 24;
                                    r5 <<= 8;
                                    r6 += r5 >> 24;
                                    r5 <<= 8;
                                    r6 += r5 >> 24;
                                    r6 += 4;
                                    r6 /= 8;
                                    for (int i = 0; i < 8; i++)
                                    {
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride + 4, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    uint r6 = 0;
                                    for (int i = 0; i < 8; i++)
                                    {
                                        r6 += Dst[Offset + i * Stride - 1];
                                    }
                                    r6 += 4;
                                    r6 /= 8;
                                    for (int i = 0; i < 8; i++)
                                    {
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride + 4, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                    }
                                    break;
                                }
                            case 3://00116EC0
                                {
                                    uint r6 = 0;
                                    uint r4 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                                    uint r5 = IOUtil.ReadU32LE(Dst, Offset - Stride + 4);
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r6 += r5 >> 24;
                                    r5 <<= 8;
                                    r6 += r5 >> 24;
                                    r5 <<= 8;
                                    r6 += r5 >> 24;
                                    r5 <<= 8;
                                    r6 += r5 >> 24;
                                    for (int i = 0; i < 8; i++)
                                    {
                                        r6 += Dst[Offset + i * Stride - 1];
                                    }
                                    r6 += 8;
                                    r6 /= 16;
                                    for (int i = 0; i < 8; i++)
                                    {
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride + 4, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 4://11716C
                    {
                        uint r11_i = (uint)Offset;
                        uint r3_i = Dst[r11_i - 1];
                        r11_i -= 1;
                        uint r12_i = Dst[r11_i + Stride];
                        r11_i += (uint)Stride;
                        uint r9_i = Dst[r11_i + Stride];
                        r11_i += (uint)Stride;
                        uint r6_i = Dst[r11_i + Stride];
                        r11_i += (uint)Stride;
                        uint r8_i = ((r3_i + r12_i) + 1) / 2;
                        uint lr_i = ((r9_i + r3_i + r12_i * 2) + 2) / 4;
                        r3_i = ((r12_i + r9_i) + 1) / 2;
                        r12_i = ((r6_i + r12_i + r9_i * 2) + 2) / 4;
                        r8_i |= (lr_i << 8) | (r3_i << 16) | (r12_i << 24);
                        lr_i = r11_i - (uint)Stride * 3;
                        IOUtil.WriteU32LE(Dst, (int)lr_i + 1, r8_i);
                        lr_i = ((r9_i + r6_i) + 1) / 2;
                        r8_i = Dst[r11_i + Stride];
                        r11_i += (uint)Stride;
                        r3_i |= (r12_i << 8) | (lr_i << 16);
                        uint r4_i = ((r8_i + r9_i + r6_i * 2) + 2) / 4;
                        uint r5_i = ((r6_i + r8_i) + 1) / 2;
                        r9_i = Dst[r11_i + Stride];
                        r11_i += (uint)Stride;
                        r3_i |= r4_i << 24;
                        uint r7_i = r11_i - (4 * (uint)Stride);
                        IOUtil.WriteU32LE(Dst, (int)r7_i + 1, r3_i);
                        r6_i = ((r9_i + r6_i + r8_i * 2) + 2) / 4;
                        r7_i = lr_i | (r4_i << 8) | (r5_i << 16) | (r6_i << 24);
                        r12_i = r11_i - (5 * (uint)Stride);
                        IOUtil.WriteU32LE(Dst, (int)r12_i + 5, r7_i);
                        r12_i = ((r8_i + r9_i) + 1) / 2;
                        r3_i = Dst[r11_i + Stride];
                        r11_i += (uint)Stride;
                        lr_i = r5_i | (r6_i << 8) | (r12_i << 16);
                        r8_i = ((r3_i + r8_i + r9_i * 2) + 2) / 4;
                        lr_i |= r8_i << 24;
                        r5_i = r11_i - (uint)Stride * 4;
                        IOUtil.WriteU32LE(Dst, (int)r5_i + 1, r7_i);
                        IOUtil.WriteU32LE(Dst, (int)r5_i /*- 0xFB*/ - (Stride - 5), lr_i);
                        r5_i = ((r9_i + r3_i) + 1) / 2;
                        r4_i = Dst[r11_i + Stride];
                        r8_i = r12_i | (r8_i << 8) | (r5_i << 16);
                        r9_i = ((r4_i + r9_i + r3_i * 2) + 2) / 4;
                        r12_i = ((r3_i + r4_i) + 1) / 2;
                        r8_i |= r9_i << 24;
                        r3_i = ((r4_i + r3_i + r4_i * 2) + 2) / 4;
                        r9_i = r5_i | (r9_i << 8) | (r12_i << 16) | (r3_i << 24);
                        r7_i = r11_i - (uint)Stride * 2;
                        IOUtil.WriteU32LE(Dst, (int)r7_i /*- 0x1FB*/ - (Stride * 2 - 5), r8_i);
                        IOUtil.WriteU32LE(Dst, (int)r7_i /*- 0xFB*/ - (Stride - 5), r9_i);
                        IOUtil.WriteU32LE(Dst, (int)r7_i /*- 0xFF*/ - (Stride - 1), lr_i);
                        IOUtil.WriteU32LE(Dst, (int)r7_i + 1, r8_i);
                        r8_i = r12_i | (r3_i << 8) | (r4_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i /*- 0xFF*/ - (Stride - 1), r9_i);
                        r9_i = r4_i | (r4_i << 8) | (r4_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i /*- 0x1FB*/ - (Stride * 2 - 5), r8_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i /*- 0xFB*/ - (Stride - 5), r9_i);
                        r11_i++;
                        IOUtil.WriteU32LE(Dst, (int)r11_i, r8_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 4, r9_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride, r9_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride + 4, r9_i);
                        r11_i -= (uint)Stride * 6;
                        break;
                    }
                case 5:
                    {
                        uint v1; // r3@1
                        uint v2; // r12@1
                        uint v4; // r1@1
                        uint v5; // r2@1
                        uint v6; // lr@1
                        uint v7; // r5@1
                        uint v8; // r4@1
                        uint v9; // r9@1
                        uint v10; // r7@1
                        uint v11; // r6@1
                        uint v12; // r7@1
                        uint v13; // r11@1
                        uint v14; // r8@1
                        uint v15; // r9@1
                        uint v16; // r12@1
                        uint v17; // r2@1
                        uint v18; // r3@1
                        uint v19; // r4@1
                        uint v20; // lr@1
                        uint v21; // r1@1
                        uint v22; // r5@1
                        uint v23; // r2@1
                        uint v25; // r3@1
                        uint v26; // r12@1
                        uint v27; // r6@1
                        uint v28; // r1@1
                        uint v29; // r4@1
                        uint v30; // lr@1
                        uint v31; // r5@1
                        uint v32; // r12@1
                        uint v33; // r2@1
                        uint v34; // r3@1
                        uint v35; // r6@1
                        uint v36; // lr@1
                        uint v37; // r1@1
                        uint v38; // r4@1
                        uint v39; // r5@1
                        uint v40; // r1@1

                        v1 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                        v2 = IOUtil.ReadU32LE(Dst, Offset - Stride + 4);
                        v4 = Dst[Offset - 1];
                        v5 = Dst[Offset - Stride - 1];
                        v6 = (v4 + v5 + 1) >> 1;
                        v7 = ((byte)v1 + v4 + 2 * v5 + 2) >> 2;
                        v8 = v1 << 16 >> 24;
                        v9 = v1 << 8 >> 24;
                        v10 = (byte)v1 + v9 + 2 * v8;
                        v11 = (uint)(v5 + v8 + 2 * (byte)v1 + 2);
                        v1 >>= 24;
                        v11 >>= 2;
                        v12 = (v10 + 2) >> 2;
                        v13 = v6 | (v7 << 8) | (v11 << 16) | (v12 << 24);
                        v14 = (uint)((int)(v8 + v1 + 2 * v9 + 2) >> 2);
                        v15 = (uint)((int)(v9 + (byte)v2 + 2 * v1 + 2) >> 2);
                        IOUtil.WriteU32LE(Dst, Offset, v13);
                        IOUtil.WriteU32LE(Dst, Offset + 4, (uint)(v14 | (v15 << 8) | ((v1
                                                                             + ((uint)(v2 << 16) >> 24)
                                                                             + 2 * (byte)v2
                                                                             + 2) >> 2 << 16) | (((byte)v2
                                                                                                            + ((uint)(v2 << 8) >> 24)
                                                                                                            + 2
                                                                                                            * ((uint)(v2 << 16) >> 24)
                                                                                                            + 2) >> 2 << 24)));
                        v16 = Dst[Offset + Stride - 1];
                        v17 = (v5 + v16 + 2 * v4 + 2) >> 2;
                        v18 = (v16 + v4 + 1) >> 1;
                        v19 = v18 | (v17 << 8) | (v6 << 16) | (v7 << 24);
                        IOUtil.WriteU32LE(Dst, Offset + Stride, v19);
                        IOUtil.WriteU32LE(Dst, Offset + Stride + 4, v11 | (v12 << 8) | (v14 << 16) | (v15 << 24));
                        v20 = Dst[Offset + Stride * 2 - 1];
                        v21 = (v4 + v20 + 2 * v16 + 2) >> 2;
                        v22 = (v20 + v16 + 1) >> 1;
                        v23 = v22 | (v21 << 8) | (v18 << 16) | (v17 << 24);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2, v23);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2 + 4, v13);
                        v25 = Dst[Offset + Stride * 3 - 1];
                        v26 = (v16 + v25 + 2 * v20 + 2) >> 2;
                        v27 = (v25 + v20 + 1) >> 1;
                        v28 = v27 | (v26 << 8) | (v22 << 16) | (v21 << 24);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3, v28);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3 + 4, v19);
                        v29 = Dst[Offset + Stride * 4 - 1];
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 4 + 4, v23);
                        v30 = (v20 + v29 + 2 * v25 + 2) >> 2;
                        v31 = (v29 + v25 + 1) >> 1;
                        v32 = v31 | (v30 << 8) | (v27 << 16) | (v26 << 24);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 4, v32);
                        v33 = Dst[Offset + Stride * 5 - 1];
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 5 + 4, v28);
                        v34 = (v25 + v33 + 2 * v29 + 2) >> 2;
                        v35 = (v33 + v29 + 1) >> 1;
                        v36 = v35 | (v34 << 8) | (v31 << 16) | (v30 << 24);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 5, v36);
                        v37 = Dst[Offset + Stride * 6 - 1];
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 6 + 4, v32);
                        v38 = (v29 + v37 + 2 * v33 + 2) >> 2;
                        v39 = (v37 + v33 + 1) >> 1;
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 6, v39 | (v38 << 8) | (v35 << 16) | (v34 << 24));
                        v40 = ((Dst[Offset + Stride * 7 - 1] + v37 + 1) >> 1) | ((v33 + Dst[Offset + Stride * 7 - 1] + 2 * v37 + 2) >> 2 << 8) | (v39 << 16) | (v38 << 24);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 7, v40);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 7 + 4, v36);
                        break;
                    }
                case 6:
                    {
                        uint v1; // r2@1
                        uint v2; // r7@1
                        uint v3; // r9@1
                        uint v4; // ST30_4@1
                        uint v5; // r10@1
                        uint v6; // ST2C_4@1
                        uint v7; // ST28_4@1
                        uint v8; // r1@1
                        uint v9; // lr@1
                        uint v10; // r4@1
                        uint v12; // r3@1
                        uint v13; // r5@1
                        uint v14; // r12@1
                        uint v15; // r6@1
                        uint v16; // ST24_4@1
                        uint v17; // r11@1
                        uint v18; // ST1C_4@1
                        uint v19; // r7@1
                        uint v20; // ST18_4@1
                        uint v21; // r7@1
                        uint v22; // r8@1
                        uint v23; // r9@1
                        uint v24; // r10@1
                        uint v25; // ST14_4@1
                        uint v26; // ST10_4@1
                        uint v27; // r1@1
                        uint v28; // ST0C_4@1
                        uint v29; // ST08_4@1
                        uint v30; // r3@1
                        uint v31; // r12@1
                        uint v32; // r6@1
                        uint v33; // ST04_4@1
                        uint v34; // r11@1
                        uint v35; // ST00_4@1
                        uint v36; // r2@1
                        uint v37; // r2@1

                        v1 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                        v2 = IOUtil.ReadU32LE(Dst, Offset - Stride + 4);
                        v3 = (byte)v1;
                        v4 = Dst[Offset - Stride - 1];
                        v5 = v1 << 16 >> 24;
                        v6 = ((uint)Dst[Offset - Stride - 1] + (byte)v1 + 1) >> 1;
                        v7 = (uint)(((byte)v1 + v5 + 1) >> 1);
                        v8 = v1 << 8 >> 24;
                        v9 = (uint)((v5 + v8 + 1) >> 1);
                        v1 >>= 24;
                        v10 = (uint)((v8 + v1 + 1) >> 1);
                        IOUtil.WriteU32LE(Dst, Offset, v6 | (v7 << 8) | (v9 << 16) | (v10 << 24));
                        v12 = (byte)v2;
                        v13 = (uint)((v1 + (byte)v2 + 1) >> 1);
                        v14 = v2 << 16 >> 24;
                        v15 = v2 << 8 >> 24;
                        v16 = (uint)(((byte)v2 + v14 + 1) >> 1);
                        v17 = (uint)((v14 + v15 + 1) >> 1);
                        v18 = v2 >> 24;
                        IOUtil.WriteU32LE(Dst, Offset + 4, (uint)(v13 | (v16 << 8) | (v17 << 16) | (((v15 + (v2 >> 24) + 1) / 2) << 24)));
                        v19 = Dst[Offset - 1];
                        v20 = v19;
                        v21 = (v19 + v3 + 2 * v4 + 2) >> 2;
                        v22 = (uint)((v4 + v5 + 2 * v3 + 2) >> 2);
                        v23 = (uint)((v3 + v8 + 2 * v5 + 2) >> 2);
                        v24 = (uint)((v5 + v1 + 2 * v8 + 2) >> 2);
                        IOUtil.WriteU32LE(Dst, Offset + Stride, v21 | (v22 << 8) | (v23 << 16) | (v24 << 24));
                        v25 = (uint)((v1 + v14 + 2 * v12 + 2) >> 2);
                        v26 = (uint)((v12 + v15 + 2 * v14 + 2) >> 2);
                        v27 = (uint)((v8 + v12 + 2 * v1 + 2) >> 2);
                        IOUtil.WriteU32LE(Dst, Offset + Stride + 4, (uint)(v27 | (v25 << 8) | (v26 << 16) | ((v14 + v18 + 2 * v15 + 2) >> 2 << 24)));
                        v28 = Dst[Offset + Stride - 1];
                        v29 = (v28 + v4 + 2 * v20 + 2) >> 2;
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2, v29 | (v6 << 8) | (v7 << 16) | (v9 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2 + 4, v10 | (v13 << 8) | (v16 << 16) | (v17 << 24));
                        v30 = Dst[Offset + Stride * 2 - 1];
                        v31 = (v30 + v20 + 2 * v28 + 2) >> 2;
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3, v31 | (v21 << 8) | (v22 << 16) | (v23 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3 + 4, v24 | (v27 << 8) | (v25 << 16) | (v26 << 24));
                        v32 = Dst[Offset + Stride * 3 - 1];
                        v33 = (v32 + v28 + 2 * v30 + 2) >> 2;
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 4, v33 | (v29 << 8) | (v6 << 16) | (v7 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 4 + 4, v9 | (v10 << 8) | (v13 << 16) | (v16 << 24));
                        v34 = Dst[Offset + Stride * 4 - 1];
                        v35 = (v34 + v30 + 2 * v32 + 2) >> 2;
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 5, v35 | (v31 << 8) | (v21 << 16) | (v22 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 5 + 4, v23 | (v24 << 8) | (v27 << 16) | (v25 << 24));
                        v36 = Dst[Offset + Stride * 5 - 1];
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 6, ((v36 + v32 + 2 * v34 + 2) >> 2) | (v33 << 8) | (v29 << 16) | (v6 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 6 + 4, v7 | (v9 << 8) | (v10 << 16) | (v13 << 24));
                        v37 = ((Dst[Offset + Stride * 6 - 1] + v34 + 2 * v36 + 2) >> 2) | (v35 << 8) | (v31 << 16) | (v21 << 24);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 7, v37);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 7 + 4, v22 | (v23 << 8) | (v24 << 16) | (v27 << 24));
                        break;
                    }
                case 7://1178BC
                    {
                        uint r11_i = (uint)Offset;
                        uint r6_i = IOUtil.ReadU32LE(Dst, (int)r11_i - Stride);
                        uint r5_i = Dst[r11_i - 1];
                        uint r4_i = Dst[r11_i - Stride - 1];
                        uint r3_i = r6_i & 0xFF;
                        uint r12_i = (r6_i << 16) >> 24;
                        uint r2_i = ((r4_i + r12_i + r3_i * 2) + 2) / 4;
                        uint r1_i = ((r5_i + r3_i + r4_i * 2) + 2) / 4;
                        uint lr_i = (r6_i << 8) >> 24;
                        r3_i = ((r3_i + lr_i + r12_i * 2) + 2) / 4;
                        r6_i >>= 24;
                        r12_i = ((r12_i + r6_i + lr_i * 2) + 2) / 4;
                        uint r8_i = r1_i | (r2_i << 8) | (r3_i << 16) | (r12_i << 24);
                        uint var_20 = r8_i;
                        IOUtil.WriteU32LE(Dst, (int)r11_i, r8_i);
                        uint r7_i = IOUtil.ReadU32LE(Dst, (int)r11_i - Stride + 4);
                        uint r0_i = r3_i | (r12_i << 8);
                        r8_i = r7_i & 0xFF;
                        uint r9_i = (r7_i << 16) >> 24;
                        lr_i = ((lr_i + r8_i + r6_i * 2) + 2) / 4;
                        uint r10_i = (r7_i << 8) >> 24;
                        r6_i = ((r6_i + r9_i + r8_i * 2) + 2) / 4;
                        r8_i = ((r8_i + r10_i + r9_i * 2) + 2) / 4;
                        r7_i = ((r9_i + (r7_i >> 24) + r10_i * 2) + 2) / 4;
                        r7_i = lr_i | (r6_i << 8) | (r8_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 4, r7_i);
                        r7_i = Dst[r11_i + Stride - 1];
                        r10_i = r12_i | (lr_i << 8) | (r6_i << 16);
                        r4_i = ((r7_i + r4_i + r5_i * 2) + 2) / 4;
                        r9_i = r4_i | (r1_i << 8) | (r2_i << 16) | (r3_i << 24);
                        r8_i = r10_i | (r8_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride + 4, r8_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride, r9_i);
                        r11_i += (uint)Stride;
                        r8_i = Dst[r11_i + Stride - 1];
                        r6_i = r0_i | (lr_i << 16) | (r6_i << 24);
                        r5_i = ((r8_i + r5_i + r7_i * 2) + 2) / 4;
                        r10_i = r5_i | (r4_i << 8) | (r1_i << 16) | (r2_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride + 4, r6_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride, r10_i);
                        r11_i += (uint)Stride;
                        r6_i = Dst[r11_i + Stride - 1];
                        r2_i |= (r3_i << 8) | (r12_i << 16) | (lr_i << 24);
                        r7_i = ((r6_i + r7_i + r8_i * 2) + 2) / 4;
                        r1_i = r7_i | (r5_i << 8) | (r4_i << 16) | (r1_i << 24);
                        r11_i += (uint)Stride;
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 0, r1_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 4, r2_i);
                        r2_i = Dst[r11_i + Stride - 1];
                        r3_i = ((r2_i + r8_i + r6_i * 2) + 2) / 4;
                        r12_i = r3_i | (r7_i << 8) | (r5_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride, r12_i);
                        r11_i += (uint)Stride;
                        r8_i = var_20;
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 4, r8_i);
                        r12_i = Dst[r11_i + Stride - 1];
                        lr_i = ((r12_i + r6_i + r2_i * 2) + 2) / 4;
                        r4_i = lr_i | (r3_i << 8) | (r7_i << 16) | (r5_i << 24);
                        r11_i += (uint)Stride;
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 0, r4_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 4, r9_i);
                        r4_i = Dst[r11_i + Stride - 1];
                        r11_i += (uint)Stride;
                        r2_i = ((r4_i + r2_i + r12_i * 2) + 2) / 4;
                        r5_i = r2_i | (lr_i << 8) | (r3_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 0, r5_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + 4, r10_i);
                        r5_i = Dst[r11_i + Stride - 1];
                        r12_i = ((r5_i + r12_i + r4_i * 2) + 2) / 4;
                        r2_i = r12_i | (r2_i << 8) | (lr_i << 16) | (r3_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride, r2_i);
                        IOUtil.WriteU32LE(Dst, (int)r11_i + Stride + 4, r1_i);
                        r11_i -= (uint)Stride * 6;
                        break;
                    }
                case 8://117AF4
                    {
                        uint r0_i = (uint)Offset;
                        uint r2_i = IOUtil.ReadU32LE(Dst, (int)r0_i - Stride);
                        uint r11_i = (r2_i << 16) >> 24;
                        uint r10_i = r2_i & 0xFF;
                        uint r12_i = ((r10_i + r11_i) + 1) / 2;
                        uint r1_i = (r2_i << 8) >> 24;
                        uint var_s20 = r10_i;
                        r10_i = ((r11_i + r1_i) + 1) / 2;
                        r2_i >>= 24;
                        uint var_s1C = r11_i;
                        r11_i = ((r1_i + r2_i) + 1) / 2;
                        uint r9_i = r0_i - (uint)Stride;
                        uint var_s14 = r11_i;
                        uint var_s18 = r10_i;
                        uint r4_i = IOUtil.ReadU32LE(Dst, (int)r9_i + 4);
                        uint r3_i = r4_i & 0xFF;
                        uint r5_i = ((r2_i + r3_i) + 1) / 2;
                        r12_i |= (r10_i << 8) | (r11_i << 16) | (r5_i << 24);
                        uint var_s10 = r5_i;
                        IOUtil.WriteU32LE(Dst, (int)r0_i, r12_i);
                        r12_i = (r4_i << 16) >> 24;
                        uint r8_i = ((r3_i + r12_i) + 1) / 2;
                        uint lr_i = (r4_i << 8) >> 24;
                        uint r6_i = ((r12_i + lr_i) + 1) / 2;
                        r4_i >>= 24;
                        uint var_sC = r8_i;
                        r8_i = IOUtil.ReadU32LE(Dst, (int)r9_i + 8);
                        uint r7_i = ((lr_i + r4_i) + 1) / 2;
                        r5_i = r8_i & 0xFF;
                        r10_i = var_sC;
                        r9_i = ((r4_i + r5_i) + 1) / 2;
                        r10_i |= (r6_i << 8) | (r7_i << 16) | (r9_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + 4, r10_i);
                        r10_i = var_s20;
                        r11_i = var_s1C;
                        r10_i = ((r10_i + r1_i + r11_i * 2) + 2) / 4;
                        uint var_s8 = r10_i;
                        r10_i = ((r11_i + r2_i + r1_i * 2) + 2) / 4;
                        r1_i = ((r1_i + r3_i + r2_i * 2) + 2) / 4;
                        uint var_0 = r1_i;
                        uint var_s4 = r10_i;
                        r1_i = ((r2_i + r12_i + r3_i * 2) + 2) / 4;
                        r2_i = var_s8 | (var_s4 << 8) | (var_0 << 16) | (r1_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride, r2_i);
                        r0_i += (uint)Stride;
                        r2_i = ((r3_i + lr_i + r12_i * 2) + 2) / 4;
                        r3_i = ((r12_i + r4_i + lr_i * 2) + 2) / 4;
                        lr_i = ((lr_i + r5_i + r4_i * 2) + 2) / 4;
                        r12_i = (r8_i << 16) >> 24;
                        r4_i = ((r4_i + r12_i + r5_i * 2) + 2) / 4;
                        r10_i = r2_i | (r3_i << 8) | (lr_i << 16) | (r4_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + 4, r10_i);
                        r10_i = var_s18 | (var_s14 << 8) | (var_s10 << 16) | (var_sC << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride, r10_i);
                        r0_i += (uint)Stride;
                        r11_i = ((r5_i + r12_i) + 1) / 2;
                        r10_i = r6_i | (r7_i << 8) | (r9_i << 16) | (r11_i << 24);
                        uint var_4 = r11_i;
                        IOUtil.WriteU32LE(Dst, (int)r0_i + 4, r10_i);
                        r10_i = var_s4 | (var_0 << 8) | (r1_i << 16) | (r2_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride, r10_i);
                        r0_i += (uint)Stride;
                        r10_i = (r8_i << 8) >> 24;
                        r11_i = ((r5_i + r10_i + r12_i * 2) + 2) / 4;
                        r5_i = r3_i | (lr_i << 8) | (r4_i << 16) | (r11_i << 24);
                        uint var_8 = r11_i;
                        IOUtil.WriteU32LE(Dst, (int)r0_i + 4, r5_i);
                        r5_i = var_s14 | (var_s10 << 8) | (var_sC << 16) | (r6_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride, r5_i);
                        r0_i += (uint)Stride;
                        r11_i = ((r12_i + r10_i) + 1) / 2;
                        uint var_C = r11_i;
                        r11_i = var_4;
                        r5_i = r7_i | (r9_i << 8) | (r11_i << 16) | (var_C << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + 4, r5_i);
                        r11_i = var_0;
                        r5_i = r11_i | (r1_i << 8) | (r2_i << 16) | (r3_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride, r5_i);
                        r0_i += (uint)Stride;
                        r5_i = r8_i >> 24;
                        r12_i = ((r12_i + r5_i + r10_i * 2) + 2) / 4;
                        r11_i = var_8;
                        r8_i = lr_i | (r4_i << 8) | (r11_i << 16) | (r12_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + 4, r8_i);
                        r8_i = var_sC;
                        r11_i = var_s10;
                        r1_i |= r2_i << 8;
                        r6_i = r11_i | (r8_i << 8) | (r6_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride, r6_i);
                        r0_i += (uint)Stride;
                        r6_i = ((r10_i + r5_i) + 1) / 2;
                        r1_i |= (r3_i << 16) | (lr_i << 24);
                        r6_i = r9_i | (var_4 << 8) | (var_C << 16) | (r6_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride, r1_i);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + 4, r6_i);
                        r11_i = r0_i - (uint)Stride * 7;
                        r1_i = ((Dst[r11_i + 0xC] + r10_i + r5_i * 2) + 2) / 4;
                        r11_i = var_8;
                        r1_i = r4_i | (r11_i << 8) | (r12_i << 16) | (r1_i << 24);
                        IOUtil.WriteU32LE(Dst, (int)r0_i + Stride + 4, r1_i);
                        r11_i = r0_i - (uint)Stride * 6;
                        break;
                    }
                case 9: break;
                //4x4 blocks
                case 10://117E34
                    {
                        uint r4 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                        IOUtil.WriteU32LE(Dst, Offset, r4);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 1, r4);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2, r4);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3, r4);
                        break;
                    }
                case 11://117E50
                    {
                        uint r6 = Dst[Offset - 1];
                        IOUtil.WriteU32LE(Dst, Offset, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                        r6 = Dst[Offset + Stride * 1 - 1];
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 1, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                        r6 = Dst[Offset + Stride * 2 - 1];
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                        r6 = Dst[Offset + Stride * 3 - 1];
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                        break;
                    }
                case 12://117E98
                    {
                        sub_117E98(ref nrBitsRemaining, ref r3, Dst, Offset);
                        break;
                    }
                case 13://1180FC
                    {
                        int r8 = 0;
                        if (((Offset - (VOffsetfix ? (Stride / 2) : 0)) % Stride) != 0) r8 += 8;
                        if (Offset >= Stride) r8 += 4;
                        switch (r8 / 4)
                        {
                            case 0://1180C8
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        Dst[Offset + i * Stride] = 0x80;
                                        Dst[Offset + i * Stride + 1] = 0x80;
                                        Dst[Offset + i * Stride + 2] = 0x80;
                                        Dst[Offset + i * Stride + 3] = 0x80;
                                    }
                                    break;
                                }
                            case 1://11807C
                                {
                                    uint r6 = 0;
                                    uint r4 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r6 += 2;
                                    r6 /= 4;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                    }
                                    break;
                                }
                            case 2://118030
                                {
                                    uint r6 = 0;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        r6 += Dst[Offset + i * Stride - 1];
                                    }
                                    r6 += 2;
                                    r6 /= 4;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    uint r6 = 0;
                                    uint r4 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    r4 <<= 8;
                                    r6 += r4 >> 24;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        r6 += Dst[Offset + i * Stride - 1];
                                    }
                                    r6 += 4;
                                    r6 /= 8;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        IOUtil.WriteU32LE(Dst, Offset + i * Stride, r6 | (r6 << 8) | (r6 << 16) | (r6 << 24));
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case 14://debug confirmed okay
                    {
                        //uint v0; // r11@0
                        uint v1; // r5@1
                        uint v2; // r6@1
                        uint v3; // r7@1
                                 //uint v4; // r11@1
                        uint v5; // t1@1
                        uint v6; // r4@1
                        uint v7; // r5@1
                        uint v8; // t1@1
                        uint v9; // r6@1
                        uint v10; // r4@1
                        uint v11; // r5@1
                        uint v12; // r6@1
                        uint v13; // r7@1
                        uint v14; // r8@1

                        v1 = Dst[Offset - 1];
                        v2 = Dst[Offset + Stride - 1];
                        v5 = Dst[Offset + Stride * 2 - 1];
                        //v4 = v0 + 511;
                        v3 = v5;
                        v6 = ((v1 + v2 + 1) >> 1) | ((v1 + 2 * v2 + v5 + 2) >> 2 << 8);
                        v7 = (v2 + v5 + 1) >> 1;
                        v8 = Dst[Offset + Stride * 3 - 1];
                        //v4 += 256;
                        v9 = (v2 + 2 * v3 + v8 + 2) >> 2;
                        v10 = v6 | (v7 << 16) | (v9 << 24);
                        v11 = v7 | (v9 << 8);
                        v12 = (v3 + v8 + 1) >> 1;
                        v13 = (v3 + 2 * v8 + v8 + 2) >> 2;
                        v14 = v8 | (v8 << 8);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3, v14 | (v14 << 16));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2, v12 | (v13 << 8) | (v14 << 16));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 1, v11 | (v12 << 16) | (v13 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 0, v10);
                        break;
                    }
                case 15:
                    {
                        uint v0; // r11@0
                        uint v1; // r8@1
                        uint v2; // r7@1
                        uint v3; // r9@1
                        uint v4; // lr@1
                        uint v5; // r4@1
                        uint v6; // r9@1
                        uint v7; // r8@1
                        uint v8; // r12@1
                                 //uint v9; // r11@1
                        uint v10; // lr@1
                        uint v11; // r7@1
                        uint v12; // r4@1

                        v1 = Dst[Offset - Stride - 1];
                        v2 = Dst[Offset - 1];
                        v3 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                        v4 = (v1 + v2 + 1) >> 1;
                        v5 = (v2 + 2 * v1 + (byte)v3 + 2) >> 2;
                        IOUtil.WriteU32LE(Dst, Offset, (uint)(
                            v4 | (v5 << 8) | ((v1 + 2 * (byte)v3 + ((uint)(v3 << 16) >> 24) + 2) >> 2 << 16) | (((byte)v3 + 2 * ((uint)(v3 << 16) >> 24) + ((uint)(v3 << 8) >> 24) + 2) >> 2 << 24)));
                        v6 = Dst[Offset + Stride - 1];
                        v7 = (v1 + 2 * v2 + v6 + 2) >> 2;
                        v8 = (v2 + v6 + 1) >> 1;
                        IOUtil.WriteU32LE(Dst, Offset + Stride, v8 | (v7 << 8) | (v4 << 16) | (v5 << 24));
                        //v9 = v0 + 256;
                        v10 = Dst[Offset + Stride * 2 - 1];
                        v11 = (v2 + 2 * v6 + v10 + 2) >> 2;
                        v12 = (v6 + v10 + 1) >> 1;
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2, v12 | (v11 << 8) | (v8 << 16) | (v7 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3,
                            ((v10 + Dst[Offset + Stride * 3 - 1] + 1) >> 1) | ((v6 + 2 * v10 + Dst[Offset + Stride * 3 - 1] + 2) >> 2 << 8) | (v12 << 16) | (v11 << 24));
                        break;
                    }
                case 16://1182C4
                    {
                        /*int r11_i = Offset;
                        uint lr_i = IOUtil.ReadU32LE(Dst, r11_i - Stride);
                        uint r3_i = Dst[r11_i - Stride - 1];
                        uint r1_i = lr_i & 0xFF;*/
                        uint v0; // r11@0
                        uint v1; // lr@1
                        uint v2; // r3@1
                        uint v3; // r1@1
                        uint v4; // r7@1
                        uint v5; // r4@1
                        uint v6; // r2@1
                        uint v7; // r5@1
                        uint v8; // r12@1
                        uint v9; // r6@1
                        uint v10; // lr@1
                        uint v11; // r9@1
                        uint v12; // r8@1
                        uint v13; // r1@1
                        uint v14; // r11@1
                        uint v15; // r2@1
                        v1 = IOUtil.ReadU32LE(Dst, Offset - Stride);
                        v2 = Dst[Offset - Stride - 1];
                        v3 = v1 & 0xFF;
                        v4 = v1 >> 24;
                        v5 = (v2 + (byte)v1 + 1) >> 1;
                        v6 = v1 << 16 >> 24;
                        v7 = ((byte)v1 + v6 + 1) >> 1;
                        v8 = v1 << 8 >> 24;
                        v9 = (v6 + v8 + 1) >> 1;
                        IOUtil.WriteU32LE(Dst, Offset, v5 | (v7 << 8) | (v9 << 16) | ((v8 + (v1 >> 24) + 1) >> 1 << 24));
                        v10 = Dst[Offset - 1];
                        v11 = (v2 + 2 * v3 + v6 + 2) >> 2;
                        v12 = (v10 + 2 * v2 + v3 + 2) >> 2;
                        v13 = (v3 + 2 * v6 + v8 + 2) >> 2;
                        IOUtil.WriteU32LE(Dst, Offset + Stride, v12 | (v11 << 8) | (v13 << 16) | ((v6 + 2 * v8 + v4 + 2) >> 2 << 24));
                        // *(v0 + 256) = v12 | (v11 << 8) | (v13 << 16) | ((v6 + 2 * v8 + v4 + 2) >> 2 << 24);
                        //v14 = v0 + 256;
                        v15 = Dst[Offset + Stride - 1];//*(v14 - 1);
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 2, ((v2 + 2 * v10 + v15 + 2) >> 2) | (v5 << 8) | (v7 << 16) | (v9 << 24));
                        IOUtil.WriteU32LE(Dst, Offset + Stride * 3, ((v10 + 2 * v15 + Dst[Offset + Stride * 2 - 1] + 2) >> 2) | (v12 << 8) | (v11 << 16) | (v13 << 24));
                        //*(v14 + 256) = ((v2 + 2 * v10 + v15 + 2) >> 2) | (v5 << 8) | (v7 << 16) | (v9 << 24);
                        // *(v14 + 512) = ((v10 + 2 * v15 + *(v14 + 255) + 2) >> 2) | (v12 << 8) | (v11 << 16) | (v13 << 24);
                        break;
                    }
                case 17://1183CC
                    {
                        int r11_i = Offset;
                        uint r7_i = IOUtil.ReadU32LE(Dst, r11_i - Stride);
                        uint r12_i = Dst[r11_i - Stride - 1];
                        uint lr_i = r7_i & 0xFF;
                        uint r9_i = Dst[r11_i - 1];
                        uint r4_i = (r7_i << 16) >> 24;
                        uint r5_i = ((r12_i + lr_i * 2 + r4_i) + 2) / 4;
                        uint r8_i = ((r9_i + r12_i * 2 + lr_i) + 2) / 4;
                        uint r6_i = (r7_i << 8) >> 24;
                        lr_i = ((lr_i + r4_i * 2 + r6_i) + 2) / 4;
                        r7_i = ((r4_i + r6_i * 2 + (r7_i >> 24)) + 2) / 4;
                        r7_i = r8_i | (r5_i << 8) | (lr_i << 16) | (r7_i << 24);
                        IOUtil.WriteU32LE(Dst, r11_i, r7_i);
                        r7_i = Dst[r11_i + Stride - 1];
                        r12_i = ((r7_i + r9_i * 2 + r12_i) + 2) / 4;
                        lr_i = r12_i | (r8_i << 8) | (r5_i << 16) | (lr_i << 24);
                        IOUtil.WriteU32LE(Dst, r11_i + Stride, lr_i);
                        r11_i += Stride;
                        lr_i = Dst[r11_i + Stride - 1];
                        r9_i = ((lr_i + r7_i * 2 + r9_i) + 2) / 4;
                        r4_i = r9_i | (r12_i << 8) | (r8_i << 16) | (r5_i << 24);
                        IOUtil.WriteU32LE(Dst, r11_i + Stride, r4_i);
                        r11_i += Stride;
                        r4_i = Dst[r11_i + Stride - 1];
                        r7_i = ((r4_i + lr_i * 2 + r7_i) + 2) / 4;
                        r7_i = r7_i | (r9_i << 8) | (r12_i << 16) | (r8_i << 24);
                        IOUtil.WriteU32LE(Dst, r11_i + Stride, r7_i);
                        r11_i -= Stride * 2;
                        break;
                    }
                case 18://1184B4
                    {
                        int r11_i = Offset;
                        uint r9_i = IOUtil.ReadU32LE(Dst, r11_i - Stride);
                        uint r2_i = (uint)r11_i - (uint)Stride;
                        uint r6_i = (r9_i << 16) >> 24;
                        uint r7_i = r9_i & 0xFF;
                        uint r8_i = ((r7_i + r6_i) + 1) / 2;
                        uint r12_i = (r9_i << 8) >> 24;
                        uint lr_i = ((r6_i + r12_i) + 1) / 2;
                        r9_i >>= 24;
                        uint r4_i = ((r12_i + r9_i) + 1) / 2;
                        uint r3_i = IOUtil.ReadU32LE(Dst, (int)r2_i + 4);
                        r7_i = r7_i + r6_i * 2 + r12_i;
                        r2_i = r3_i & 0xFF;
                        uint r5_i = ((r9_i + r2_i) + 1) / 2;
                        r8_i |= (lr_i << 8) | (r4_i << 16) | (r5_i << 24);
                        IOUtil.WriteU32LE(Dst, r11_i, r8_i);
                        r8_i = (r7_i + 2) / 4;
                        r7_i = ((r12_i + r9_i * 2 + r2_i) + 2) / 4;
                        r6_i = ((r6_i + r12_i * 2 + r9_i) + 2) / 4;
                        r12_i = (r3_i << 16) >> 24;
                        r9_i = ((r9_i + r2_i * 2 + r12_i) + 2) / 4;
                        r8_i |= (r6_i << 8) | (r7_i << 16) | (r9_i << 24);
                        IOUtil.WriteU32LE(Dst, r11_i + Stride, r8_i);
                        r11_i += Stride;
                        r8_i = ((r2_i + r12_i) + 1) / 2;
                        r2_i = ((r2_i + r12_i * 2 + ((r3_i << 8) >> 24)) + 2) / 4;
                        lr_i |= (r4_i << 8) | (r5_i << 16) | (r8_i << 24);
                        r9_i = r6_i | (r7_i << 8) | (r9_i << 16) | (r2_i << 24);
                        IOUtil.WriteU32LE(Dst, r11_i + Stride, lr_i);
                        r11_i += Stride;
                        IOUtil.WriteU32LE(Dst, r11_i + Stride, r9_i);
                        r11_i -= Stride * 2;
                        break;
                    }
                case 19: break;
                default:
                    break;
            }
        }

        private void loc_116368(ref int nrBitsRemaining, ref uint r3, int r5, byte[] Dst, int Offset)
        {
            if (((r3 >> 31) & 1) == 1)
            {
                r3 <<= 1;
                nrBitsRemaining--;
                fixed (uint* InternalPtr = &Internal[0])
                {
                    byte* InternalByte = (byte*)InternalPtr;
                    uint r12 = InternalByte[r5 - 8];
                    uint r6 = InternalByte[r5 - 1];
                    if (r12 > r6) r12 = r6;
                    if (r12 == 9) r12 = 3;
                    r6 = r3 >> 28;
                    if (r6 >= r12) r6++;
                    int r7;
                    if (r6 < 9)
                    {
                        r12 = r6;
                        r7 = 4;
                    }
                    else r7 = 1;
                    r3 <<= r7;
                    nrBitsRemaining -= r7;
                    if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                    InternalByte[r5] = (byte)r12;
                    InternalByte[r5 + 1] = (byte)r12;
                    InternalByte[r5 + 8] = (byte)r12;
                    InternalByte[r5 + 9] = (byte)r12;
                    loc_116518(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                }
            }
            else
            {
                uint r4 = byte_1164F4[ReadVarIntUnsigned(ref nrBitsRemaining, ref r3)];
                uint r12 = sub_1163DC(ref nrBitsRemaining, ref r3, r5);
                if ((r4 & 1) == 1)
                    loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset += 4;
                r12 = sub_1163DC(ref nrBitsRemaining, ref r3, r5 + 1);
                if (((r4 >> 1) & 1) == 1)
                    loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset += Stride * 4;
                Offset -= 4;
                r12 = sub_1163DC(ref nrBitsRemaining, ref r3, r5 + 8);
                if (((r4 >> 2) & 1) == 1)
                    loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset += 4;
                r12 = sub_1163DC(ref nrBitsRemaining, ref r3, r5 + 9);
                if (((r4 >> 3) & 1) == 1)
                    loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset -= Stride * 4;
                Offset -= 4;
            }
        }

        private uint sub_1163DC(ref int nrBitsRemaining, ref uint r3, int r5)
        {
            fixed (uint* InternalPtr = &Internal[0])
            {
                byte* InternalByte = (byte*)InternalPtr;
                uint r12 = InternalByte[r5 - 8];
                uint r6 = InternalByte[r5 - 1];
                if (r12 > r6) r12 = r6;
                if (r12 == 9) r12 = 3;
                r6 = r3 >> 28;
                if (r6 >= r12) r6++;
                int r7;
                if (r6 < 9)
                {
                    r12 = r6;
                    r7 = 4;
                }
                else r7 = 1;
                InternalByte[r5] = (byte)r12;
                r12 += 0xA;
                r3 <<= r7;
                nrBitsRemaining -= r7;
                if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                return r12;
            }
        }

        private byte[] byte_1164F4 =
        {
            0x00, 0x0F, 0x00, 0x02, 0x01, 0x04, 0x08, 0x0C, 0x03, 0x0B, 0x0D, 0x0E,
            0x07, 0x0A, 0x05, 0x09, 0x06, 0x00, 0x00, 0x00
        };

        private void sub_116508(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset, uint r12)
        {
            if (((r3 >> 31) & 1) == 1)
            {
                r3 += r3;
                nrBitsRemaining--;
                loc_116518(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
            }
            else
            {
                r12 += 0xA;
                int r4 = byte_1164F4[ReadVarIntUnsigned(ref nrBitsRemaining, ref r3)];
                if ((r4 & 1) == 1) loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset += 4;
                if (((r4 >> 1) & 1) == 1) loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset += Stride * 4;
                Offset -= 4;
                if (((r4 >> 2) & 1) == 1) loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset += 4;
                if (((r4 >> 3) & 1) == 1) loc_116628(ref nrBitsRemaining, ref r3, Dst, Offset, r12);
                else PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
                Offset -= Stride * 4;
                Offset -= 4;
            }
        }

        private void loc_116518(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset, uint r12)
        {
            PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
            loc_116540(ref nrBitsRemaining, ref r3, Dst, Offset);
        }

        private byte[] byte_1165C4 =
        {
            0, 4, 1, 8, 2, 0xC, 3, 5, 0xA, 0xF, 7, 0xD, 0xE, 0xB, 9, 6
        };

        private void loc_11652C(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset)
        {
            if (((r3 >> 31) & 1) == 1)
            {
                r3 += r3;
                nrBitsRemaining--;
                loc_116540(ref nrBitsRemaining, ref r3, Dst, Offset);
            }
            else
            {
                uint r12 = byte_1165C4[ReadVarIntUnsigned(ref nrBitsRemaining, ref r3)];
                if ((r12 & 1) != 0) sub_1166E8(ref nrBitsRemaining, ref r3, Dst, Offset);
                Offset += 4;
                if (((r12 >> 1) & 1) != 0) sub_1166E8(ref nrBitsRemaining, ref r3, Dst, Offset);
                Offset += Stride * 4;
                Offset -= 4;
                if (((r12 >> 2) & 1) != 0) sub_1166E8(ref nrBitsRemaining, ref r3, Dst, Offset);
                Offset += 4;
                if (((r12 >> 3) & 1) != 0) sub_1166E8(ref nrBitsRemaining, ref r3, Dst, Offset);
            }
        }

        private void loc_116540(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset)
        {
            for (int i = 0; i < 64; i++)
            {
                Internal[90 + i] = 0;
            }
            uint r12_2 = 10;
            ReadDCTMatrix(ref nrBitsRemaining, ref r3, ref r12_2);
            if (r12_2 <= 11) IDCT1Px8(Dst, Offset);
            else if (r12_2 <= 13) IDCT3Px8(Dst, Offset);
            else if (r12_2 <= 20) IDCT16Px8(Dst, Offset);
            else IDCT64Px8(Dst, Offset);
        }

        private void loc_116628(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset, uint r12)
        {
            PredictIntra(ref nrBitsRemaining, ref r3, r12, Dst, Offset);
            for (int i = 0; i < 16; i++)
            {
                Internal[90 + i] = 0;
            }
            uint r12_2 = 74;
            ReadDCTMatrix(ref nrBitsRemaining, ref r3, ref r12_2);
            if (r12_2 <= 75) IDCT1Px4(Dst, Offset);
            else IDCT16Px4(Dst, Offset);
        }

        private void sub_1166E8(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset)
        {
            for (int i = 0; i < 16; i++)
            {
                Internal[90 + i] = 0;
            }
            uint r12_2 = 74;
            ReadDCTMatrix(ref nrBitsRemaining, ref r3, ref r12_2);
            if (r12_2 <= 75) IDCT1Px4(Dst, Offset);
            else IDCT16Px4(Dst, Offset);
        }

        private uint ReadVarIntUnsigned(ref int nrBitsRemaining, ref uint r3)
        {
            int r10 = CLZ(r3);//nr zeros
            r3 <<= r10;//remove the zeros
            r3 += r3;//remove the stop bit
            int r9 = 0x20 - r10;//shift amount
            uint r6;
            if (r9 == 0x20) r6 = 0;
            else r6 = r3 >> r9;
            r9 = 1;
            r6 += (uint)(r9 << r10);
            r6--;
            r3 <<= r10;
            nrBitsRemaining -= r10 << 1;
            if (--nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            return r6;
        }

        private void FillBits(ref int nrBitsRemaining, ref uint r3)
        {
            if (Offset >= Data.Length) return;
            uint r10 = IOUtil.ReadU16LE(Data, Offset);
            Offset += 2;
            nrBitsRemaining += 0x10;
            int r9 = 0x10 - nrBitsRemaining;
            r3 |= r10 << r9;
        }

        private int ReadVarIntSigned(ref int nrBitsRemaining, ref uint r3)
        {
            int r10 = CLZ(r3);
            r3 <<= r10;
            r3 += r3;
            int r9 = 0x20 - r10;
            int r6;
            if (r9 == 0x20) r6 = 0;
            else r6 = (int)(r3 >> r9);
            r9 = 1;
            r6 += r9 << r10;
            if ((r6 & 1) != 0) r6 = 1 - r6;
            r6 >>= 1;
            r3 <<= r10;
            nrBitsRemaining -= r10 << 1;
            if (--nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
            return r6;
        }

        private void sub_1167BC(byte[] Dst, int Offset, ref int nrBitsRemaining, ref uint r3)
        {
            int r6 = ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            byte[] vals = new byte[16];
            Array.Copy(Dst, Offset - Stride, vals, 0, 16);
            int r4 = Dst[Offset + Stride * 15 - 1];
            int r10 = vals[15];
            int r5 = ((r4 + r10) + 1) >> 1;
            r5 += r6 * 2;
            r6 = r5 - r4;
            r6++;
            r4 <<= 3;
            int[] sp_min0x80 = new int[32];
            for (int i = 0; i < 16; i++)
            {
                r4 += r6 >> 1;
                sp_min0x80[i * 2] = vals[i] * 64;
                sp_min0x80[i * 2 + 1] = (r4 - vals[i] * 8) + 1;
            }
            int r9 = r5 - r10;
            r9++;
            r10 <<= 3;
            uint lr = 16;
            while (true)
            {
                r10 += r9 >> 1;
                int r8 = Dst[Offset - 1];
                int r7 = r10 - (r8 << 3);
                r7++;
                r8 <<= 6;
                int r0_i = sp_min0x80[0];
                int r1_i = sp_min0x80[1];
                int r2_i = sp_min0x80[2];
                int r3_i2 = sp_min0x80[3];
                r4 = sp_min0x80[4];
                r5 = sp_min0x80[5];
                r6 = sp_min0x80[6];
                int r12 = sp_min0x80[7];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[0] = r0_i;
                sp_min0x80[2] = r2_i;
                sp_min0x80[4] = r4;
                sp_min0x80[6] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Dst, Offset, (uint)r5);
                Offset += 4;
                r0_i = sp_min0x80[8];
                r1_i = sp_min0x80[9];
                r2_i = sp_min0x80[10];
                r3_i2 = sp_min0x80[11];
                r4 = sp_min0x80[12];
                r5 = sp_min0x80[13];
                r6 = sp_min0x80[14];
                r12 = sp_min0x80[15];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[8] = r0_i;
                sp_min0x80[10] = r2_i;
                sp_min0x80[12] = r4;
                sp_min0x80[14] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Dst, Offset, (uint)r5);
                Offset += 4;
                r0_i = sp_min0x80[16];
                r1_i = sp_min0x80[17];
                r2_i = sp_min0x80[18];
                r3_i2 = sp_min0x80[19];
                r4 = sp_min0x80[20];
                r5 = sp_min0x80[21];
                r6 = sp_min0x80[22];
                r12 = sp_min0x80[23];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[16] = r0_i;
                sp_min0x80[18] = r2_i;
                sp_min0x80[20] = r4;
                sp_min0x80[22] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Dst, Offset, (uint)r5);
                Offset += 4;
                r0_i = sp_min0x80[24];
                r1_i = sp_min0x80[25];
                r2_i = sp_min0x80[26];
                r3_i2 = sp_min0x80[27];
                r4 = sp_min0x80[28];
                r5 = sp_min0x80[29];
                r6 = sp_min0x80[30];
                r12 = sp_min0x80[31];
                r0_i += r1_i >> 1;
                r2_i += r3_i2 >> 1;
                r4 += r5 >> 1;
                r6 += r12 >> 1;
                sp_min0x80[24] = r0_i;
                sp_min0x80[26] = r2_i;
                sp_min0x80[28] = r4;
                sp_min0x80[30] = r6;
                r8 += r7 >> 1;
                r5 = ((r0_i + r8) + 64) >> 7;
                r8 += r7 >> 1;
                r12 = ((r2_i + r8) + 64) >> 7;
                r5 |= (r12 << 8);
                r8 += r7 >> 1;
                r12 = ((r4 + r8) + 64) >> 7;
                r5 |= (r12 << 16);
                r8 += r7 >> 1;
                r12 = ((r6 + r8) + 64) >> 7;
                r5 |= (r12 << 24);
                IOUtil.WriteU32LE(Dst, Offset, (uint)r5);
                Offset += Stride - 12;
                lr--;
                if (lr <= 0) break;
            }
        }

        private void sub_116CCC(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset)
        {
            int r6 = (int)ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            byte[] vals = new byte[8];
            Array.Copy(Dst, Offset - Stride, vals, 0, 8);
            int r4 = Dst[Offset + Stride * 7 - 1];
            int r10 = vals[7];
            int r5 = ((r4 + r10) + 1) >> 1;
            r5 += r6 * 2;
            r6 = r5 - r4;
            r4 *= 8;
            int[] sp_min0x40 = new int[16];
            for (int i = 0; i < 8; i++)
            {
                r4 += r6;
                sp_min0x40[i * 2] = vals[i] * 64;
                sp_min0x40[i * 2 + 1] = r4 - vals[i] * 8;
            }
            int r9 = r5 - r10;
            r10 <<= 3;
            uint lr = 8;
            while (true)
            {
                r10 += r9;
                int r8 = Dst[Offset - 1];
                int r7 = r10 - r8 * 8;
                r8 *= 64;
                int r0_i = sp_min0x40[0];
                int r1_i = sp_min0x40[1];
                int r2 = sp_min0x40[2];
                int r3_i = sp_min0x40[3];
                r4 = sp_min0x40[4];
                r5 = sp_min0x40[5];
                r6 = sp_min0x40[6];
                int r12 = sp_min0x40[7];
                r0_i += r1_i;
                r2 += r3_i;
                r4 += r5;
                r6 += r12;
                sp_min0x40[0] = r0_i;
                sp_min0x40[2] = r2;
                sp_min0x40[4] = r4;
                sp_min0x40[6] = r6;
                r8 += r7;
                uint r5_i = (uint)(((r0_i + r8) + 64) >> 7);
                r8 += r7;
                r5_i |= ((uint)(((r2 + r8) + 64) >> 7) << 8);
                r8 += r7;
                r5_i |= ((uint)(((r4 + r8) + 64) >> 7) << 16);
                r8 += r7;
                r5_i |= ((uint)(((r6 + r8) + 64) >> 7) << 24);
                IOUtil.WriteU32LE(Dst, Offset, r5_i);
                Offset += 4;
                r0_i = sp_min0x40[8];
                r1_i = sp_min0x40[9];
                r2 = sp_min0x40[10];
                r3_i = sp_min0x40[11];
                r4 = sp_min0x40[12];
                r5 = sp_min0x40[13];
                r6 = sp_min0x40[14];
                r12 = sp_min0x40[15];
                r0_i += r1_i;
                r2 += r3_i;
                r4 += r5;
                r6 += r12;
                sp_min0x40[8] = r0_i;
                sp_min0x40[10] = r2;
                sp_min0x40[12] = r4;
                sp_min0x40[14] = r6;
                r8 += r7;
                r5_i = (uint)(((r0_i + r8) + 64) >> 7);
                r8 += r7;
                r5_i |= ((uint)(((r2 + r8) + 64) >> 7) << 8);
                r8 += r7;
                r5_i |= ((uint)(((r4 + r8) + 64) >> 7) << 16);
                r8 += r7;
                r5_i |= ((uint)(((r6 + r8) + 64) >> 7) << 24);
                IOUtil.WriteU32LE(Dst, Offset, r5_i);
                Offset += Stride - 4;
                lr--;
                if (lr <= 0) break;
            }
            Offset -= Stride * 8;
        }

        private void sub_117E98(ref int nrBitsRemaining, ref uint r3, byte[] Dst, int Offset)
        {
            int r6 = (int)ReadVarIntSigned(ref nrBitsRemaining, ref r3);
            uint r0 = IOUtil.ReadU32LE(Dst, Offset - Stride);
            int r4 = Dst[Offset + Stride * 3 - 1];
            int r10 = (int)(r0 >> 24);
            int r5 = ((r4 + r10) + 1) >> 1;
            r5 += r6 * 2;
            r6 = r5 - r4;
            r4 <<= 2;
            r4 += r6;
            int r7 = (int)(r0 & 0xFF);
            int r8 = r4 - (r7 << 2);
            r7 <<= 4;
            r4 += r6;
            int r9 = (int)((r0 >> 8) & 0xFF);
            int r12 = r4 - (r9 << 2);
            r9 <<= 4;
            int[] sp_min0x20 = new int[8];
            sp_min0x20[0] = r7;
            sp_min0x20[1] = r8;
            sp_min0x20[2] = r9;
            sp_min0x20[3] = r12;
            r4 += r6;
            r7 = (int)((r0 >> 16) & 0xFF);
            r8 = r4 - (r7 << 2);
            r7 <<= 4;
            r4 += r6;
            r9 = (int)((r0 >> 24) & 0xFF);
            r12 = r4 - (r9 << 2);
            r9 <<= 4;
            sp_min0x20[4] = r7;
            sp_min0x20[5] = r8;
            sp_min0x20[6] = r9;
            sp_min0x20[7] = r12;
            r9 = r5 - r10;
            r10 <<= 2;
            uint lr = 4;
            while (true)
            {
                r10 += r9;
                r8 = Dst[Offset - 1];
                r7 = r10 - (r8 << 2);
                r8 <<= 4;
                int r0_i = sp_min0x20[0];
                int r1 = sp_min0x20[1];
                int r2 = sp_min0x20[2];
                int r3_i = sp_min0x20[3];
                r4 = sp_min0x20[4];
                r5 = sp_min0x20[5];
                r6 = sp_min0x20[6];
                r12 = sp_min0x20[7];
                r0_i += r1;
                r2 += r3_i;
                r4 += r5;
                r6 += r12;
                sp_min0x20[0] = r0_i;
                sp_min0x20[2] = r2;
                sp_min0x20[4] = r4;
                sp_min0x20[6] = r6;
                r8 += r7;
                uint r5_i = (uint)(((r0_i + r8) + 16) >> 5);
                r8 += r7;
                r5_i |= (uint)((((r2 + r8) + 16) >> 5) << 8);
                r8 += r7;
                r5_i |= (uint)((((r4 + r8) + 16) >> 5) << 16);
                r8 += r7;
                r5_i |= (uint)((((r6 + r8) + 16) >> 5) << 24);
                IOUtil.WriteU32LE(Dst, Offset, r5_i);
                Offset += Stride;
                lr--;
                if (lr <= 0) break;
            }
            Offset -= Stride * 4;
        }

        //sub_1186A0
        private void ReadDCTMatrix(ref int nrBitsRemaining, ref uint r3, ref uint r12)
        {
            ushort[] r11A = (Internal[218] == 1 ? Table1A : Table0A);
            byte[] r11B = (Internal[218] == 1 ? Table1B : Table0B);
            while (true)
            {
                int skip;
                uint r4 = r3 >> 25;
                int r5;
                int value;
                int r7;
                uint r8;
                if (r4 == 3)
                {
                    r3 <<= 7;
                    bool C = (r3 >> 31) == 1;
                    r3 <<= 1;
                    if (!C)
                    {
                        nrBitsRemaining -= 8;
                        if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                        r4 = r3 >> 20;
                        r4 = r11A[r4];
                        r7 = r11B[r4 >> 9];
                        r5 = (int)(r4 & 0xF);//nr bits
                        r4 >>= 4;
                        value = (int)(r4 & 0x1F);
                        value += r7;
                        r4 >>= 5;
                        r3 <<= r5 - 1;
                        if (((r3 >> 31) & 1) == 1) value = -value;
                        r3 <<= 1;
                        nrBitsRemaining -= r5;
                        if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                        skip = (int)(r4 & 0x3F);
                        r4 >>= 6;
                    }
                    else
                    {
                        C = (r3 >> 31) == 1;
                        r3 <<= 1;
                        if (!C)
                        {
                            nrBitsRemaining -= 9;
                            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                            r4 = r3 >> 20;
                            r4 = r11A[r4];
                            r5 = (int)(r4 & 0xF);
                            r4 >>= 4;
                            value = (int)(r4 & 0x1F);
                            r4 >>= 5;
                            r8 = r4 & 0x3F;
                            r4 >>= 6;
                            r7 = r11B[0x80 + value + (r4 << 6)];
                            r3 <<= r5 - 1;
                            if (((r3 >> 31) & 1) == 1) value = -value;
                            r3 <<= 1;
                            nrBitsRemaining -= (int)r5;
                            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                            skip = (int)r8 + r7;
                        }
                        else
                        {
                            nrBitsRemaining -= 9;
                            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                            r4 = r3 >> 31;//stop
                            r3 <<= 1;
                            skip = (int)(r3 >> 26);//skip
                            r3 <<= 6;
                            nrBitsRemaining -= 7;
                            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                            value = (int)r3 >> 20;//value
                            r3 <<= 12;
                            nrBitsRemaining -= 12;
                            if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                        }
                    }
                }
                else
                {
                    r4 = r3 >> 20;
                    r4 = r11A[r4];
                    r5 = (int)(r4 & 0xF);//nr bits
                    r4 >>= 4;
                    value = (int)r4 & 0x1F;
                    r4 >>= 5;
                    r3 <<= (int)(r5 - 1);
                    if (((r3 >> 31) & 1) == 1) value = -value;
                    r3 <<= 1;
                    nrBitsRemaining -= (int)r5;
                    if (nrBitsRemaining < 0) FillBits(ref nrBitsRemaining, ref r3);
                    skip = (int)(r4 & 0x3F);
                    r4 >>= 6;
                }
                r12 = (uint)(r12 + skip);
                r8 = Internal[r12++];
                r5 = (int)(r8 & 0xFF);//zigzag idx
                r7 = (int)(r8 >> 8);
                r7 *= value;
                Internal[90 + r5] = (uint)r7;
                if ((r4 & 1) != 0) break;
            }
        }

        //loc_118710
        private void IDCT64Px8(byte[] Dst, int Offset)
        {
            int lr = 90;
            int r11 = lr + 64;
            int r0 = (int)Internal[lr++];
            int r1 = (int)Internal[lr++];
            int r2 = (int)Internal[lr++];
            int r3 = (int)Internal[lr++];
            int r4 = (int)Internal[lr++];
            int r5 = (int)Internal[lr++];
            int r6 = (int)Internal[lr++];
            int r7 = (int)Internal[lr++];
            int r8, r9;
            r0 += 0x20;
            int r12 = 8;
            while (true)
            {
                r8 = r0 + r4;
                r9 = r0 - r4;
                r0 = r2 + (r6 >> 1);
                r4 = (r2 >> 1) - r6;
                r2 = r9 + r4;
                r4 = r9 - r4;
                r6 = r8 - r0;
                r0 = r8 + r0;
                r8 = r1 + r7;
                r8 -= r3;
                r8 -= (r3 >> 1);
                r9 = r7 - r1;
                r9 += r5;
                r9 += (r5 >> 1);
                r7 += (r7 >> 1);
                r7 = r5 - r7;
                r7 -= r3;
                r3 += r5;
                r3 += r1;
                r3 += (r1 >> 1);
                r1 = r7 + (r3 >> 2);
                r7 = r3 - (r7 >> 2);
                r3 = r8 + (r9 >> 2);
                r5 = (r8 >> 2) - r9;
                r0 += r7;
                r7 = r0 - r7 * 2;
                r8 = r2 + r5;
                r9 = r2 - r5;
                r2 = r4 + r3;
                r5 = r4 - r3;
                r3 = r6 + r1;
                r4 = r6 - r1;
                r1 = r8;
                r6 = r9;
                Internal[r11 + 56] = (uint)r7;
                Internal[r11 + 48] = (uint)r6;
                Internal[r11 + 40] = (uint)r5;
                Internal[r11 + 32] = (uint)r4;
                Internal[r11 + 24] = (uint)r3;
                Internal[r11 + 16] = (uint)r2;
                Internal[r11 + 8] = (uint)r1;
                Internal[r11 + 0] = (uint)r0;
                r11++;
                r12--;
                if (r12 <= 0) break;
                r0 = (int)Internal[lr++];
                r1 = (int)Internal[lr++];
                r2 = (int)Internal[lr++];
                r3 = (int)Internal[lr++];
                r4 = (int)Internal[lr++];
                r5 = (int)Internal[lr++];
                r6 = (int)Internal[lr++];
                r7 = (int)Internal[lr++];
            }
            r11 -= 8;
            for (int i = 0; i < 8; i++)
            {
                r0 = (int)Internal[r11++];
                r1 = (int)Internal[r11++];
                r2 = (int)Internal[r11++];
                r3 = (int)Internal[r11++];
                r4 = (int)Internal[r11++];
                r5 = (int)Internal[r11++];
                r6 = (int)Internal[r11++];
                r7 = (int)Internal[r11++];
                r9 = r0 + r4;
                int r10 = r0 - r4;
                r0 = r2 + (r6 >> 1);
                r4 = (r2 >> 1) - r6;
                r2 = r10 + r4;
                r4 = r10 - r4;
                r6 = r9 - r0;
                r0 = r9 + r0;
                r9 = r1 + r7;
                r9 -= r3;
                r9 -= (r3 >> 1);
                r10 = r7 - r1;
                r10 += r5;
                r10 += (r5 >> 1);
                r7 += (r7 >> 1);
                r7 = r5 - r7;
                r7 -= r3;
                r3 += r5;
                r3 += r1;
                r3 += (r1 >> 1);
                r1 = r7 + (r3 >> 2);
                r7 = r3 - (r7 >> 2);
                r3 = r9 + (r10 >> 2);
                r5 = (r9 >> 2) - r10;
                r0 += r7;
                r7 = r0 - r7 * 2;
                r9 = r2 + r5;
                r10 = r2 - r5;
                r2 = r4 + r3;
                r5 = r4 - r3;
                r3 = r6 + r1;
                r4 = r6 - r1;
                r1 = r9;
                r6 = r10;
                Dst[Offset + 0] = MinMaxTable[0x40 + Dst[Offset + 0] + (r0 >> 6)];
                Dst[Offset + 1] = MinMaxTable[0x40 + Dst[Offset + 1] + (r1 >> 6)];
                Dst[Offset + 2] = MinMaxTable[0x40 + Dst[Offset + 2] + (r2 >> 6)];
                Dst[Offset + 3] = MinMaxTable[0x40 + Dst[Offset + 3] + (r3 >> 6)];
                Dst[Offset + 4] = MinMaxTable[0x40 + Dst[Offset + 4] + (r4 >> 6)];
                Dst[Offset + 5] = MinMaxTable[0x40 + Dst[Offset + 5] + (r5 >> 6)];
                Dst[Offset + 6] = MinMaxTable[0x40 + Dst[Offset + 6] + (r6 >> 6)];
                Dst[Offset + 7] = MinMaxTable[0x40 + Dst[Offset + 7] + (r7 >> 6)];
                Offset += Stride;
            }
        }

        //sub_11890C
        private void IDCT16Px8(byte[] Dst, int Offset)
        {
            int lr = 90;
            int r11 = lr + 64;
            int r0 = (int)Internal[lr++];
            int r1 = (int)Internal[lr++];
            int r2 = (int)Internal[lr++];
            int r3 = (int)Internal[lr++];
            r0 += 0x20;
            int r12 = 4;
            while (true)
            {
                lr += 4;
                int r4 = r0 - (r2 >> 1);
                int r6 = r0 - r2;
                int r9 = r0 + (r2 >> 1);
                r0 += r2;
                int r8 = r1 - r3;
                r8 -= (r3 >> 1);
                int r7 = r3 + r1;
                r7 += (r1 >> 1);
                r2 = -r3;
                int r5 = r1 + (r8 >> 2);
                r3 = -r1;
                r3 = r8 + (r3 >> 2);
                r1 = r2 + (r7 >> 2);
                r7 -= (r2 >> 2);
                r0 += r7;
                r7 = r0 - r7 * 2;
                r8 = r9 + r5;
                r9 -= r5;
                r2 = r4 + r3;
                r5 = r4 - r3;
                r3 = r6 + r1;
                r4 = r6 - r1;
                r1 = r8;
                r6 = r9;
                Internal[r11 + 28] = (uint)r7;
                Internal[r11 + 24] = (uint)r6;
                Internal[r11 + 20] = (uint)r5;
                Internal[r11 + 16] = (uint)r4;
                Internal[r11 + 12] = (uint)r3;
                Internal[r11 + 8] = (uint)r2;
                Internal[r11 + 4] = (uint)r1;
                Internal[r11 + 0] = (uint)r0;
                r11++;
                r12--;
                if (r12 <= 0) break;
                r0 = (int)Internal[lr++];
                r1 = (int)Internal[lr++];
                r2 = (int)Internal[lr++];
                r3 = (int)Internal[lr++];
            }
            r11 -= 4;
            for (int i = 0; i < 8; i++)
            {
                r0 = (int)Internal[r11++];
                r1 = (int)Internal[r11++];
                r2 = (int)Internal[r11++];
                r3 = (int)Internal[r11++];
                int r4 = r0 - (r2 >> 1);
                int r6 = r0 - r2;
                int r10 = r0 + (r2 >> 1);
                r0 += r2;
                int r9 = r1 - r3;
                r9 -= (r3 >> 1);
                int r7 = r3 + r1;
                r7 += (r1 >> 1);
                r2 = -r3;
                int r5 = r1 + (r9 >> 2);
                r3 = -r1;
                r3 = r9 + (r3 >> 2);
                r1 = r2 + (r7 >> 2);
                r7 -= (r2 >> 2);
                r0 += r7;
                r7 = r0 - r7 * 2;
                r9 = r10 + r5;
                r10 -= r5;
                r2 = r4 + r3;
                r5 = r4 - r3;
                r3 = r6 + r1;
                r4 = r6 - r1;
                r1 = r9;
                r6 = r10;
                Dst[Offset + 0] = MinMaxTable[0x40 + Dst[Offset + 0] + (r0 >> 6)];
                Dst[Offset + 1] = MinMaxTable[0x40 + Dst[Offset + 1] + (r1 >> 6)];
                Dst[Offset + 2] = MinMaxTable[0x40 + Dst[Offset + 2] + (r2 >> 6)];
                Dst[Offset + 3] = MinMaxTable[0x40 + Dst[Offset + 3] + (r3 >> 6)];
                Dst[Offset + 4] = MinMaxTable[0x40 + Dst[Offset + 4] + (r4 >> 6)];
                Dst[Offset + 5] = MinMaxTable[0x40 + Dst[Offset + 5] + (r5 >> 6)];
                Dst[Offset + 6] = MinMaxTable[0x40 + Dst[Offset + 6] + (r6 >> 6)];
                Dst[Offset + 7] = MinMaxTable[0x40 + Dst[Offset + 7] + (r7 >> 6)];
                Offset += Stride;
            }
        }

        //sub_118ABC
        private void IDCT3Px8(byte[] Dst, int Offset)
        {
            int r8 = (int)Internal[90];
            int r9 = (int)Internal[91];
            int r10 = (int)Internal[90 + 8];
            r8 += 32;
            int r7 = r9 + (r9 >> 1);
            int r11 = (r7 >> 2);
            int r3 = -r9;
            r3 = r9 + (r3 >> 2);
            int r5 = r9 + (r9 >> 2);
            int r0 = r8 + r7;
            r7 = r8 - r7;
            int r1 = r8 + r5;
            int r6 = r8 - r5;
            int r2 = r8 + r3;
            r5 = r8 - r3;
            r3 = r8 + r11;
            int r4 = r8 - r11;
            Internal[90] = (uint)r0;
            Internal[91] = (uint)r1;
            Internal[92] = (uint)r2;
            Internal[93] = (uint)r3;
            Internal[94] = (uint)r4;
            Internal[95] = (uint)r5;
            Internal[96] = (uint)r6;
            Internal[97] = (uint)r7;
            r7 = r10 + (r10 >> 1);
            r1 = (r7 >> 2);
            r3 = -r10;
            r3 = r10 + (r3 >> 2);
            r5 = r10 + (r10 >> 2);
            int lr = 90;
            for (int i = 0; i < 8; i++)
            {
                r0 = (int)Internal[lr++];
                Dst[Offset + 0] = MinMaxTable[0x40 + Dst[Offset + 0] + ((r0 + r7) >> 6)];
                Dst[Offset + 1] = MinMaxTable[0x40 + Dst[Offset + 1] + ((r0 + r5) >> 6)];
                Dst[Offset + 2] = MinMaxTable[0x40 + Dst[Offset + 2] + ((r0 + r3) >> 6)];
                Dst[Offset + 3] = MinMaxTable[0x40 + Dst[Offset + 3] + ((r0 + r1) >> 6)];
                Dst[Offset + 4] = MinMaxTable[0x40 + Dst[Offset + 4] + ((r0 - r1) >> 6)];
                Dst[Offset + 5] = MinMaxTable[0x40 + Dst[Offset + 5] + ((r0 - r3) >> 6)];
                Dst[Offset + 6] = MinMaxTable[0x40 + Dst[Offset + 6] + ((r0 - r5) >> 6)];
                Dst[Offset + 7] = MinMaxTable[0x40 + Dst[Offset + 7] + ((r0 - r7) >> 6)];
                Offset += Stride;
            }
        }

        //sub_118BE0
        private void IDCT1Px8(byte[] Dst, int Offset)
        {
            int r9 = ((int)Internal[90] + 32) >> 6;
            for (int i = 0; i < 8; i++)
            {
                Dst[Offset + 0] = MinMaxTable[0x40 + Dst[Offset + 0] + r9];
                Dst[Offset + 1] = MinMaxTable[0x40 + Dst[Offset + 1] + r9];
                Dst[Offset + 2] = MinMaxTable[0x40 + Dst[Offset + 2] + r9];
                Dst[Offset + 3] = MinMaxTable[0x40 + Dst[Offset + 3] + r9];
                Dst[Offset + 4] = MinMaxTable[0x40 + Dst[Offset + 4] + r9];
                Dst[Offset + 5] = MinMaxTable[0x40 + Dst[Offset + 5] + r9];
                Dst[Offset + 6] = MinMaxTable[0x40 + Dst[Offset + 6] + r9];
                Dst[Offset + 7] = MinMaxTable[0x40 + Dst[Offset + 7] + r9];
                Offset += Stride;
            }
        }

        //loc_118C90
        private void IDCT16Px4(byte[] Dst, int Offset)
        {
            int lr = 90;
            int r11 = lr + 16;
            int r0 = (int)Internal[lr++];
            int r1 = (int)Internal[lr++];
            int r2 = (int)Internal[lr++];
            int r3 = (int)Internal[lr++];
            r0 += 0x20;
            int r12 = 4;
            while (true)
            {
                r0 += r2;
                r2 = r0 - r2 * 2;
                int r8 = (r1 >> 1) - r3;
                int r9 = r1 + (r3 >> 1);
                r3 = r0 - r9;
                r0 += r9;
                r1 = r2 + r8;
                r2 -= r8;
                Internal[r11 + 12] = (uint)r3;
                Internal[r11 + 8] = (uint)r2;
                Internal[r11 + 4] = (uint)r1;
                Internal[r11 + 0] = (uint)r0;
                r11++;
                r12--;
                if (r12 <= 0) break;
                r0 = (int)Internal[lr++];
                r1 = (int)Internal[lr++];
                r2 = (int)Internal[lr++];
                r3 = (int)Internal[lr++];
            }
            r11 -= 4;
            r12 = 4;
            while (true)
            {
                r0 = (int)Internal[r11++];
                r1 = (int)Internal[r11++];
                r2 = (int)Internal[r11++];
                r3 = (int)Internal[r11++];
                r0 += r2;
                r2 = r0 - r2 * 2;
                int r9 = (r1 >> 1) - r3;
                int r10 = r1 + (r3 >> 1);
                r3 = r0 - r10;
                r0 += r10;
                r1 = r2 + r9;
                r2 -= r9;
                Dst[Offset + 0] = MinMaxTable[0x40 + Dst[Offset] + (r0 >> 6)];
                Dst[Offset + 1] = MinMaxTable[0x40 + Dst[Offset + 1] + (r1 >> 6)];
                Dst[Offset + 2] = MinMaxTable[0x40 + Dst[Offset + 2] + (r2 >> 6)];
                Dst[Offset + 3] = MinMaxTable[0x40 + Dst[Offset + 3] + (r3 >> 6)];
                Offset += Stride;
                r12--;
                if (r12 <= 0) break;
            }
        }

        //loc_118D68
        private void IDCT1Px4(byte[] Dst, int Offset)
        {
            int r9 = ((int)Internal[90] + 32) >> 6;
            for (int i = 0; i < 4; i++)
            {
                Dst[Offset + 0] = MinMaxTable[0x40 + Dst[Offset] + r9];
                Dst[Offset + 1] = MinMaxTable[0x40 + Dst[Offset + 1] + r9];
                Dst[Offset + 2] = MinMaxTable[0x40 + Dst[Offset + 2] + r9];
                Dst[Offset + 3] = MinMaxTable[0x40 + Dst[Offset + 3] + r9];
                Offset += Stride;
            }
        }

        private byte[] byte_118DD4 =
        {
            0x14, 0x13, 0x13, 0x19, 0x12, 0x19, 0x13, 0x18, 0x18, 0x13, 0x14, 0x12,
            0x20, 0x12, 0x14, 0x13, 0x13, 0x18, 0x18, 0x13, 0x13, 0x19, 0x12, 0x19,
            0x12, 0x19, 0x12, 0x19, 0x13, 0x18, 0x18, 0x13, 0x13, 0x18, 0x18, 0x13,
            0x12, 0x20, 0x12, 0x14, 0x12, 0x20, 0x12, 0x18, 0x18, 0x13, 0x13, 0x18,
            0x18, 0x12, 0x19, 0x12, 0x19, 0x12, 0x13, 0x18, 0x18, 0x13, 0x12, 0x20,
            0x12, 0x18, 0x18, 0x12, 0x16, 0x15, 0x15, 0x1C, 0x13, 0x1C, 0x15, 0x1A,
            0x1A, 0x15, 0x16, 0x13, 0x23, 0x13, 0x16, 0x15, 0x15, 0x1A, 0x1A, 0x15,
            0x15, 0x1C, 0x13, 0x1C, 0x13, 0x1C, 0x13, 0x1C, 0x15, 0x1A, 0x1A, 0x15,
            0x15, 0x1A, 0x1A, 0x15, 0x13, 0x23, 0x13, 0x16, 0x13, 0x23, 0x13, 0x1A,
            0x1A, 0x15, 0x15, 0x1A, 0x1A, 0x13, 0x1C, 0x13, 0x1C, 0x13, 0x15, 0x1A,
            0x1A, 0x15, 0x13, 0x23, 0x13, 0x1A, 0x1A, 0x13, 0x1A, 0x18, 0x18, 0x21,
            0x17, 0x21, 0x18, 0x1F, 0x1F, 0x18, 0x1A, 0x17, 0x2A, 0x17, 0x1A, 0x18,
            0x18, 0x1F, 0x1F, 0x18, 0x18, 0x21, 0x17, 0x21, 0x17, 0x21, 0x17, 0x21,
            0x18, 0x1F, 0x1F, 0x18, 0x18, 0x1F, 0x1F, 0x18, 0x17, 0x2A, 0x17, 0x1A,
            0x17, 0x2A, 0x17, 0x1F, 0x1F, 0x18, 0x18, 0x1F, 0x1F, 0x17, 0x21, 0x17,
            0x21, 0x17, 0x18, 0x1F, 0x1F, 0x18, 0x17, 0x2A, 0x17, 0x1F, 0x1F, 0x17,
            0x1C, 0x1A, 0x1A, 0x23, 0x19, 0x23, 0x1A, 0x21, 0x21, 0x1A, 0x1C, 0x19,
            0x2D, 0x19, 0x1C, 0x1A, 0x1A, 0x21, 0x21, 0x1A, 0x1A, 0x23, 0x19, 0x23,
            0x19, 0x23, 0x19, 0x23, 0x1A, 0x21, 0x21, 0x1A, 0x1A, 0x21, 0x21, 0x1A,
            0x19, 0x2D, 0x19, 0x1C, 0x19, 0x2D, 0x19, 0x21, 0x21, 0x1A, 0x1A, 0x21,
            0x21, 0x19, 0x23, 0x19, 0x23, 0x19, 0x1A, 0x21, 0x21, 0x1A, 0x19, 0x2D,
            0x19, 0x21, 0x21, 0x19, 0x20, 0x1E, 0x1E, 0x28, 0x1C, 0x28, 0x1E, 0x26,
            0x26, 0x1E, 0x20, 0x1C, 0x33, 0x1C, 0x20, 0x1E, 0x1E, 0x26, 0x26, 0x1E,
            0x1E, 0x28, 0x1C, 0x28, 0x1C, 0x28, 0x1C, 0x28, 0x1E, 0x26, 0x26, 0x1E,
            0x1E, 0x26, 0x26, 0x1E, 0x1C, 0x33, 0x1C, 0x20, 0x1C, 0x33, 0x1C, 0x26,
            0x26, 0x1E, 0x1E, 0x26, 0x26, 0x1C, 0x28, 0x1C, 0x28, 0x1C, 0x1E, 0x26,
            0x26, 0x1E, 0x1C, 0x33, 0x1C, 0x26, 0x26, 0x1C, 0x24, 0x22, 0x22, 0x2E,
            0x20, 0x2E, 0x22, 0x2B, 0x2B, 0x22, 0x24, 0x20, 0x3A, 0x20, 0x24, 0x22,
            0x22, 0x2B, 0x2B, 0x22, 0x22, 0x2E, 0x20, 0x2E, 0x20, 0x2E, 0x20, 0x2E,
            0x22, 0x2B, 0x2B, 0x22, 0x22, 0x2B, 0x2B, 0x22, 0x20, 0x3A, 0x20, 0x24,
            0x20, 0x3A, 0x20, 0x2B, 0x2B, 0x22, 0x22, 0x2B, 0x2B, 0x20, 0x2E, 0x20,
            0x2E, 0x20, 0x22, 0x2B, 0x2B, 0x22, 0x20, 0x3A, 0x20, 0x2B, 0x2B, 0x20
        };

        private byte[] ZigZagTable8x8 =
        {
            0x00, 0x01, 0x08, 0x10, 0x09, 0x02, 0x03, 0x0A, 0x11, 0x18, 0x20, 0x19,
            0x12, 0x0B, 0x04, 0x05, 0x0C, 0x13, 0x1A, 0x21, 0x28, 0x30, 0x29, 0x22,
            0x1B, 0x14, 0x0D, 0x06, 0x07, 0x0E, 0x15, 0x1C, 0x23, 0x2A, 0x31, 0x38,
            0x39, 0x32, 0x2B, 0x24, 0x1D, 0x16, 0x0F, 0x17, 0x1E, 0x25, 0x2C, 0x33,
            0x3A, 0x3B, 0x34, 0x2D, 0x26, 0x1F, 0x27, 0x2E, 0x35, 0x3C, 0x3D, 0x36,
            0x2F, 0x37, 0x3E, 0x3F
        };

        private byte[] byte_118F94 =
        {
            0x0A, 0x0D, 0x0D, 0x0A, 0x10, 0x0A, 0x0D, 0x0D, 0x0D, 0x0D, 0x10, 0x0A,
            0x10, 0x0D, 0x0D, 0x10, 0x0B, 0x0E, 0x0E, 0x0B, 0x12, 0x0B, 0x0E, 0x0E,
            0x0E, 0x0E, 0x12, 0x0B, 0x12, 0x0E, 0x0E, 0x12, 0x0D, 0x10, 0x10, 0x0D,
            0x14, 0x0D, 0x10, 0x10, 0x10, 0x10, 0x14, 0x0D, 0x14, 0x10, 0x10, 0x14,
            0x0E, 0x12, 0x12, 0x0E, 0x17, 0x0E, 0x12, 0x12, 0x12, 0x12, 0x17, 0x0E,
            0x17, 0x12, 0x12, 0x17, 0x10, 0x14, 0x14, 0x10, 0x19, 0x10, 0x14, 0x14,
            0x14, 0x14, 0x19, 0x10, 0x19, 0x14, 0x14, 0x19, 0x12, 0x17, 0x17, 0x12,
            0x1D, 0x12, 0x17, 0x17, 0x17, 0x17, 0x1D, 0x12, 0x1D, 0x17, 0x17, 0x1D
        };

        private byte[] ZigZagTable4x4 =
        {
            0x00, 0x04, 0x01, 0x02, 0x05, 0x08, 0x0C, 0x09, 0x06, 0x03, 0x07, 0x0A,
            0x0D, 0x0E, 0x0B, 0x0F
        };

        private byte[] byte_119004 =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
            0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05,
            0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x08, 0x08, 0x08, 0x08, 0x08, 0x08
        };

        private byte[] byte_11903A =
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05
        };


        //sub_119070
        private unsafe void SetupQuantizationTables(uint quantizer)
        {
            Quantizer = quantizer;
            int r6 = byte_119004[quantizer] + 8;
            int r5 = byte_11903A[quantizer];
            int r4 = r5 << 4;
            int zigzagoffset = 0;
            int internaldataoffset = 74;
            for (int i = 0; i < 16; i++)
            {
                Internal[internaldataoffset++] =
                    (uint)ZigZagTable4x4[zigzagoffset++] |
                    ((uint)byte_118F94[r4++] << r6);
            }
            r6 -= 2;
            r4 = r5 << 6;
            zigzagoffset = 0;
            internaldataoffset = 10;
            for (int i = 0; i < 64; i++)
            {
                Internal[internaldataoffset++] =
                   (uint)ZigZagTable8x8[zigzagoffset++] |
                   ((uint)byte_118DD4[r4++] << r6);
            }
            fixed (uint* InternalPtr = &Internal[0])
            {
                byte* InternalByte = (byte*)InternalPtr;
                InternalByte[1] = 9;
                InternalByte[2] = 9;
                InternalByte[3] = 9;
                InternalByte[4] = 9;
                InternalByte[8] = 9;
                InternalByte[0x10] = 9;
                InternalByte[0x18] = 9;
                InternalByte[0x20] = 9;
            }
        }

        private static int CLZ(uint value)
        {
            int leadingZeros = 0;
            while (value != 0)
            {
                value = value >> 1;
                leadingZeros++;
            }

            return (32 - leadingZeros);
        }
    }
}

