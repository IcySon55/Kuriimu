using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Kontract.Interface
{
    public interface IImageAdapter : IFilePlugin
    {
        // Images
        IList<BitmapInfo> Bitmaps { get; }
    }

    public class BitmapInfo
    {
        [Browsable(false)]
        public Bitmap Bitmap { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public string Name { get; set; }

        [Category("Properties")]
        [Description("The dimensions of the image.")]
        public Size Size => Bitmap?.Size ?? new Size();
    }
}
