using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Image;
using Kontract.Image.Format;

namespace image_mt.Mobile
{
    public class MobileMTTEX
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public List<string> formatNames = new List<string>();
        public ImageSettings settings = new ImageSettings();

        private Header header;
        public HeaderInfo headerInfo { get; set; }

        public MobileMTTEX(Stream input)
        {
            
        }

        public void Save(Stream output)
        {
            
        }
    }
}
