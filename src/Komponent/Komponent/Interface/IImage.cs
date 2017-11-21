using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Komponent.Interface
{
    //Format
    public interface IImageFormat
    {
        int BitDepth { get; }

        string FormatName { get; }

        IEnumerable<Color> Load(byte[] input);
        byte[] Save(IEnumerable<Color> colors);
    }

    public interface IImagePalette : IImageFormat
    {
        void SetPaletteFormat(IImageFormat format);

        void SetPaletteColors(byte[] bytes);
        void SetPaletteColors(IEnumerable<Color> colors);

        byte[] GetPaletteBytes();
        IEnumerable<Color> GetPaletteColors();
    }

    //Swizzle
    public interface IImageSwizzle
    {
        int Width { get; }
        int Height { get; }

        Point Get(Point point);
    }
}
