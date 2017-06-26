using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Cetera.Image;
using Kuriimu.IO;

namespace image_tex
{
    class TEX
    {
        private const int WidthMultiplier = 4;
        private const int HeightMultiplier = 32;
        private const int MinHeight = 8;

        public Header Header { get; set; }
        public ImageSettings Settings = new ImageSettings();
        public List<Bitmap> Bitmaps = new List<Bitmap>();

        public TEX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                Header = br.ReadStruct<Header>();
                var mipMaps = br.ReadMultiple<int>(Header.MipMapCount);
                Settings.Format = ImageSettings.ConvertFormat(Header.Format);

                for (var i = 0; i < mipMaps.Count; i++)
                {
                    var texDataSize = (i + 1 < mipMaps.Count ? mipMaps[i + 1] : (int)br.BaseStream.Length) - mipMaps[i];
                    Settings.Width = (Header.Width * WidthMultiplier) >> i;
                    Settings.Height = Math.Max((Header.Height * HeightMultiplier) >> i, MinHeight);
                    Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                bw.WriteStruct(Header);

                Settings.Format = ImageSettings.ConvertFormat(Header.Format);
                var bitmaps = Bitmaps.Select(bmp => Common.Save(bmp, Settings)).ToList();

                var offset = 0;
                foreach (var bitmap in bitmaps)
                {
                    bw.Write(offset);
                    offset += bitmap.Length;
                }

                foreach (var bitmap in bitmaps)
                    bw.Write(bitmap);
            }
        }
    }
}
