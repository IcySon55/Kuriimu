using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.IO;
using System.Xml.Serialization;
using System.Xml;
using Cetera.Image;
using System;

namespace image_texi
{
    public class TEXI
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        SERIList seri;

        public TEXI(Stream input, Stream texi)
        {
            using (var br = new BinaryReaderX(input))
            {
                seri = (SERIList)new XmlSerializer(typeof(SERIList)).Deserialize(XmlReader.Create(texi));

                var width = (Integer)seri.Parameters.Find(x => x.Name == "w");
                var height = (Integer)seri.Parameters.Find(x => x.Name == "h");
                var mipMapCount = (Integer)seri.Parameters.Find(x => x.Name == "mipmap");
                var format = (Integer)seri.Parameters.Find(x => x.Name == "format");

                for (int i = 1; i <= ((mipMapCount.Value == 0) ? 1 : mipMapCount.Value); i++)
                {
                    var settings = new ImageSettings
                    {
                        Width = width.Value,
                        Height = height.Value,
                        Format = ImageSettings.ConvertFormat((Format)format.Value),
                        PadToPowerOf2 = false
                    };

                    bmps.Add(Common.Load(br.ReadBytes(width.Value * height.Value * Common.GetBitDepth(ImageSettings.ConvertFormat((Format)format.Value)) / 8), settings));

                    width.Value /= 2;
                    height.Value /= 2;
                }
            }
        }

        public void Save(Stream texInput, Stream texiInput)
        {
            //Check sizes, if mipMaps
            if (bmps.Count > 1)
            {
                var bmpW = bmps[0].Width / 2;
                var bmpH = bmps[0].Height / 2;
                for (int i = 1; i < bmps.Count; i++)
                {
                    if (bmps[i].Width != bmpW)
                        throw new Exception($"Width of image {i} has to be {bmpW}");
                    if (bmps[i].Height != bmpH)
                        throw new Exception($"Width of image {i} has to be {bmpH}");
                    bmpW /= 2;
                    bmpH /= 2;
                }
            }

            //Creating image
            using (var bw = new BinaryWriterX(texInput))
            {
                var width = (Integer)seri.Parameters.Find(x => x.Name == "w");
                var height = (Integer)seri.Parameters.Find(x => x.Name == "h");
                var owidth = (Integer)seri.Parameters.Find(x => x.Name == "ow");
                var oheight = (Integer)seri.Parameters.Find(x => x.Name == "oh");
                var mipMapCount = (Integer)seri.Parameters.Find(x => x.Name == "mipmap");
                var format = (Integer)seri.Parameters.Find(x => x.Name == "format");

                width.Value = bmps[0].Width;
                height.Value = bmps[0].Height;
                owidth.Value = bmps[0].Width;
                oheight.Value = bmps[0].Height;

                foreach (var bmp in bmps)
                {
                    var settings = new ImageSettings
                    {
                        Width = bmp.Width,
                        Height = bmp.Height,
                        Format = ImageSettings.ConvertFormat((Format)format.Value),
                        PadToPowerOf2 = false,
                    };

                    bw.Write(Common.Save(bmp, settings));
                }

                XmlSerializer Serializer = new XmlSerializer(typeof(SERIList));
                Serializer.Serialize(texiInput, seri);
                texiInput.Close();
            }
        }
    }
}
