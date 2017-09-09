using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.IO;
using System.Xml.Serialization;
using System.Xml;
using Cetera.Image;
using System;

namespace image_texi
{
    public class TEXI
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        ImageSettings settings;

        public TEXI(Stream input, Stream texi)
        {
            using (var br = new BinaryReaderX(input))
            {
                GetXMLValues(texi, out var width, out var height, out var mipMapCount, out var format);

                for (int i = 1; i <= ((mipMapCount == 0) ? 1 : mipMapCount); i++)
                {
                    settings = new ImageSettings
                    {
                        Width = width,
                        Height = height,
                        Format = ImageSettings.ConvertFormat(format)
                    };

                    bmps.Add(Common.Load(br.ReadBytes(width * height * Common.GetBitDepth(ImageSettings.ConvertFormat(format)) / 8), settings));

                    width /= 2;
                    height /= 2;
                }
            }
        }

        public void GetXMLValues(Stream texi, out int width, out int height, out int mipMapCount, out Format format)
        {
            width = 0;
            height = 0;
            mipMapCount = 0;
            format = Format.RGBA8888;

            var xmlReader = XmlReader.Create(texi);

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement())
                {
                    switch (xmlReader.GetAttribute("Name"))
                    {
                        case "w":
                            for (int i = 0; i < 3; i++) xmlReader.Read();
                            width = Int32.Parse(xmlReader.Value);
                            break;
                        case "h":
                            for (int i = 0; i < 3; i++) xmlReader.Read();
                            height = Int32.Parse(xmlReader.Value);
                            break;
                        case "mipmap":
                            for (int i = 0; i < 3; i++) xmlReader.Read();
                            mipMapCount = Int32.Parse(xmlReader.Value);
                            break;
                        case "format":
                            for (int i = 0; i < 3; i++) xmlReader.Read();
                            format = (Format)Int32.Parse(xmlReader.Value);
                            break;
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
