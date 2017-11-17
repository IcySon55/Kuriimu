using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kontract.Interface
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
        byte[] GetPaletteBytes();
    }

    //Swizzle
    public interface IImageSwizzle
    {
        int Width { get; }
        int Height { get; }

        Point Get(Point point);
    }
}
