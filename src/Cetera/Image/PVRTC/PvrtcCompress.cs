using System.Collections;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Cetera.Image.PVRTC
{
    public class PvrtcCompress
    {

        private static int GetMortonNumber(int x, int y)
        {
            return MortonTable.MORTON_TABLE[x >> 8] << 17 | MortonTable.MORTON_TABLE[y >> 8] << 16 | MortonTable.MORTON_TABLE[x & 0xFF] << 1 | MortonTable.MORTON_TABLE[y & 0xFF];
        }

        private static void GetMinMaxColors(Bitmap bitmap, int startX, int startY, ref Color minColor, ref Color maxColor)
        {
            minColor = Color.White; // white is same as all 255
            maxColor = Color.Black; // clear is same as all 0

            for (int x = startX; x < startX + 4; x++)
            {
                for (int y = startY; y < startY + 4; y++)
                {
                    Color currentColor = bitmap.GetPixel(x, y);
                    if (currentColor.R < minColor.R) { minColor = Color.FromArgb(minColor.A, currentColor.R, minColor.G, minColor.B); }
                    if (currentColor.G < minColor.G) { minColor = Color.FromArgb(minColor.A, minColor.R, currentColor.G, minColor.B); }
                    if (currentColor.B < minColor.B) { minColor = Color.FromArgb(minColor.A, minColor.R, minColor.G, currentColor.B); }
                    if (currentColor.R > maxColor.R) { maxColor = Color.FromArgb(maxColor.A, currentColor.R, maxColor.G, maxColor.B); }
                    if (currentColor.G > maxColor.G) { maxColor = Color.FromArgb(maxColor.A, maxColor.R, currentColor.G, maxColor.B); }
                    if (currentColor.B > maxColor.B) { maxColor = Color.FromArgb(maxColor.A, maxColor.R, maxColor.G, currentColor.B); }
                }
            }
            /*
            byte[] inset = new byte[3];
            inset[0] = (byte)(( maxColor.r - minColor.r ) >> this.insetShift); 
            inset[1] = (byte)(( maxColor.g - minColor.g ) >> this.insetShift); 
            inset[2] = (byte)(( maxColor.b - minColor.b ) >> this.insetShift);

            minColor.r = (byte)(( minColor.r + inset[0] <= 255 ) ? minColor.r + inset[0] : 255); 
            minColor.g = (byte)(( minColor.g + inset[1] <= 255 ) ? minColor.g + inset[1] : 255); 
            minColor.b = (byte)(( minColor.b + inset[2] <= 255 ) ? minColor.b + inset[2] : 255);

            maxColor.r = (byte)(( maxColor.r >= inset[0] ) ? maxColor.r - inset[0] : 0); 
            maxColor.g = (byte)(( maxColor.g >= inset[1] ) ? maxColor.g - inset[1] : 0); 
            maxColor.b = (byte)(( maxColor.b >= inset[2] ) ? maxColor.b - inset[2] : 0);
            */
        }

        private static void GetMinMaxColorsWithAlpha(List<Color> colorArray, int size, int startX, int startY, ref Color minColor, ref Color maxColor)
        {
            minColor = Color.White; // white is same as all 255
            maxColor = Color.Black; // clear is same as all 0

            for (int x = startX; x < startX + 4; x++)
            {
                for (int y = startY; y < startY + 4; y++)
                {
                    Color currentColor = colorArray[x + y * size];
                    if (currentColor.A < minColor.A) { minColor = Color.FromArgb(currentColor.A, minColor.R, minColor.G, minColor.B); }
                    if (currentColor.R < minColor.R) { minColor = Color.FromArgb(minColor.A, currentColor.R, minColor.G, minColor.B); }
                    if (currentColor.G < minColor.G) { minColor = Color.FromArgb(minColor.A, minColor.R, currentColor.G, minColor.B); }
                    if (currentColor.B < minColor.B) { minColor = Color.FromArgb(minColor.A, minColor.R, minColor.G, currentColor.B); }
                    if (currentColor.A < maxColor.A) { maxColor = Color.FromArgb(currentColor.A, maxColor.R, maxColor.G, maxColor.B); }
                    if (currentColor.R > maxColor.R) { maxColor = Color.FromArgb(maxColor.A, currentColor.R, maxColor.G, maxColor.B); }
                    if (currentColor.G > maxColor.G) { maxColor = Color.FromArgb(maxColor.A, maxColor.R, currentColor.G, maxColor.B); }
                    if (currentColor.B > maxColor.B) { maxColor = Color.FromArgb(maxColor.A, maxColor.R, maxColor.G, currentColor.B); }
                }
            }
            /*
            byte[] inset = new byte[4];
            inset[0] = (byte)(( maxColor.r - minColor.r ) >> this.insetShift); 
            inset[1] = (byte)(( maxColor.g - minColor.g ) >> this.insetShift); 
            inset[2] = (byte)(( maxColor.b - minColor.b ) >> this.insetShift);
            inset[3] = (byte)(( maxColor.a - minColor.a ) >> this.insetShift);

            minColor.r = (byte)(( minColor.r + inset[0] <= 255 ) ? minColor.r + inset[0] : 255); 
            minColor.g = (byte)(( minColor.g + inset[1] <= 255 ) ? minColor.g + inset[1] : 255); 
            minColor.b = (byte)(( minColor.b + inset[2] <= 255 ) ? minColor.b + inset[2] : 255);
            minColor.a = (byte)(( minColor.a + inset[3] <= 255 ) ? minColor.a + inset[3] : 255);

            maxColor.r = (byte)(( maxColor.r >= inset[0] ) ? maxColor.r - inset[0] : 0); 
            maxColor.g = (byte)(( maxColor.g >= inset[1] ) ? maxColor.g - inset[1] : 0); 
            maxColor.b = (byte)(( maxColor.b >= inset[2] ) ? maxColor.b - inset[2] : 0);
            maxColor.a = (byte)(( maxColor.a >= inset[3] ) ? maxColor.a - inset[3] : 0);
            */
        }

        public static byte[] EncodeRgba4Bpp(Bitmap bitmap)
        {
            if (bitmap.Height != bitmap.Width) throw new Exception("Texture isn't square!");
            if (!((bitmap.Height & (bitmap.Height - 1)) == 0)) throw new Exception("Texture resolution must be 2^N!");

            int size = bitmap.Width;
            int blocks = size / 4;
            int blockMask = blocks - 1;

            PvrtcPacket[] packets = new PvrtcPacket[blocks * blocks];
            for (int i = 0; i < packets.Length; i++)
            {
                packets[i] = new PvrtcPacket();
            }

            List<Color> tempColor32Array = new List<Color>();
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    tempColor32Array.Add(bitmap.GetPixel(x, y));

            for (int y = 0; y < blocks; ++y)
            {
                for (int x = 0; x < blocks; ++x)
                {
                    Color minColor = Color.White; // white is same as all 255, should be same as Color.max 
                    Color maxColor = Color.Black; // clear is same as all 0, should be same as Color.min
                    GetMinMaxColorsWithAlpha(tempColor32Array, size, 4 * x, 4 * y, ref minColor, ref maxColor);

                    PvrtcPacket packet = packets[GetMortonNumber(x, y)];
                    packet.SetPunchthroughAlpha(false);
                    packet.SetColorA(minColor);
                    packet.SetColorB(maxColor);
                }
            }

            int currentFactorIndex = 0;

            for (int y = 0; y < blocks; ++y)
            {
                for (int x = 0; x < blocks; ++x)
                {
                    currentFactorIndex = 0;

                    uint modulationData = 0;

                    for (int py = 0; py < 4; ++py)
                    {
                        int yOffset = (py < 2) ? -1 : 0;
                        int y0 = (y + yOffset) & blockMask;
                        int y1 = (y0 + 1) & blockMask;

                        for (int px = 0; px < 4; ++px)
                        {
                            int xOffset = (px < 2) ? -1 : 0;
                            int x0 = (x + xOffset) & blockMask;
                            int x1 = (x0 + 1) & blockMask;

                            PvrtcPacket p0 = packets[GetMortonNumber(x0, y0)];
                            PvrtcPacket p1 = packets[GetMortonNumber(x1, y0)];
                            PvrtcPacket p2 = packets[GetMortonNumber(x0, y1)];
                            PvrtcPacket p3 = packets[GetMortonNumber(x1, y1)];

                            byte[] currentFactors = PvrtcPacket.BILINEAR_FACTORS[currentFactorIndex];

                            Vector4Int ca = p0.GetColorRgbaA() * currentFactors[0] +
                                                p1.GetColorRgbaA() * currentFactors[1] +
                                                p2.GetColorRgbaA() * currentFactors[2] +
                                                p3.GetColorRgbaA() * currentFactors[3];

                            Vector4Int cb = p0.GetColorRgbaB() * currentFactors[0] +
                                                p1.GetColorRgbaB() * currentFactors[1] +
                                                p2.GetColorRgbaB() * currentFactors[2] +
                                                p3.GetColorRgbaB() * currentFactors[3];

                            Color pixel = tempColor32Array[4 * x + px + (4 * y + py) * size]; //(Color32)bitmap.GetPixel(4*x + px, 4*y + py);
                            Vector4Int d = cb - ca;
                            Vector4Int p = new Vector4Int(pixel.R * 16, pixel.G * 16, pixel.B * 16, pixel.A * 16);
                            Vector4Int v = p - ca;

                            // PVRTC uses weightings of 0, 3/8, 5/8 and 1
                            // The boundaries for these are 3/16, 1/2 (=8/16), 13/16
                            int projection = (v % d) * 16; //Mathf.RoundToInt(Vector4.Dot(v, d)) * 16;
                            int lengthSquared = d % d; //Mathf.RoundToInt(Vector4.Dot(d,d));
                            if (projection > 3 * lengthSquared) modulationData++;
                            if (projection > 8 * lengthSquared) modulationData++;
                            if (projection > 13 * lengthSquared) modulationData++;

                            modulationData = RotateRight(modulationData, 2);

                            currentFactorIndex++;
                        }
                    }

                    PvrtcPacket packet = packets[GetMortonNumber(x, y)];
                    packet.SetModulationData(modulationData);
                }
            }

            byte[] returnValue = new byte[size * size / 2];

            // Create final byte array from PVRTC packets
            for (int i = 0; i < packets.Length; i++)
            {
                byte[] tempArray = packets[i].GetAsByteArray();
                Buffer.BlockCopy(tempArray, 0, returnValue, 8 * i, 8);
            }

            return returnValue;
        }


        public static byte[] EncodeRgb4Bpp(Bitmap bitmap)
        {
            if (bitmap.Height != bitmap.Width) throw new Exception("Texture isn't square!");
            if (!((bitmap.Height & (bitmap.Height - 1)) == 0)) throw new Exception("Texture resolution must be 2^N!");

            int size = bitmap.Width;
            int blocks = size / 4;
            int blockMask = blocks - 1;

            PvrtcPacket[] packets = new PvrtcPacket[blocks * blocks];
            for (int i = 0; i < packets.Length; i++)
            {
                packets[i] = new PvrtcPacket();
            }

            for (int y = 0; y < blocks; ++y)
            {
                for (int x = 0; x < blocks; ++x)
                {
                    Color minColor = Color.White; // white is same as all 255, should be same as Color.max 
                    Color maxColor = Color.Black; // clear is same as all 0,   should be same as Color.min

                    GetMinMaxColors(bitmap, 4 * x, 4 * y, ref minColor, ref maxColor);

                    PvrtcPacket packet = packets[GetMortonNumber(x, y)];
                    packet.SetPunchthroughAlpha(false);
                    packet.SetColorA(minColor.R, minColor.G, minColor.B);
                    packet.SetColorB(maxColor.R, maxColor.G, maxColor.B);
                }
            }

            int currentFactorIndex = 0;

            for (int y = 0; y < blocks; ++y)
            {
                for (int x = 0; x < blocks; ++x)
                {
                    currentFactorIndex = 0;

                    uint modulationData = 0;

                    for (int py = 0; py < 4; ++py)
                    {
                        int yOffset = (py < 2) ? -1 : 0;
                        int y0 = (y + yOffset) & blockMask;
                        int y1 = (y0 + 1) & blockMask;

                        for (int px = 0; px < 4; ++px)
                        {
                            int xOffset = (px < 2) ? -1 : 0;
                            int x0 = (x + xOffset) & blockMask;
                            int x1 = (x0 + 1) & blockMask;

                            PvrtcPacket p0 = packets[GetMortonNumber(x0, y0)];
                            PvrtcPacket p1 = packets[GetMortonNumber(x1, y0)];
                            PvrtcPacket p2 = packets[GetMortonNumber(x0, y1)];
                            PvrtcPacket p3 = packets[GetMortonNumber(x1, y1)];

                            byte[] currentFactors = PvrtcPacket.BILINEAR_FACTORS[currentFactorIndex];

                            Vector3Int ca = p0.GetColorRgbA() * currentFactors[0] +
                                                p1.GetColorRgbA() * currentFactors[1] +
                                                p2.GetColorRgbA() * currentFactors[2] +
                                                p3.GetColorRgbA() * currentFactors[3];

                            Vector3Int cb = p0.GetColorRgbB() * currentFactors[0] +
                                                p1.GetColorRgbB() * currentFactors[1] +
                                                p2.GetColorRgbB() * currentFactors[2] +
                                                p3.GetColorRgbB() * currentFactors[3];

                            Color pixel = bitmap.GetPixel(4 * x + px, 4 * y + py);

                            Vector3Int d = cb - ca;
                            Vector3Int p = new Vector3Int(pixel.R * 16, pixel.G * 16, pixel.B * 16);
                            Vector3Int v = p - ca;

                            // PVRTC uses weightings of 0, 3/8, 5/8 and 1
                            // The boundaries for these are 3/16, 1/2 (=8/16), 13/16
                            int projection = (v % d) * 16; // Mathf.RoundToInt(Vector3.Dot(v, d)) * 16;
                            int lengthSquared = d % d;//Mathf.RoundToInt(Vector3.Dot(d,d));
                            if (projection > 3 * lengthSquared) modulationData++;
                            if (projection > 8 * lengthSquared) modulationData++;
                            if (projection > 13 * lengthSquared) modulationData++;

                            modulationData = RotateRight(modulationData, 2);

                            currentFactorIndex++;
                        }
                    }

                    PvrtcPacket packet = packets[GetMortonNumber(x, y)];
                    packet.SetModulationData(modulationData);
                }
            }

            byte[] returnValue = new byte[size * size / 2];

            // Create final byte array from PVRTC packets
            for (int i = 0; i < packets.Length; i++)
            {
                byte[] tempArray = packets[i].GetAsByteArray();
                Buffer.BlockCopy(tempArray, 0, returnValue, 8 * i, 8);
            }

            return returnValue;
        }

        private static uint RotateRight(uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
    }
}