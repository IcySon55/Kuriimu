using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kontract.Interface
{
    //Common
    public interface IImageCommon
    {
        Bitmap Load(byte[] bytes, ImageSettings2 settings);
        byte[] Save(Bitmap bmp, ImageSettings2 settings);
    }

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

    public interface IImageMasterSwizzle
    {
        int MacroTileWidth { get; }
        int MacroTileHeight { get; }

        Point Get(int pointCount);
    }

    /// <summary>
    /// Defines the settings with which an image will be loaded/saved
    /// </summary>
    public class ImageSettings2
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IImageFormat Format { get; set; }

        public IImageSwizzle Swizzle { get; set; }
        public Func<Color, Color> PixelShader { get; set; }
    }
}
