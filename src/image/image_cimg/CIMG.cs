using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract.IO;
using System.Text;
using System;
using Kontract.Compression;
using Kontract.Interface;

namespace image_cimg
{
    public class CIMG
    {
        public List<Bitmap> bmps = new List<Bitmap>();
        public ImageSettings settings;
        IImageFormat paletteFormat = null;

        string magic;
        Header header;

        const int unkByteSize1 = 0x8;
        const int unkByteSize2 = 0x3;
        const int tileEntrySize = 2;
        const int imgDataEntry = 0x40;

        byte[] unkChunk1;
        byte[] unkChunk2;

        public CIMG(Stream input)
        {
            var decomp = Nintendo.Decompress(input);
            input.Dispose();

            using (var br = new BinaryReaderX(new MemoryStream(decomp)))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Get PaletteData
                br.BaseStream.Position = header.paletteOffset;
                var pal = br.ReadBytes(header.colorCount * tileEntrySize);

                //Get Unknowns
                br.BaseStream.Position = header.unkOffset1;
                unkChunk1 = br.ReadBytes(header.unkCount1 * unkByteSize1);
                unkChunk2 = br.ReadBytes(header.unkCount2 * unkByteSize2);

                //Get Tiledata
                br.BaseStream.Position = header.tileDataOffset;
                var tileData = br.ReadMultiple<short>(header.tileEntryCount);

                //Get Imagedata
                br.BaseStream.Position = header.imageDataOffset;
                var imgData = br.ReadBytes(header.imageTileCount * imgDataEntry);

                //Deflate imgData
                imgData = Inflate(imgData.ToList(), tileData);

                switch (header.imgFormat)
                {
                    case 1:
                        paletteFormat = new Palette(pal, new RGBA(5, 5, 5, 0, ByteOrder.LittleEndian, true));
                        break;
                    case 2:
                        paletteFormat = new AI(3, 5, pal, new RGBA(5, 5, 5, 0, ByteOrder.LittleEndian, true));
                        break;
                    default:
                        throw new NotImplementedException($"Image mode {header.imgFormat} not supported.");
                }
                settings = new ImageSettings
                {
                    Width = header.width,
                    Height = header.height,
                    Format = paletteFormat,
                    Swizzle = new NitroSwizzle(header.width, header.height)
                };
                bmps.Add(Common.Load(imgData, settings));
            }
        }

        byte[] Inflate(List<byte> imgData, List<short> tileData)
        {
            List<byte> result = new List<byte>();

            foreach (var tileID in tileData)
                result.AddRange(imgData.GetRange(tileID * imgDataEntry, imgDataEntry));

            return result.ToArray();
        }

        public void Save(string filename)
        {
            if (bmps[0].Width % 8 != 0 || bmps[0].Height % 8 != 0)
                throw new Exception("Width and Height need to be dividable by 8.");

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms))
            {
                //Get new image data
                settings.Swizzle = new NitroSwizzle(bmps[0].Width, bmps[0].Height);
                var newImgData = Common.Save(bmps[0], settings);

                //Get Palette
                byte[] pal = null;
                switch (header.imgFormat)
                {
                    case 1:
                        pal = (paletteFormat as Palette).paletteBytes;
                        break;
                    case 2:
                        pal = (paletteFormat as AI).paletteBytes;
                        break;
                    default:
                        throw new NotImplementedException($"Image mode {header.imgFormat} not supported.");
                }
                short colorCount = (short)(pal.Length / 2);

                //Deflate image data
                List<byte[]> tiles;
                List<short> tileIndeces;
                (tiles, tileIndeces) = Deflate(newImgData);

                //Update Header
                header.tileEntryCount = (short)tileIndeces.Count();
                header.imageDataOffset = (short)(header.tileDataOffset + header.tileEntryCount * tileEntrySize);
                header.imageTileCount = (short)tiles.Count();
                header.colorCount = (short)((colorCount + 0xf) & ~0xf);
                header.width = (short)bmps[0].Width;
                header.height = (short)bmps[0].Height;

                //Write Data
                bw.WriteStruct(header);
                bw.Write(pal);
                while (colorCount++ % 0x10 != 0) bw.Write((ushort)0x8000);
                foreach (var unk1 in unkChunk1) bw.Write(unk1);
                foreach (var unk2 in unkChunk2) bw.Write(unk2);
                foreach (var index in tileIndeces) bw.Write(index);
                foreach (var tile in tiles) bw.Write(tile);

                //Compress data with LZ10
                bw.BaseStream.Position = 0;
                File.WriteAllBytes(filename, Nintendo.Compress(ms, Nintendo.Method.LZ10));
            }
        }

        (List<byte[]>, List<short>) Deflate(byte[] newImgData)
        {
            var existingTiles = new List<byte[]>();
            var tileIndeces = new List<short>();

            short tileIndex = 0;
            var tileCount = newImgData.Length / imgDataEntry;
            using (var imgBr = new BinaryReaderX(new MemoryStream(newImgData)))
                for (int i = 0; i < tileCount; i++)
                {
                    var tile = imgBr.ReadBytes(imgDataEntry);
                    if (existingTiles.Find(et => et.Equals(tile)) == null)
                    {
                        existingTiles.Add(tile);
                        tileIndeces.Add(tileIndex++);
                    }
                    else
                        tileIndeces.Add((short)existingTiles.FindIndex(et => et.Equals(tile)));
                }

            return (existingTiles, tileIndeces);
        }
    }
}
