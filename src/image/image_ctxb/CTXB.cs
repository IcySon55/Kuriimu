using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;

/*Original image and data types by xdaniel and his tool Tharsis
 * https://github.com/xdanieldzd/Tharsis */

namespace image_ctxb
{
    public class CTXB
    {
        public Header header;
        List<Chunk> chunks = new List<Chunk>();

        public List<Bitmap> bmps = new List<Bitmap>();
        public List<ImageSettings> settingsList = new List<ImageSettings>();

        public CTXB(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Chunks
                for (int i = 0; i < header.chunkCount; i++)
                    chunks.Add(new Chunk(br.BaseStream));

                br.BaseStream.Position = header.texDataOffset;
                for (int i = 0; i < header.chunkCount; i++)
                {
                    for (int j = 0; j < chunks[i].texCount; j++)
                    {
                        var settings = new ImageSettings
                        {
                            Width = chunks[i].textures[j].width,
                            Height = chunks[i].textures[j].height,
                            Format = Support.Format[chunks[i].textures[j].imageFormat],
                            Swizzle = new CTRSwizzle(chunks[i].textures[j].width, chunks[i].textures[j].height)
                            /*Format = (chunks[i].textures[j].dataType == DataTypes.UnsignedShort565)
                                ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGB565)
                                : (chunks[i].textures[j].dataType == DataTypes.UnsignedShort5551)
                                    ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA5551)
                                    : (chunks[i].textures[j].dataType == DataTypes.UnsignedShort4444)
                                        ? ImageSettings.ConvertFormat(Cetera.Image.Format.RGBA4444)
                                        : ImageSettings.ConvertFormat(chunks[i].textures[j].imageFormat),
                            PadToPowerOf2 = false*/
                        };

                        settingsList.Add(settings);
                        bmps.Add(Kontract.Image.Image.Load(br.ReadBytes(chunks[i].textures[j].dataLength), settings));
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.BaseStream.Position = header.texDataOffset;
                var count = 0;
                for (int i = 0; i < header.chunkCount; i++)
                {
                    for (int j = 0; j < chunks[i].texCount; j++)
                    {
                        settingsList[count].Width = bmps[count].Width;
                        settingsList[count].Height = bmps[count].Height;

                        var resBmp = Kontract.Image.Image.Save(bmps[count], settingsList[count]);
                        bw.Write(resBmp);

                        chunks[i].textures[j].dataLength = resBmp.Length;
                        chunks[i].textures[j].height = (ushort)bmps[count].Height;
                        chunks[i].textures[j].width = (ushort)bmps[count].Width;

                        count++;
                    }
                }

                //Ctxb Header
                header.fileSize = (uint)bw.BaseStream.Length;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(header);

                //Chunks
                for (int i = 0; i < header.chunkCount; i++)
                {
                    chunks[i].Write(bw.BaseStream);
                    for (int j = 0; j < chunks[i].texCount; j++)
                    {
                        bw.WriteStruct(chunks[i].textures[j]);
                    }
                }
            }
        }
    }
}
