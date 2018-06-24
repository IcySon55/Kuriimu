using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Interface;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract.IO;
using System.Text;

//http://cdn.imgtec.com/sdk-documentation/PVR+File+Format.Specification.pdf

namespace image_pvr
{
    public class PVR
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public List<ImageSettings> settings;

        ByteOrder ByteOrder;
        Header Header;

        public PVR(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Get Endianess
                br.ByteOrder = ByteOrder = br.PeekString(4) == "PVR\x3" ? ByteOrder.LittleEndian : ByteOrder.BigEndian;

                //Header
                Header = br.ReadStruct<Header>();

                //Meta
                if (Header.metaSize > 0)
                {
                    var meta = br.ReadStruct<Meta>();
                    //For pre-defined meta data look at 2.2.5 in the documentation
                    //meta data will not be valued
                    br.ReadBytes(meta.dataSize - 0xc);
                }

                //Load textures
                settings = new List<ImageSettings>();
                for (int mip = 0; mip < Header.mipmapCount; mip++)
                {
                    var width = Header.width >> mip;
                    var height = Header.height >> mip;
                    for (int surface = 0; surface < Header.surfaceCount; surface++)
                        for (int face = 0; face < Header.facesCount; face++)
                            for (int d = 0; d < Header.depth; d++)
                            {
                                IImageFormat format = Support.Formats[Header.format];
                                if (format.FormatName.Contains("PVRTC"))
                                {
                                    (format as PVRTC)._width = width;
                                    (format as PVRTC)._height = height;
                                }

                                var setting = new ImageSettings
                                {
                                    Width = width,
                                    Height = height,
                                    Format = format,
                                    Swizzle = Support.formatToSwizzle.Contains((int)Header.format) ? new Support.BlockSwizzle(width, height) : null
                                };
                                settings.Add(setting);

                                bmps.Add(Common.Load(br.ReadBytes(width * height * format.BitDepth / 8), setting));
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
