using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.IO;

namespace image_aif
{
    public class AIF
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        byte[] header;
        public int dataOffset;
        public TexInfo texInfo;
        public ImageSettings settings;

        public AIF(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Get dataOffset
                dataOffset = 0;
                br.BaseStream.Position = 4;
                for (int i = 0; i < 4; i++)
                {
                    var size = br.ReadInt32();
                    dataOffset += size;
                    if (i == 0)
                    {
                        br.BaseStream.Position += 0x10;
                        dataOffset += 0x10;
                    }
                    br.BaseStream.Position += size - 4;
                }

                //Get information
                texInfo = new TexInfo();
                br.BaseStream.Position = 0x30;
                texInfo.format = br.ReadByte();
                br.BaseStream.Position = 0x38;
                texInfo.width = br.ReadUInt16();
                texInfo.height = br.ReadUInt16();
                br.BaseStream.Position = 0x4c;
                uint dataSize = br.ReadUInt32();
                br.BaseStream.Position = 0;
                header = br.ReadBytes(dataOffset);

                //Add Image
                br.BaseStream.Position = dataOffset;
                settings = new ImageSettings
                {
                    Width = texInfo.width,
                    Height = texInfo.height,
                    Format = Support.Format[texInfo.format],
                    Swizzle = new AIFSwizzle(texInfo.width, texInfo.height)
                };
                bmps.Add(Kontract.Image.Image.Load(br.ReadBytes((int)dataSize), settings));
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //Check original size
                if (bmps[0].Width != texInfo.width || bmps[0].Height != texInfo.height)
                    throw new System.Exception($"Image has to be {texInfo.width}x{texInfo.height}px!");

                //Write header
                bw.Write(header);

                //Write Image
                settings = new ImageSettings
                {
                    Width = texInfo.width,
                    Height = texInfo.height,
                    Format = Support.Format[texInfo.format],
                    Swizzle = new AIFSwizzle(texInfo.width, texInfo.height)
                };
                bw.Write(Kontract.Image.Image.Save(bmps[0], settings));
            }
        }
    }
}
