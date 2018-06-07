using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract.IO;
using Kontract.Compression;
using Kontract;
using System;
using System.Linq;

namespace image_nintendo.VCX
{
    /* VCL - Palette
     * VCG - Tiles
     * VCE - ??? 
     */

    public sealed class VCX
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        Palette paletteFormat;
        VCLHeader vclHeader;
        VCGHeader vcgHeader;
        VCEHeader vceHeader;

        public VCX(Stream inputVCG, Stream inputVCL, Stream inputVCE)
        {
            List<Color> pal;

            // Palette
            using (var brVCL = new BinaryReaderX(new MemoryStream(Nintendo.Decompress(inputVCL))))
            {
                vclHeader = brVCL.ReadStruct<VCLHeader>();

                paletteFormat = new Palette(brVCL.ReadBytes(vclHeader.colorCount * 2), new RGBA(5, 5, 5, 0, ByteOrder.LittleEndian, true));

                pal = paletteFormat.colors;
                pal[vclHeader.transparentId - 1] = Color.FromArgb(0, pal[vclHeader.transparentId - 1].R, pal[vclHeader.transparentId - 1].G, pal[vclHeader.transparentId - 1].B);
            }

            var width = Convert.ToInt32(InputBox.Show("Input image Width:", "Width"));
            var height = Convert.ToInt32(InputBox.Show("Input image Height:", "Height"));

            var settings = new ImageSettings
            {
                Width = width,
                Height = height,
                Format = new Palette(pal, 4),
                Swizzle = new NitroSwizzle(width, height)
            };

            // Tiles
            var tiles = new List<byte[]>();
            using (var brVCG = new BinaryReaderX(new MemoryStream(Nintendo.Decompress(inputVCG))))
            {
                vcgHeader = brVCG.ReadStruct<VCGHeader>();

                if (width * height != vcgHeader.dataSize * 2)
                    throw new InvalidOperationException($"Image dimensions {width}*{height} don't fit with dataSize.");

                //while (brVCG.BaseStream.Position < brVCG.BaseStream.Length)
                //    tiles.Add(brVCG.ReadBytes(4 * 4));

                // Temporary
                brVCG.BaseStream.Position = 0x10;
                bmps.Add(Common.Load(brVCG.ReadBytes(vcgHeader.dataSize), settings));
            }

            // Tile Map
            using (var brVCE = new BinaryReaderX(new MemoryStream(Nintendo.Decompress(inputVCE))))
            {
                vceHeader = brVCE.ReadStruct<VCEHeader>();

                brVCE.BaseStream.Position = vceHeader.offset0;
                var l0 = brVCE.ReadMultiple<VCEEntry0>(vceHeader.count0);
                brVCE.BaseStream.Position = vceHeader.offset1;
                var l1 = brVCE.ReadMultiple<VCEEntry1>(vceHeader.count1);
                brVCE.BaseStream.Position = vceHeader.offset2;
                var l2 = brVCE.ReadMultiple<VCEEntry2>(vceHeader.count2);
                brVCE.BaseStream.Position = vceHeader.offset3;
                var l3 = brVCE.ReadMultiple<VCEEntry3>(vceHeader.count3);

                //Merge Entries
                var merge = new List<MergedEntry>();
                foreach (var l in l0)
                    merge.Add(new MergedEntry
                    {
                        unk1 = l.unk1,
                        t1Entries = l1.GetRange(l.t1Index, l.count).Select(e => new MergedEntry.MergeT1
                        {
                            unk1 = e.unk1,
                            t2Entry = new MergedEntry.MergeT1.MergeT2
                            {
                                unk1 = l2[e.t2Index].unk1,
                                t3Entry = l3[l2[e.t2Index].t3Index]
                            }
                        }).ToList()
                    });
                //bmps.Add(Common.Load(brVCG.ReadBytes((int)(brVCG.BaseStream.Length - brVCG.BaseStream.Position)), settings, pal));
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
