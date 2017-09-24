using System.Collections;
using System;
using System.Collections.Specialized;
using System.Drawing;

namespace Cetera.Image.PVRTC
{
    public class PvrtcPacket
    {

        public static readonly byte[][] BILINEAR_FACTORS =
        {
        new byte[] { 4, 4, 4, 4 },
        new byte[] { 2, 6, 2, 6 },
        new byte[] { 8, 0, 8, 0 },
        new byte[] { 6, 2, 6, 2 },

        new byte[] { 2, 2, 6, 6 },
        new byte[] { 1, 3, 3, 9 },
        new byte[] { 4, 0, 12, 0 },
        new byte[] { 3, 1, 9, 3 },

        new byte[] { 8, 8, 0, 0 },
        new byte[] { 4, 12, 0, 0 },
        new byte[] { 16, 0, 0, 0 },
        new byte[] { 12, 4, 0, 0 },

        new byte[] { 6, 6, 2, 2 },
        new byte[] { 3, 9, 1, 3 },
        new byte[] { 12, 0, 4, 0 },
        new byte[] { 9, 3, 3, 1 },
    };

        public static readonly byte[][] WEIGHTS =
        {
        // Weights for Mode=0
        new byte[] { 8, 0, 8, 0 },
        new byte[] { 5, 3, 5, 3 },
        new byte[] { 3, 5, 3, 5 },
        new byte[] { 0, 8, 0, 8 },
        
        // Weights for Mode=1
        new byte[] { 8, 0, 8, 0 },
        new byte[] { 4, 4, 4, 4 },
        new byte[] { 4, 4, 0, 0 },
        new byte[] { 0, 8, 0, 8 },
    };

        uint modulationData;
        bool usePunchthroughAlpha;
        bool colorAIsOpaque;
        bool colorBIsOpaque;
        uint colorA;
        uint colorB;

        public PvrtcPacket()
        {
            // Default constructor doesn't do anything
        }

        public uint GetModulationData()
        {
            return this.modulationData;
        }

        public void SetModulationData(uint newValue)
        {
            this.modulationData = newValue;
        }

        public int GetPunchthroughAlpha()
        {
            if (this.usePunchthroughAlpha)
            {
                return 1;
            }
            return 0;
        }

        public void SetPunchthroughAlpha(bool newValue)
        {
            this.usePunchthroughAlpha = newValue;
        }

        public bool GetColorAIsOpaque()
        {
            return this.colorAIsOpaque;
        }

        public bool GetColorBIsOpaque()
        {
            return this.colorBIsOpaque;
        }

        public uint GetColorA()
        {
            return this.colorA;
        }

        public uint GetColorB()
        {
            return this.colorB;
        }

        // Gets A color as RGB int
        public Vector3Int GetColorRgbA()
        {
            if (this.colorAIsOpaque)
            {
                byte r = (byte)(this.colorA >> 9);
                byte g = (byte)(this.colorA >> 4 & 0x1f);
                byte b = (byte)(this.colorA & 0xf);

                return new Vector3Int(BitScale.BITSCALE_5_TO_8[r],
                                     BitScale.BITSCALE_5_TO_8[g],
                                     BitScale.BITSCALE_4_TO_8[b]);
            }
            else
            {
                byte r = (byte)((this.colorA >> 7) & 0xf);
                byte g = (byte)((this.colorA >> 3) & 0xf);
                byte b = (byte)(this.colorA & 7);

                return new Vector3Int(BitScale.BITSCALE_4_TO_8[r],
                                     BitScale.BITSCALE_4_TO_8[g],
                                     BitScale.BITSCALE_3_TO_8[b]);
            }
        }

        // Gets B color as RGB int
        public Vector3Int GetColorRgbB()
        {
            if (this.colorBIsOpaque)
            {
                byte r = (byte)(this.colorB >> 10);
                byte g = (byte)(this.colorB >> 5 & 0x1f);
                byte b = (byte)(this.colorB & 0x1f);

                return new Vector3Int(BitScale.BITSCALE_5_TO_8[r],
                                     BitScale.BITSCALE_5_TO_8[g],
                                     BitScale.BITSCALE_5_TO_8[b]);
            }
            else
            {
                byte r = (byte)(this.colorB >> 8 & 0xf);
                byte g = (byte)(this.colorB >> 4 & 0xf);
                byte b = (byte)(this.colorB & 0xf);

                return new Vector3Int(BitScale.BITSCALE_4_TO_8[r],
                                     BitScale.BITSCALE_4_TO_8[g],
                                     BitScale.BITSCALE_4_TO_8[b]);
            }
        }

