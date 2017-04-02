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
		public class SCompressInfo
		{
			public SCompressInfo(uint workPos)
			{
				work = new short[(4098 + 4098 + 256 + 256) * 2];
				WindowLen = 0;
				WindowPos = 0;
				offsetTablePos = (ushort)workPos;
				reversedOffsetTablePos= (ushort)(workPos+4098);
				byteTablePos= (ushort)(workPos+4098+4098);
				endTablePos= (ushort)(workPos+4098+4098+256);

				for (int i=0;i<256;i++)
				{
					work[byteTablePos+i] = -1;
					work[endTablePos+i] = -1;
				}
			}
			public ushort WindowPos;
			public ushort WindowLen;
			public short[] work;
			public ushort offsetTablePos;
			public ushort reversedOffsetTablePos;
			public ushort byteTablePos;
			public ushort endTablePos;
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

		public static byte[] Compress(byte[] input)
		{
			bool res = true;

			using (BinaryReaderX br=new BinaryReaderX(new MemoryStream(input)))
			{
				int compSize = (int)br.BaseStream.Length;
				byte[] result = new byte[br.BaseStream.Length];

				if (br.BaseStream.Length>8)
				{
					uint workPos = 0;

					do
					{
						SCompressInfo info = new SCompressInfo(workPos);
						const int nMaxSize = 0xF + 3;
						uint srcPos = (uint)br.BaseStream.Length;
						uint destPos = (uint)br.BaseStream.Length;

						while (srcPos > 0 && destPos > 0)
						{
							uint pFlag = --destPos;
							result[pFlag] = 0;

							for(int i=0;i<8;i++)
							{
								int nOffset = 0;
								int nSize = Search(br,info,srcPos,nOffset,Math.Min(Math.Min(nMaxSize,(int)srcPos),(int)br.BaseStream.Length-(int)srcPos)); //could cause problems

								if (nSize<3)
								{
									if (destPos<1)
									{
										res = false;
										break;
									}
									Slide(br, info, srcPos, 1);
									br.BaseStream.Position = --srcPos;
									result[--destPos] = br.ReadByte();
								} else
								{
									if (destPos<2)
									{
										res = false;
										break;
									}

									result[pFlag] |= (byte)(0x80 >> i);
									Slide(br,info,srcPos,nSize);
									srcPos -= (uint)nSize;
									nSize -= 3;
									result[--destPos] = (byte)((nSize << 4 & 0xF0) | ((nOffset - 3) >> 8 & 0x0F));
									result[--destPos] = (byte)((nOffset - 3) & 0xFF);
								}

								if (srcPos<=0)
								{
									break;
								}
							}

							if (res==false)
							{
								break;
							}
						}

						if (res==false)
						{
							break;
						}
						compSize = (int)(br.BaseStream.Length - destPos);
					} while (false);
				} else
				{
					res = false;
				}

				if (res)
				{
					uint uOriginSize = (uint)br.BaseStream.Length;
					uint compressBufferPos = (uint)(br.BaseStream.Length - compSize);
					uint compressBufferSize = (uint)compSize;
					uint uOriginSafe = 0;
					uint uCompressSafe = 0;
					bool bOver = false;

					while(uOriginSize>0)
					{
						byte uFlag = result[compressBufferPos + (--compressBufferSize)];

						for (int i=0;i<8;i++)
						{
							if ((uFlag<<i&0x80)==0)
							{
								compressBufferSize--;
								uOriginSize--;
							} else
							{
								int nSize = (result[compressBufferPos + (--compressBufferSize)] >> 4 & 0x0F) + 3;
								compressBufferSize--;
								uOriginSize -= (uint)nSize;

								if (uOriginSize<compressBufferSize)
								{
									uOriginSafe = uOriginSize;
									uCompressSafe = compressBufferSize;
									bOver = true;
									break;
								}
							}

							if (uOriginSize<=0)
							{
								break;
							}
						}

						if (bOver)
						{
							break;
						}
					}

					uint uCompressedSize = (uint)compSize - uCompressSafe;
					uint uPadOffset = uOriginSafe + uCompressedSize;
					uint uCompFooterOffset = (uPadOffset + 4 - 1) / 4 * 4;
					compSize = (int)uCompFooterOffset + 8;
					uint uTop = (uint)compSize - uOriginSafe;
					uint uBottom = (uint)compSize - uPadOffset;

					if (compSize>=br.BaseStream.Length || uTop>0xFFFFFF)
					{
						res = false;
					} else
					{
						//memcpy
						br.BaseStream.Position = 0;
						for (int i = 0; i < uOriginSafe; i++) result[i] = br.ReadByte();

						//memmove
						byte[] tmp = new byte[uCompressedSize];
						for (int i = (int)(compressBufferPos + uCompressSafe); i < compressBufferPos + uCompressSafe + uCompressedSize; i++) tmp[i - (compressBufferPos + uCompressSafe)] = result[i];
						for (int i = (int)uOriginSafe; i < tmp.Length; i++) result[i] = tmp[i- uOriginSafe];

						//memset
						for (int i = (int)uPadOffset; i < uPadOffset + (uCompFooterOffset - uPadOffset); i++) result[i] = 0xFF;

						CompFooter pCompFooter = new CompFooter();
						pCompFooter.bufferTopAndBottom = (uint)(result[uCompFooterOffset+3] << 24) + (uint)(result[uCompFooterOffset + 2] << 16) + (uint)(result[uCompFooterOffset + 1] << 8) + result[uCompFooterOffset];
						pCompFooter.originalBottom= (uint)(result[uCompFooterOffset + 7] << 24) + (uint)(result[uCompFooterOffset + 6] << 16) + (uint)(result[uCompFooterOffset + 5] << 8) + result[uCompFooterOffset + 4];

						pCompFooter.bufferTopAndBottom = uTop | (uBottom << 24);
						pCompFooter.originalBottom = (uint)(br.BaseStream.Length - compSize);
					}
				}

				return result.ToArray();
			}
		}

		public static int Search(BinaryReaderX br,SCompressInfo info, uint srcPos, int nOffset, int nMaxSize)
		{
			if (nMaxSize<3)
			{
				return 0;
			}

			uint searchPos = 0;
			int nSize = 2;
			ushort uWindowPos = info.WindowPos;
			ushort uWindowLen = info.WindowLen;

			br.BaseStream.Position = srcPos - 1;
			for (short inOffset=info.work[info.endTablePos+br.ReadByte()];inOffset!=-1;inOffset=info.work[info.reversedOffsetTablePos+inOffset])
			{
				if (inOffset<uWindowPos)
				{
					searchPos = srcPos + uWindowPos - (uint)inOffset;
				} else
				{
					searchPos = srcPos + uWindowPos + uWindowLen - (uint)inOffset;
				}

				if (searchPos-srcPos<3)
				{
					continue;
				}

				br.BaseStream.Position = searchPos-3;
				byte i1 = br.ReadByte();
				byte i2 = br.ReadByte();
				br.BaseStream.Position = srcPos - 3;
				byte i3 = br.ReadByte();
				byte i4 = br.ReadByte();
				if (i2!=i4||i1!=i3)
				{
					continue;
				}

				int inMaxSize = Math.Min(nMaxSize,(int)(searchPos-srcPos));
				int nCurrentSize = 3;

				br.BaseStream.Position = searchPos - nCurrentSize - 1;
				byte i5 = br.ReadByte();
				br.BaseStream.Position = srcPos - nCurrentSize - 1;
				byte i6 = br.ReadByte();
				while (nCurrentSize<inMaxSize && i5==i6)
				{
					nCurrentSize++;
					br.BaseStream.Position = searchPos - nCurrentSize - 1;
					i5 = br.ReadByte();
					br.BaseStream.Position = srcPos - nCurrentSize - 1;
					i6 = br.ReadByte();
				}

				if (nCurrentSize>nSize)
				{
					nSize = nCurrentSize;
					nOffset = (int)(searchPos - srcPos);
					if (nSize==nMaxSize)
					{
						break;
					}
				}
			}

			if (nSize<3)
			{
				return 0;
			}

			return nSize;
		}

		public static void Slide(BinaryReaderX br, SCompressInfo info, uint srcPos, int nSize)
		{
			for (int i=0;i<nSize;i++)
			{
				SlideByte(br, info, srcPos--);
			}
		}

		public static void SlideByte(BinaryReaderX br, SCompressInfo info, uint srcPos)
		{
			br.BaseStream.Position = srcPos - 1;
			byte uInData = br.ReadByte();
			ushort uInsertOffset = 0;
			ushort uWindowPos = info.WindowPos;
			ushort uWindowLen = info.WindowLen;

			if (uWindowLen==4098)
			{
				br.BaseStream.Position = srcPos + 4097;
				byte uOutData = br.ReadByte();
				if ((info.work[info.byteTablePos+uOutData]=info.work[info.offsetTablePos+info.work[info.byteTablePos+uOutData]])==-1)
				{
					info.work[info.endTablePos+uOutData] = -1;
				} else
				{
					info.work[info.reversedOffsetTablePos+info.work[info.byteTablePos+uOutData]] = -1;
				}
				uInsertOffset = uWindowPos;
			}else
			{
				uInsertOffset = uWindowLen;
			}

			short nOffset = info.work[info.endTablePos+uInData];
			if (nOffset==-1)
			{
				info.work[info.byteTablePos+uInData] = (short)uInsertOffset;
			} else
			{
				info.work[info.offsetTablePos+nOffset] = (short)uInsertOffset;
			}

			info.work[info.endTablePos+uInData] = (short)uInsertOffset;
			info.work[info.offsetTablePos+uInsertOffset] = -1;
			info.work[info.reversedOffsetTablePos+uInsertOffset] = nOffset;

			if (uWindowLen==4098)
			{
				info.WindowPos = (ushort)((uWindowPos + 1) % 4098);
			} else
			{
				info.WindowLen++;
			}
		}
    }
}
