using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CeteraDS.Hash;
using CeteraDS.Image;
using Kontract.IO;
using Microsoft.VisualBasic;
using System;

namespace image_nintendo.NCGLR
{
    public class NCGLR
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public NCLR nclr;
        public NCGR ncgr;
        public NRCS nrcs;

        bool usesMap = false;
        MapType type = MapType.None;

        public List<uint> cellSizeW = new List<uint> { 8, 16, 32, 64, 16, 32, 32, 64, 8, 8, 16, 32 };
        public List<uint> cellSizeH = new List<uint> { 8, 16, 32, 64, 8, 8, 16, 32, 16, 32, 32, 64 };

        public NCGLR(Stream ncgrS, Stream nclrS, Stream optS = null)
        {
            if (optS != null) usesMap = true;

            using (var ncgrR = new BinaryReaderX(ncgrS))
            using (var nclrR = new BinaryReaderX(nclrS))
            {
                var input = Interaction.InputBox("Please set a width and height for this graphic! (Format: <width>x<height>)", "Set default dimension", "256x192");
                CheckDimension(input);
                var width = Int32.Parse(input.Split('x')[0]);
                var height = Int32.Parse(input.Split('x')[1]);

                //Palette
                //Header
                var nclrHeader = nclrR.ReadStruct<GenericHeader>();

                //TTLP Header
                var ttlpHeader = nclrR.ReadStruct<TTLPHeader>();

                //get palette
                var pal = Common.GetPalette(nclrR.ReadBytes((int)ttlpHeader.dataSize), Format.BGR555);

                //PMCP Header
                var pmcpHeader = nclrR.ReadStruct<PMCPHeader>();

                nclr = new NCLR
                {
                    header = nclrHeader,
                    ttlpHeader = ttlpHeader,
                    pmcpHeader = pmcpHeader,
                    palID = nclrR.ReadMultiple<ushort>(pmcpHeader.palCount)
                };


                //Colour
                //Header
                var ncgrHeader = ncgrR.ReadStruct<GenericHeader>();

                //CHAR Header
                var charHeader = ncgrR.ReadStruct<CHARHeader>();

                //Colour indeces
                var clr = ncgrR.ReadBytes((int)charHeader.dataSize);

                //SOPC
                SOPCHeader sopcHeader = null;
                if (ncgrHeader.subSecCount >= 2)
                {
                    sopcHeader = ncgrR.ReadStruct<SOPCHeader>();
                }

                ncgr = new NCGR
                {
                    header = ncgrHeader,
                    charHeader = charHeader,
                    sopcHeader = sopcHeader
                };


                //TileMap
                if (usesMap)
                {
                    using (var optR = new BinaryReaderX(optS))
                    {
                        //CEII Resource used
                        if (optR.PeekString() == "RECN")
                        {
                            throw new Exception("CEII Resources not supported yet!");
                        }
                        else
                        {
                            throw new Exception("NSCR Resources not supported yet!");

                            //TileMap Header
                            /*var nrcsHeader = optR.ReadStruct<NRCSHeader>();
                            width = nrcsHeader.width;
                            height = nrcsHeader.height;

                            //Map
                            var map = optR.ReadMultiple<ushort>((int)nrcsHeader.dataSize / 2);*/
                        }
                    }
                }
                else
                {
                    //Image
                    var settings = new ImageSettings
                    {
                        Width = width,
                        Height = height,
                        TileSize = 8,
                        BitPerIndex = (charHeader.bitDepth == 4) ? BitLength.Bit8 : BitLength.Bit4
                    };
                    bmps.Add(Common.Load(clr, settings, pal));
                }
            }
        }

        public void CheckDimension(string input)
        {
            if (input.Split('x').Length != 2)
            {
                throw new Exception("Bad format!");
            }
            else
            {
                var val = input.Split('x');
                try
                {
                    var w = Int32.Parse(val[0]);
                    var h = Int32.Parse(val[0]);
                }
                catch
                {
                    throw new Exception("Bad number!");
                }
            }
        }

        public void DrawTile(Bitmap img, byte[] tile, uint tileBitDepth, int tileWidth, int tileHeight, IEnumerable<Color> pal)
        {
            var settings = new ImageSettings
            {
                Width = tileWidth,
                Height = tileHeight,
                BitPerIndex = (tileBitDepth == 3) ? BitLength.Bit4 : BitLength.Bit8
            };
            var bmp = Common.Load(tile, settings, pal);

            using (var g = Graphics.FromImage(img))
                g.DrawImage(bmp, new Point(20, 20));
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
