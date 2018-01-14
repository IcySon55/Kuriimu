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

        bool _compressed = false;

        Header header;

        const int unkByteSize1 = 0x8;
        const int unkByteSize2 = 0x3;
        const int tileEntrySize = 2;
        const int imgDataEntry = 0x40;

        byte[] unkChunk1;
        byte[] unkChunk2;

        public CIMG(Stream input)
        {
            using (var br = new BinaryReaderX(GetStreamToUse(input)))
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

                //Get and Deflate imgData
                byte[] imgData = null;
                switch (header.imgFormat)
                {
                    case 0:
                        paletteFormat = new Palette(pal, new RGBA(5, 5, 5, 0, ByteOrder.LittleEndian, true), 4);
                        imgData = Inflate(new SubStream(br.BaseStream, header.imageDataOffset, header.imageTileCount * (imgDataEntry / 2)), tileData, imgDataEntry / 2);
                        break;
                    case 1:
                        paletteFormat = new Palette(pal, new RGBA(5, 5, 5, 0, ByteOrder.LittleEndian, true));
                        imgData = Inflate(new SubStream(br.BaseStream, header.imageDataOffset, header.imageTileCount * imgDataEntry), tileData, imgDataEntry);
                        break;
                    case 2:
                        paletteFormat = new AI(3, 5, pal, new RGBA(5, 5, 5, 0, ByteOrder.LittleEndian, true));
                        imgData = Inflate(new SubStream(br.BaseStream, header.imageDataOffset, header.imageTileCount * imgDataEntry), tileData, imgDataEntry);
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

        Stream GetStreamToUse(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var magic = br.ReadString(4);
                br.BaseStream.Position = 0;
                if (magic == "LIMG")
                {
                    return input;
                }
                else
                {
                    var decomp = Nintendo.Decompress(br.BaseStream);
                    br.Dispose();

                    _compressed = true;
                    return new MemoryStream(decomp);
                }
            }
        }

        byte[] Inflate(Stream imgData, List<short> tileData, int tileSize)
        {
            List<byte> result = new List<byte>();

            using (var br = new BinaryReaderX(imgData, true))
                foreach (var tileID in tileData)
                    if (tileID >= 0)
                    {
                        br.BaseStream.Position = tileID * tileSize;
                        result.AddRange(br.ReadBytes(tileSize));
                    }
                    else
                        result.AddRange(new byte[tileSize]);

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

                //Get Palette and deflate imgData
                byte[] pal = null;
                List<byte[]> tiles = null;
                List<short> tileIndeces = null;
                switch (header.imgFormat)
                {
                    case 0:
                        pal = (paletteFormat as Palette).paletteBytes;
                        (tiles, tileIndeces) = Deflate(newImgData, imgDataEntry / 2);
                        break;
                    case 1:
                        pal = (paletteFormat as Palette).paletteBytes;
                        (tiles, tileIndeces) = Deflate(newImgData, imgDataEntry);
                        break;
                    case 2:
                        pal = (paletteFormat as AI).paletteBytes;
                        (tiles, tileIndeces) = Deflate(newImgData, imgDataEntry);
                        break;
                    default:
                        throw new NotImplementedException($"Image mode {header.imgFormat} not supported.");
                }
                short colorCount = (short)(pal.Length / 2);

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
                bw.WriteAlignment(4);
                foreach (var index in tileIndeces) bw.Write(index);
                bw.WriteAlignment(4);
                foreach (var tile in tiles) bw.Write(tile);

                //Compress data with LZ10
                bw.BaseStream.Position = 0;
                if (_compressed)
                    File.WriteAllBytes(filename, Nintendo.Compress(ms, Nintendo.Method.LZ10));
                else
                    File.WriteAllBytes(filename, ms.ToArray());
            }
        }

        (List<byte[]>, List<short>) Deflate(byte[] newImgData, int tileSize)
        {
            var existingTiles = new List<byte[]>();
            var tileIndeces = new List<short>();

            short tileIndex = 0;
            var tileCount = newImgData.Length / tileSize;
            using (var imgBr = new BinaryReaderX(new MemoryStream(newImgData)))
                for (int i = 0; i < tileCount; i++)
                {
                    var tile = imgBr.ReadBytes(tileSize);
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
