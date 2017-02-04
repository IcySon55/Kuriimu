using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace KuriimuContract
{
	public class ImageCommon
	{
		[DllImport("rg_etc1.dll", EntryPoint = "pack_etc1_block", CallingConvention = CallingConvention.Cdecl)]
		static extern int pack_etc1_block(byte[] pETC1_block, int[] pSrc_pixels_rgba, int[] pack_params);

		[DllImport("rg_etc1.dll", EntryPoint = "unpack_etc1_block", CallingConvention = CallingConvention.Cdecl)]
		static extern bool unpack_etc1_block(byte[] pETC1_block, int[] pDst_pixels_rgba, bool preserve_alpha = false);

		[DllImport("rg_etc1.dll", EntryPoint = "pack_etc1_block_init", CallingConvention = CallingConvention.Cdecl)]
		static extern bool pack_etc1_block_init();

		static readonly int[] order = { 0, 4, 1, 5, 8, 12, 9, 13, 2, 6, 3, 7, 10, 14, 11, 15 };

		static int npo2(int n) => 2 << (int)Math.Log(n - 1, 2); // Next power of 2

		// Standard Format Definitions
		public enum Format : byte
		{
			L8, A8, LA4, LA8, HILO8,
			RGB565, RGB8, RGBA5551,
			RGBA4444, RGBA8888,
			ETC1, ETC1A4, L4, A4
		}

		// Image Orientation
		public enum ImageOrientation : byte
		{
			RightDown, UpRight = 4, DownRight = 8
		}

		static ImageCommon()
		{
			pack_etc1_block_init();
		}

		public static Bitmap FromTexture(byte[] texture, int width, int height, Format format, ImageOrientation orientation = 0)
		{
			var bmp = new Bitmap(width, height);
			int? nibble = null;

			int strideWidth = Math.Max(8, npo2(width));
			int strideHeight = Math.Max(8, npo2(height));

			var etc1colors = new Queue<Color>();

			using (var br = new BinaryReader(new MemoryStream(texture)))
			{
				Func<int> ReadNibble = () =>
				{
					int val;
					if (nibble == null)
					{
						val = br.ReadByte();
						nibble = val / 16;
						val %= 16;
					}
					else
					{
						val = nibble.Value;
						nibble = null;
					}
					return val;
				};
				//throw new Exception((strideWidth * strideHeight).ToString() + " " + texture.Length.ToString());
				for (int i = 0; i < strideWidth * strideHeight; i++)
				{
					int a = 255, r = 255, g = 255, b = 255;
					Color c = default(Color);
					switch (format)
					{
						case Format.L8:
							b = g = r = br.ReadByte();
							break;
						case Format.A8:
							a = br.ReadByte();
							break;
						case Format.LA4:
							a = ReadNibble() * 17;
							b = g = r = ReadNibble() * 17;
							break;
						case Format.LA8:
							a = br.ReadByte();
							b = g = r = br.ReadByte();
							break;
						case Format.HILO8:
							g = br.ReadByte();
							r = br.ReadByte();
							break;
						case Format.RGB565:
							var s = br.ReadUInt16();
							b = (s % 32) * 33 / 4;
							g = (s >> 5) % 64 * 65 / 16;
							r = (s >> 11) * 33 / 4;
							break;
						case Format.RGB8:
							try
							{
								b = br.ReadByte();
								g = br.ReadByte();
								r = br.ReadByte();
								break;
							} catch (Exception)
							{
								b = 255;
								g = 255;
								r = 255;
								break;
							}
						case Format.RGBA5551:
							var s2 = br.ReadUInt16();
							a = (s2 & 1) * 255;
							b = (s2 >> 1) % 32 * 33 / 4;
							g = (s2 >> 6) % 32 * 33 / 4;
							r = (s2 >> 11) % 32 * 33 / 4;
							break;
						case Format.RGBA4444:
							a = ReadNibble() * 17;
							b = ReadNibble() * 17;
							g = ReadNibble() * 17;
							r = ReadNibble() * 17;
							break;
						case Format.RGBA8888:
							a = br.ReadByte();
							b = br.ReadByte();
							g = br.ReadByte();
							r = br.ReadByte();
							break;
						case Format.ETC1:
						case Format.ETC1A4:
							if (etc1colors.Count == 0)
							{
								var alpha = (format == Format.ETC1A4) ? br.ReadUInt64() : ulong.MaxValue;
								var unpacked = new int[16];
								unpack_etc1_block(br.ReadBytes(8).Reverse().ToArray(), unpacked);
								foreach (int j in order) // Unscramble
								{
									var k = BitConverter.GetBytes(unpacked[(j % 4) * 4 + j / 4]);
									etc1colors.Enqueue(Color.FromArgb((byte)(alpha >> (4 * j)) % 16 * 17, k[0], k[1], k[2]));
								}
							}
							c = etc1colors.Dequeue();
							break;
						case Format.L4:
							b = g = r = ReadNibble() * 17;
							break;
						case Format.A4:
							a = ReadNibble() * 17;
							break;
						default:
							throw new NotSupportedException();
					}

					int x, y;
					switch (orientation)
					{
						case ImageOrientation.RightDown:
							x = (i / 64 % (strideWidth / 8)) * 8 + (i / 4 & 4) | (i / 2 & 2) | (i & 1);
							y = (i / 64 / (strideWidth / 8)) * 8 + (i / 8 & 4) | (i / 4 & 2) | (i / 2 & 1);
							break;
						case ImageOrientation.UpRight:
							x = (i / 64 / (strideHeight / 8)) * 8 + (i / 8 & 4) | (i / 4 & 2) | (i / 2 & 1);
							y = (i / 64 % (strideHeight / 8)) * 8 + (i / 4 & 4) | (i / 2 & 2) | (i & 1);
							y = strideHeight - 1 - y;
							break;
						case ImageOrientation.DownRight:
							x = (i / 64 / (strideHeight / 8)) * 8 + (i / 8 & 4) | (i / 4 & 2) | (i / 2 & 1);
							y = (i / 64 % (strideHeight / 8)) * 8 + (i / 4 & 4) | (i / 2 & 2) | (i & 1);
							break;
						default:
							throw new NotSupportedException();
					}
					if (format != Format.ETC1 && format != Format.ETC1A4)
					{
						c = Color.FromArgb(a, r, g, b);
					}
					if (0 <= x && x < width && 0 <= y && y < height)
					{
						bmp.SetPixel(x, y, c);
					}
				}
			}
			return bmp;
		}

		public static byte[] ToTexture(Bitmap bmp, Format format, ImageOrientation orientation = 0)
		{
			var ms = new MemoryStream();
			int width = bmp.Width, height = bmp.Height;
			int stride = Math.Max(8, npo2(height));

			var etc1colors = new Queue<Color>();

			using (var bw = new BinaryWriter(ms))
			{
				int? nibble = null;
				Action<int> WriteNibble = val =>
				{
					val &= 15;
					if (nibble == null)
					{
						nibble = val;
					}
					else
					{
						bw.Write((byte)(nibble.Value + 16 * val));
						nibble = null;
					}
				};

				for (int i = 0; i < ((width + 7) & ~7) * stride; i++)
				{
					//throw new NotSupportedException("Need to make changes to some swizzle stuff");
					int x = (i / 64 / (stride / 8)) * 8 + (i / 8 & 4) | (i / 4 & 2) | (i / 2 & 1);
					int y = (i / 64 % (stride / 8)) * 8 + (i / 4 & 4) | (i / 2 & 2) | (i & 1);
					if (orientation == ImageOrientation.UpRight)
					{
						y = stride - 1 - y;
					}

					x = Math.Min(x, bmp.Width - 1);
					y = Math.Max(0, Math.Min(y, bmp.Height - 1));
					var color = bmp.GetPixel(x, y);
					if (color.A == 0) color = default(Color);

					switch (format)
					{
						case Format.L8:
							bw.Write((byte)((color.R + color.G + color.B) / 3));
							break;
						case Format.A8:
							bw.Write(color.A);
							break;
						case Format.LA4:
							WriteNibble(color.A / 16);
							WriteNibble((color.R + color.G + color.B) / 48);
							break;
						case Format.LA8:
							bw.Write(color.A);
							bw.Write((byte)((color.R + color.G + color.B) / 3));
							break;
						case Format.HILO8:
							bw.Write(color.G);
							bw.Write(color.B);
							break;
						case Format.RGB565:
							bw.Write((short)((color.R / 8 << 11) | (color.G / 4 << 5) | (color.B / 8)));
							break;
						case Format.RGB8:
							bw.Write(color.B);
							bw.Write(color.G);
							bw.Write(color.R);
							break;
						case Format.RGBA5551:
							bw.Write((short)((color.R / 8 << 11) | (color.G / 8 << 6) | (color.B / 8 << 1) | color.A / 128));
							break;
						case Format.RGBA4444:
							WriteNibble(color.A / 16);
							WriteNibble(color.B / 16);
							WriteNibble(color.G / 16);
							WriteNibble(color.R / 16);
							break;
						case Format.RGBA8888:
							bw.Write(color.A);
							bw.Write(color.B);
							bw.Write(color.G);
							bw.Write(color.R);
							break;
						case Format.ETC1:
						case Format.ETC1A4:
							etc1colors.Enqueue(color);
							if (etc1colors.Count != 16) continue;

							var c = Enumerable.Range(0, 16).Select(j => etc1colors.ElementAt(order[order[order[j]]])).ToList();
							etc1colors.Clear();

							if (format == Format.ETC1A4) c.ForEach(d => WriteNibble(d.A / 16));

							var packed = new byte[8];
							var colorArray = new int[16];
							for (int j = 0; j < 16; j++)
							{
								colorArray[(j % 4) * 4 + j / 4] = BitConverter.ToInt32(new byte[] { c[j].R, c[j].G, c[j].B, 255 }, 0);
							}
							pack_etc1_block(packed, colorArray, new[] { 2, 0 });
							bw.Write(packed.Reverse().ToArray());
							break;
						case Format.L4:
							WriteNibble((color.R + color.G + color.B) / 48);
							break;
						case Format.A4:
							WriteNibble(color.A / 3);
							break;
						default:
							throw new NotSupportedException();
					}
				}
			}

			return ms.ToArray();
		}
	}
}