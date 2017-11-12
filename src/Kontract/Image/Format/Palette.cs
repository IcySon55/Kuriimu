using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;
using Kontract.IO;
using System.IO;

namespace Kontract.Image.Format
{
    public class Palette : IImageFormat
    {
        IImageFormat paletteFormat;
        public List<Color> colors;
        public byte[] paletteBytes;

        ByteOrder byteOrder;

        public int BitDepth { get; }

        public string FormatName { get; }

        public Palette(byte[] paletteData, IImageFormat paletteFormat, int indexDepth = 8, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if (indexDepth % 4 != 0) throw new Exception("IndexDepth has to be dividable by 4.");

            this.byteOrder = byteOrder;

            BitDepth = indexDepth;
            FormatName = "Paletted " + paletteFormat.FormatName;

            this.paletteFormat = paletteFormat;
            paletteBytes = paletteData;
            colors = paletteFormat.Load(paletteData).ToList();
        }

        public Palette(List<Color> palette, int indexDepth = 8, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if (indexDepth % 4 != 0) throw new Exception("IndexDepth has to be dividable by 4.");

            this.byteOrder = byteOrder;

            BitDepth = indexDepth;
            FormatName = "Custom Palette";

            paletteFormat = null;
            paletteBytes = null;
            colors = palette;
        }

        public IEnumerable<Color> Load(byte[] data)
        {
            using (var br = new BinaryReaderX(new MemoryStream(data), true, byteOrder))
                while (true)
                    switch (BitDepth)
                    {
                        case 4:
                            yield return colors[br.ReadNibble()];
                            break;
                        case 8:
                            yield return colors[br.ReadByte()];
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

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true, byteOrder))
            {
                foreach (var color in colors)
                    switch (BitDepth)
                    {
                        case 4:
                            bw.WriteNibble(redColors.FindIndex(c => c == color));
                            break;
                        case 8:
                            bw.Write((byte)redColors.FindIndex(c => c == color));
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
