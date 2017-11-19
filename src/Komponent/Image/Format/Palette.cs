using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;
using Komponent.IO;
using System.IO;

namespace Komponent.Image.Format
{
    public class Palette : IImagePalette
    {
        public int BitDepth { get; }

        public string FormatName { get; set; }

        List<Color> PaletteColors;
        byte[] PaletteBytes;

        IImageFormat PaletteFormat;

        ByteOrder byteOrder;

        public Palette(int indexDepth, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if (indexDepth % 4 != 0) throw new Exception("IndexDepth has to be dividable by 4.");

            this.byteOrder = byteOrder;

            BitDepth = indexDepth;
        }

        public void SetPaletteFormat(IImageFormat format)
        {
            FormatName = "Paletted " + format.FormatName;

            PaletteFormat = format;
        }

        public void SetPaletteColors(byte[] bytes)
        {
            if (PaletteFormat == null)
                throw new Exception("PaletteFormat wasn't set! Call SetPaletteFormat beforehand!");

            PaletteColors = PaletteFormat.Load(bytes).ToList();
        }

        public void SetPaletteColors(IEnumerable<Color> colors)
        {
            PaletteColors = colors.ToList();
        }

        public byte[] GetPaletteBytes() => PaletteBytes;

        public IEnumerable<Color> GetPaletteColors() => PaletteColors;

        public IEnumerable<Color> Load(byte[] data)
        {
            if (PaletteColors == null)
                throw new Exception("PaletteColors aren't set! Call SetPaletteColors beforehand!");

            using (var br = new BinaryReaderX(new MemoryStream(data), true, byteOrder))
                while (true)
                    switch (BitDepth)
                    {
                        case 4:
                            yield return PaletteColors[br.ReadNibble()];
                            break;
                        case 8:
                            yield return PaletteColors[br.ReadByte()];
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
            PaletteBytes = (PaletteFormat == null) ? null : PaletteFormat.Save(redColors);

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