        // Gets A color as RGBA int
        public Vector4Int GetColorRgbaA()
        {
            if (this.colorAIsOpaque)
            {
                byte r = (byte)(this.colorA >> 9);
                byte g = (byte)(this.colorA >> 4 & 0x1f);
                byte b = (byte)(this.colorA & 0xf);

                return new Vector4Int(BitScale.BITSCALE_5_TO_8[r],
                                      BitScale.BITSCALE_5_TO_8[g],
                                      BitScale.BITSCALE_4_TO_8[b],
                                      255);
            }
            else
            {
                byte a = (byte)(this.colorA >> 11 & 7);
                byte r = (byte)(this.colorA >> 7 & 0xf);
                byte g = (byte)(this.colorA >> 3 & 0xf);
                byte b = (byte)(this.colorA & 7);

                return new Vector4Int(BitScale.BITSCALE_4_TO_8[r],
                                      BitScale.BITSCALE_4_TO_8[g],
                                      BitScale.BITSCALE_3_TO_8[b],
                                      BitScale.BITSCALE_3_TO_8[a]);
            }
        }

        // Gets B color as RGBA int
        public Vector4Int GetColorRgbaB()
        {
            if (this.colorBIsOpaque)
            {
                byte r = (byte)(this.colorB >> 10);
                byte g = (byte)(this.colorB >> 5 & 0x1f);
                byte b = (byte)(this.colorB & 0x1f);

                return new Vector4Int(BitScale.BITSCALE_5_TO_8[r],
                                      BitScale.BITSCALE_5_TO_8[g],
                                      BitScale.BITSCALE_5_TO_8[b],
                                      255);
            }
            else
            {
                byte a = (byte)(this.colorB >> 12 & 7);
                byte r = (byte)(this.colorB >> 8 & 0xf);
                byte g = (byte)(this.colorB >> 4 & 0xf);
                byte b = (byte)(this.colorB & 0xf);

                return new Vector4Int(BitScale.BITSCALE_4_TO_8[r],
                                      BitScale.BITSCALE_4_TO_8[g],
                                      BitScale.BITSCALE_4_TO_8[b],
                                      BitScale.BITSCALE_3_TO_8[a]);
            }
        }

        // Set color A, NO alpha
        public void SetColorA(byte rr, byte gg, byte bb)
        {
            int r = BitScale.BITSCALE_8_TO_5_FLOOR[rr];
            int g = BitScale.BITSCALE_8_TO_5_FLOOR[gg];
            int b = BitScale.BITSCALE_8_TO_4_FLOOR[bb];

            this.colorA = (uint)(r << 9 | g << 4 | b);
            this.colorAIsOpaque = true;
        }

        // Set color B, NO alpha
        public void SetColorB(byte rr, byte gg, byte bb)
        {
            int r = BitScale.BITSCALE_8_TO_5_CEIL[rr];
            int g = BitScale.BITSCALE_8_TO_5_CEIL[gg];
            int b = BitScale.BITSCALE_8_TO_5_CEIL[bb];

            this.colorB = (uint)(r << 10 | g << 5 | b);
            this.colorBIsOpaque = true;
        }

        // Set color A with alpha
        public void SetColorA(Color c)
        {
            int a = BitScale.BITSCALE_8_TO_3_FLOOR[c.A];
            if (a == 7)
            {
                int r = BitScale.BITSCALE_8_TO_5_FLOOR[c.R];
                int g = BitScale.BITSCALE_8_TO_5_FLOOR[c.G];
                int b = BitScale.BITSCALE_8_TO_4_FLOOR[c.B];

                this.colorA = (uint)(r << 9 | g << 4 | b);
                this.colorAIsOpaque = true;
            }
            else
            {
                int r = BitScale.BITSCALE_8_TO_4_FLOOR[c.R];
                int g = BitScale.BITSCALE_8_TO_4_FLOOR[c.G];
                int b = BitScale.BITSCALE_8_TO_3_FLOOR[c.B];

                this.colorA = (uint)(a << 11 | r << 7 | g << 3 | b);
                this.colorAIsOpaque = false;
            }
        }

