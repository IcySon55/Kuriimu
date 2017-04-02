using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cetera.IO;
using System.IO;

namespace Cetera.Compression
{
    public sealed class RevLZ77
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CompFooter
        {
            public uint bufferTopAndBottom;
            public uint originalBottom;
        }

        public static byte[] Decompress(byte[] input, uint decompSize)
        {
            bool res = true;
            List<byte> result = new List<byte>();

            if (input.Length >= 8)
            {
                for (int i = 0; i < decompSize; i++) result.Add(0x00);

                using (BinaryReaderX br = new BinaryReaderX(new MemoryStream(input)))
                {
                    br.BaseStream.Position = br.BaseStream.Length - 8; CompFooter pCompFooter = br.ReadStruct<CompFooter>();
                    uint uTop = pCompFooter.bufferTopAndBottom & 0xFFFFFF;
                    uint uBottom = pCompFooter.bufferTopAndBottom >> 24 & 0xFF;
                    if (uBottom >= 8 && uBottom <= 8 + 3 && uTop >= uBottom && uTop <= br.BaseStream.Length && decompSize >= br.BaseStream.Length + pCompFooter.originalBottom)
                    {
                        decompSize = (uint)br.BaseStream.Length + pCompFooter.originalBottom;
                        br.BaseStream.Position = 0; for (int i = 0; i < br.BaseStream.Length; i++) result[i] = br.ReadByte();
                        uint destPos = decompSize;
                        uint srcPos = (uint)br.BaseStream.Length - uBottom;
                        uint end = (uint)br.BaseStream.Length - uTop;

                        while (srcPos - end > 0)
                        {
                            br.BaseStream.Position = --srcPos;
                            byte uFlag = br.ReadByte();

                            for (int i = 0; i < 8; i++)
                            {
                                if ((uFlag << i & 0x80) == 0)
                                {
                                    if (destPos - end < 1 || srcPos - end < 1)
                                    {
                                        res = false;
                                        break;
                                    }
                                    br.BaseStream.Position = --srcPos;
                                    byte get = br.ReadByte();
                                    result[(int)--destPos] = get;
                                }
                                else
                                {
                                    if (srcPos - end < 2)
                                    {
                                        res = false;
                                        break;
                                    }
                                    br.BaseStream.Position = --srcPos;
                                    int nSize = br.ReadByte();

                                    br.BaseStream.Position = --srcPos;
                                    int nOffset = (((nSize & 0x0F) << 8) | br.ReadByte()) + 3;

                                    nSize = (nSize >> 4 & 0x0F) + 3;

                                    if (nSize > destPos - end)
                                    {
                                        res = false;
                                        break;
                                    }

                                    uint dataPos = destPos + (uint)nOffset;
                                    if (dataPos > decompSize)
                                    {
                                        res = false;
                                        break;
                                    }

                                    for (int j = 0; j < nSize; j++)
                                    {
                                        byte get = result[(int)--dataPos];
                                        result[(int)--destPos] = get;
                                    }
                                }

                                if (srcPos - end <= 0)
                                {
                                    break;
                                }
                            }

                            if (res == false)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        res = false;
                    }
                }
            }
            else
            {
                res = false;
            }

            if (res)
            {
                return result.ToArray();
            }
            else
            {
                throw new Exception("Decompression failed!");
            }
        }
    }
}
