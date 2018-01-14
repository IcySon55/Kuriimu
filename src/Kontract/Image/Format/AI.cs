using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;
using Kontract.IO;
using System.IO;
using Kontract.Image.Support;

namespace Kontract.Image.Format
{
    public class AI : IImageFormat
    {
        IImageFormat paletteFormat;
        public List<Color> colors;
        public byte[] paletteBytes;

        int alpha;
        int indexSize;

        ByteOrder byteOrder;

        public int BitDepth { get; }

        public string FormatName { get; }

        public AI(int alpha, int indexSize, byte[] paletteData, IImageFormat paletteFormat, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if ((alpha + indexSize) % 8 != 0) throw new Exception("Alpha + IndexSize has to be dividable by 8.");

            this.byteOrder = byteOrder;

            this.alpha = alpha;
            this.indexSize = indexSize;

            BitDepth = alpha + indexSize;
            FormatName = $"A{alpha}I{indexSize}";

            this.paletteFormat = paletteFormat;
            paletteBytes = paletteData;
            colors = paletteFormat.Load(paletteData).ToList();
        }

        public AI(int alpha, int indexSize, List<Color> palette, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if ((alpha + indexSize) % 8 != 0) throw new Exception("Alpha + IndexSize has to be dividable by 8.");

            this.byteOrder = byteOrder;

            this.alpha = alpha;
            this.indexSize = indexSize;

            BitDepth = alpha + indexSize;
            FormatName = $"A{alpha}I{indexSize}";

            paletteFormat = null;
            paletteBytes = null;
            colors = palette;
        }

        public IEnumerable<Color> Load(byte[] data)
        {
            var alphaShift = indexSize;

            using (var br = new BinaryReaderX(new MemoryStream(data), true, byteOrder))
                while (true)
                    switch (BitDepth)
                    {
                        case 8:
                            var b = br.ReadByte();
                            yield return Color.FromArgb(
                                Support.Support.ChangeBitDepth(b >> alphaShift, alpha, 8),
                                colors[b & ((1 << indexSize) - 1)]);
                            break;
                        default:
                            throw new Exception($"BitDepth {BitDepth} not supported!");
                    }
        }

        /// <summary>
        /// Converts an Enumerable of colors into a byte[].
        /// Palette will be written before the image data
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        public byte[] Save(IEnumerable<Color> colors)
        {
            var redColors = CreatePalette(colors.ToList());
            paletteBytes = (paletteFormat == null) ? null : paletteFormat.Save(redColors);

            var alphaShift = indexSize;

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true, byteOrder))
            {
                foreach (var color in colors)
                    switch (BitDepth)
                    {
                        case 8:
                            byte b = (byte)(Support.Support.ChangeBitDepth(color.A, 8, alpha) << alphaShift);
                            bw.Write(b | redColors.FindIndex(c => c == color));
                            break;
                        default:
                            throw new Exception($"BitDepth {BitDepth} not supported!");
                    }
            }

            return ms.ToArray();
        }

        List<Color> CreatePalette(List<Color> colors)
        {
            List<Color> reducedColors = new List<Color>();
            foreach (var color in colors)
                if (!reducedColors.Exists(c => c == color)) reducedColors.Add(color);

            return reducedColors;
        }
    }
}
