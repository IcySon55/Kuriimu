using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.IO;

namespace image_tex
{
    class TEX
    {
        private const int HeaderLength = 0x10;
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

                for (int i = 0; i < mipMaps.Count; i++)
                {
                    br.BaseStream.Position = HeaderLength + (Header.MipMapCount * 4) + mipMaps[i];
                    var texDataSize = (i + 1 < mipMaps.Count) ? mipMaps[i + 1] : ((int)br.BaseStream.Length - (int)br.BaseStream.Position);
                    Settings.Width = (Header.Width * WidthMultiplier) / Math.Max((int)Math.Pow(2, i), 1);
                    Settings.Height = Math.Max((Header.Height * HeightMultiplier) / Math.Max((int)Math.Pow(2, i), 1), MinHeight);
                    Bitmaps.Add(Common.Load(br.ReadBytes(texDataSize), Settings));
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                bw.WriteStruct(Header);

                var bitmaps = new List<byte[]>();
                for (int i = 0; i < Bitmaps.Count; i++)
                {
                    Settings.Width = (Header.Width * WidthMultiplier) / Math.Max((int)Math.Pow(2, i), 1);
                    Settings.Height = Math.Max((Header.Height * HeightMultiplier) / Math.Max((int)Math.Pow(2, i), 1), MinHeight);
                    bitmaps.Add(Common.Save(Bitmaps[i], Settings));
                }
                var mipMaps = new List<int>() { 0 };

                for (int i = 0; i < Bitmaps.Count - 1; i++)
                    mipMaps.Add(bitmaps[i].Length);

                foreach (var mipMap in mipMaps)
                    bw.Write(mipMap);

                foreach (var bitmap in bitmaps)
                    bw.Write(bitmap);
            }
        }
    }
}
