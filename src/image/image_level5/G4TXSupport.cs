using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using archive_nlp.PACK;
using Kontract;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract.Interface;
using Kontract.IO;

namespace image_xi.G4TX
{
    public class NXTCH
    {
        public Bitmap bmp;
        public List<Bitmap> mipMaps = new List<Bitmap>();

        private NXTCHHeader _header;
        private byte[] _unknowns;

        public NXTCH(Stream stream)
        {
            using (var br = new BinaryReaderX(stream))
            {
                _header = br.ReadStruct<NXTCHHeader>();
                var mipMapOffsets = br.ReadMultiple<int>(_header.mipMapCount);
                _unknowns = br.ReadBytes(0x100 - 0x30 - _header.mipMapCount * 4);

                if (!G4TXSupport.Formats.ContainsKey(_header.format))
                    throw new InvalidOperationException($"Unknown encoding {_header.format}.");

                for (int i = 0; i < _header.mipMapCount; i++)
                {
                    var size = (i + 1 >= _header.mipMapCount ? _header.textureDataSize2 : mipMapOffsets[i + 1]) -
                               mipMapOffsets[i];
                    br.BaseStream.Position = 0x100 + mipMapOffsets[i];
                    var data = br.ReadBytes(size);

                    if (i == 0)
                        bmp = Common.Load(data, new ImageSettings
                        {
                            Format = G4TXSupport.Formats[_header.format].Item1,
                            Height = _header.height,
                            Width = _header.width,
                            Swizzle = new SwitchSwizzle(_header.width, _header.height,
                                G4TXSupport.Formats[_header.format].Item1.BitDepth,
                                G4TXSupport.Formats[_header.format].Item2)
                        });
                    // TODO: MipMaps get ommitted; Fix in K2
                    //else
                    //    mipMaps.Add(Common.Load(data, new ImageSettings
                    //    {
                    //        Format = G4TXSupport.Formats[header.format].Item1,
                    //        Height = header.height >> i,
                    //        Width = header.width >> i,
                    //        Swizzle = new SwitchSwizzle(header.width>>1, header.height>>1,
                    //            G4TXSupport.Formats[header.format].Item1.BitDepth,
                    //            G4TXSupport.Formats[header.format].Item2)
                    //    }));
                }
            }
        }

        public void Save(Stream stream, long offset)
        {
            using (var bw = new BinaryWriterX(stream, true))
            {
                var padWidth = 2 << (int)Math.Log(bmp.Width - 1, 2);
                var padHeight = 2 << (int)Math.Log(bmp.Height - 1, 2);

                _header.width = padWidth;
                _header.height = padHeight;

                // Write mipMap offsets
                bw.BaseStream.Position = offset + 0x30;
                var mipMapOffset = 0;
                for (int i = 0; i < _header.mipMapCount; i++)
                {
                    bw.Write(mipMapOffset);

                    var pixelCount = (padWidth >> i) * (padHeight >> i);
                    var bitDepth = G4TXSupport.Formats[_header.format].Item1.BitDepth;
                    // Pad every mipMap to a multiple of 0x200
                    mipMapOffset += (pixelCount * bitDepth / 8 + 0x1FF) & ~0x1FF;
                }

                // Write unknown data
                bw.Write(_unknowns);

                // Write textures
                var ms = new MemoryStream();
                for (int i = 0; i < _header.mipMapCount; i++)
                {
                    var format = G4TXSupport.Formats[_header.format];

                    if (i == 0)
                    {
                        var data = Common.Save(bmp, new ImageSettings
                        {
                            Format = format.Item1,
                            Height = padHeight,
                            Width = padWidth,
                            Swizzle = new SwitchSwizzle(padWidth, padHeight, format.Item1.BitDepth, format.Item2)
                        });
                        ms.Write(data, 0, data.Length);
                    }
                    else
                    {
                        var toSave = Resize(bmp, padWidth >> i, padHeight >> i);
                        var data = Common.Save(toSave, new ImageSettings
                        {
                            Format = format.Item1,
                            Height = toSave.Height,
                            Width = toSave.Width,
                            Swizzle = new SwitchSwizzle(toSave.Width, toSave.Height, format.Item1.BitDepth, format.Item2)
                        });
                        ms.Write(data, 0, data.Length);
                    }

                    var toFill = ((ms.Length + 0x1FF) & ~0x1FF) - ms.Length;
                    for (int j = 0; j < toFill; j++)
                        ms.WriteByte(0);
                }

                bw.BaseStream.Position = offset + 0x100;
                ms.Position = 0;
                ms.CopyTo(bw.BaseStream);

                // Write header
                var bkPos = bw.BaseStream.Position;
                bw.BaseStream.Position = offset;
                _header.textureDataSize = _header.textureDataSize2 = (int)(bw.BaseStream.Length - offset - 0x100);
                bw.WriteStruct(_header);
                bw.BaseStream.Position = bkPos;
            }
        }

        private Bitmap Resize(Bitmap img, int newWidth, int newHeight)
        {
            var result = new Bitmap(newWidth, newHeight);
            var gfx = Graphics.FromImage(result);
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            gfx.DrawImage(img, new Rectangle(0, 0, result.Width, result.Height));

            return result;
        }
    }

    public class G4TXSupport
    {
        public static Dictionary<int, (IImageFormat, SwitchSwizzle.Format)> Formats = new Dictionary<int, (IImageFormat, SwitchSwizzle.Format)>
        {
            [0x25] = (new RGBA(8, 8, 8, 8), SwitchSwizzle.Format.RGBA8888),
            [0x42] = (new DXT(DXT.Version.DXT1), SwitchSwizzle.Format.DXT1)
        };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic = "G4TX"; // G4TX
        public short headerSize = 0x60;
        public short fileType = 0x65;
        public int unk1 = 0x00180000;
        public int contentSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] zeroes = new byte[0x10];
        public short textureCount;
        public short textureCount2;
        public long unk2;
        public int textureDataSize;
        public long unk3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x28)]
        public byte[] unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class TextureEntry
    {
        public int unk1;
        public int nxtchOffset;
        public int nxtchSize;
        public int unk2;
        public int unk3;
        public int const1 = 0xAAE4;
        public short width;
        public short height;
        public int const2 = 1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] unk4 = new byte[0x10];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class NXTCHHeader
    {
        public Magic8 magic = "NXTCH000"; // NXTCH000

        public int textureDataSize;
        public int unk1;
        public int unk2;
        public int width;
        public int height;
        public int unk3;
        public int unk4;
        public int format; // TODO: has to be proven
        public int mipMapCount;
        public int textureDataSize2;
    }
}