        // Set color B with alpha
        public void SetColorB(Color c)
        {
            int a = BitScale.BITSCALE_8_TO_3_CEIL[c.A];
            if (a == 7)
            {
                int r = BitScale.BITSCALE_8_TO_5_CEIL[c.R];
                int g = BitScale.BITSCALE_8_TO_5_CEIL[c.G];
                int b = BitScale.BITSCALE_8_TO_5_CEIL[c.B];

                this.colorB = (uint)(r << 10 | g << 5 | b);
                this.colorBIsOpaque = true;
            }
            else
            {
                int r = BitScale.BITSCALE_8_TO_4_CEIL[c.R];
                int g = BitScale.BITSCALE_8_TO_4_CEIL[c.G];
                int b = BitScale.BITSCALE_8_TO_4_CEIL[c.B];

                this.colorB = (uint)(a << 12 | r << 8 | g << 4 | b);
                this.colorBIsOpaque = false;
            }
        }

        public byte[] GetAsByteArray()
        {
            byte[] returnValue = new byte[8];
            byte[] modulationDataByteArray = BitConverter.GetBytes(this.modulationData);

            BitVector32 tempBitVector = new BitVector32(0);
            int currentIndex = 0;
            tempBitVector[1 << currentIndex] = this.usePunchthroughAlpha;
            currentIndex++;

            BitVector32 tempBitVectorColorA = new BitVector32((int)this.colorA);
            for (int i = 0; i < 14; i++)
            {
                tempBitVector[1 << currentIndex] = tempBitVectorColorA[1 << i];
                currentIndex++;
            }

            tempBitVector[1 << currentIndex] = this.colorAIsOpaque;
            currentIndex++;

            BitVector32 tempBitVectorColorB = new BitVector32((int)this.colorB);
            for (int i = 0; i < 15; i++)
            {
                tempBitVector[1 << currentIndex] = tempBitVectorColorB[1 << i];
                currentIndex++;
            }

            tempBitVector[1 << currentIndex] = this.colorBIsOpaque;
            currentIndex++;

            Buffer.BlockCopy(modulationDataByteArray, 0, returnValue, 0, 4);
            byte[] otherDataByteArray = BitConverter.GetBytes(tempBitVector.Data);
            Buffer.BlockCopy(otherDataByteArray, 0, returnValue, 4, 4);

            return returnValue;
        }

        public void InitFromBytes(byte[] data)
        {
            byte[] modulationDataByteArray = new byte[4];
            byte[] otherDataByteArray = new byte[4];
            Buffer.BlockCopy(data, 0, modulationDataByteArray, 0, 4);
            Buffer.BlockCopy(data, 4, otherDataByteArray, 0, 4);

            this.modulationData = BitConverter.ToUInt32(modulationDataByteArray, 0);
            BitVector32 tempBitVector = new BitVector32(BitConverter.ToInt32(otherDataByteArray, 0));

            BitVector32.Section punchthroughAlphaSection = BitVector32.CreateSection(1);
            BitVector32.Section colorASection = BitVector32.CreateSection(16383 /*(1 << 14) - 1*/, punchthroughAlphaSection);
            BitVector32.Section colorAIsOpaque = BitVector32.CreateSection(1, colorASection);
            BitVector32.Section colorBSection = BitVector32.CreateSection(32767 /*(1 << 15) - 1*/, colorAIsOpaque);
            BitVector32.Section colorBIsOpaque = BitVector32.CreateSection(1, colorBSection);

            if (tempBitVector[punchthroughAlphaSection] == 1)
            {
                this.usePunchthroughAlpha = true;
            }

            this.colorA = (uint)tempBitVector[colorASection];

            if (tempBitVector[colorAIsOpaque] == 1)
            {
                this.colorAIsOpaque = true;
            }

            this.colorB = (uint)tempBitVector[colorBSection];

            if (tempBitVector[colorBIsOpaque] == 1)
            {
                this.colorBIsOpaque = true;
            }
        }
    }
}
