using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;
using Kontract.IO;
using System.IO;
using static Kontract.Image.Support.PVRTC;

namespace Kontract.Image.Format
{
    public class ETC2 : IImageFormat
    {
        public enum Format : ulong
        {
            ETC2 = PixelFormat.ETC2_RGB,
            ETC2A = PixelFormat.ETC2_RGBA,
            ETC2A1 = PixelFormat.ETC2_RGB_A1,
        }

        public int BitDepth { get; set; }
        public int BlockBitDepth { get; set; }

        public string FormatName { get; set; }

        public int _width = -1;
        public int _height = -1;
        Format _format;

        ByteOrder byteOrder;

        public ETC2(Format format, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            BitDepth = 4;
            BlockBitDepth = 128;

            _format = format;

            FormatName = "ETC2";

            this.byteOrder = byteOrder;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            if (_width < 0 || _height < 0)
                throw new InvalidDataException("Height and Width has to be set for ETC2.");

            var pvrtcTex = PVRTexture.CreateTexture(tex, (uint)_width, (uint)_height, 1, (PixelFormat)_format, false, VariableType.UnsignedByte, ColourSpace.lRGB);

            pvrtcTex.Transcode(PixelFormat.RGBA8888, VariableType.UnsignedByteNorm, ColourSpace.lRGB);

            byte[] decodedTex = new byte[pvrtcTex.GetTextureDataSize()];
            pvrtcTex.GetTextureData(decodedTex, pvrtcTex.GetTextureDataSize());

            using (var br = new BinaryReaderX(new MemoryStream(decodedTex)))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var v0 = br.ReadByte();
                    var v1 = br.ReadByte();
                    var v2 = br.ReadByte();
                    var v3 = br.ReadByte();
                    yield return Color.FromArgb(v3, v0, v1, v2);
                }
            }
        }

        public byte[] Save(IEnumerable<Color> colors)
        {
            if (_width < 0 || _height < 0)
                throw new InvalidDataException("Height and Width has to be set for ETC2.");

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true))
                foreach (var color in colors)
                {
                    bw.Write(color.R);
                    bw.Write(color.G);
                    bw.Write(color.B);
                    bw.Write(color.A);
                }

            var pvrtcTex = PVRTexture.CreateTexture(ms.ToArray(), (uint)_width, (uint)_height, 1, PixelFormat.RGBA8888, false, VariableType.UnsignedByteNorm, ColourSpace.lRGB);

            pvrtcTex.Transcode((PixelFormat)_format, VariableType.UnsignedByteNorm, ColourSpace.lRGB, CompressorQuality.PVRTCHigh);

            byte[] encodedTex = new byte[pvrtcTex.GetTextureDataSize()];
            pvrtcTex.GetTextureData(encodedTex, pvrtcTex.GetTextureDataSize());

            return encodedTex;
        }
    }
}
