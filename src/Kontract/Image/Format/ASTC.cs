using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Diagnostics;
using TgaLib;

namespace Kontract.Image.Format
{
    public class ASTC : IImageFormat
    {
        public int BitDepth { get; set; }
        public int BlockBitDepth { get; set; }
        public string FormatName { get; set; }

        ByteOrder byteOrder;

        List<(int, int)> legitBlockSizes = new List<(int, int)> { (4, 4), (5, 4), (5, 5), (6, 5), (6, 6), (8, 5), (8, 6), (8, 8), (10, 5), (10, 6), (10, 8), (10, 10), (12, 10), (12, 12) };
        (int, int) blockDim;
        int depth = 1;
        int depthDim = 1;

        public int Width = -1;
        public int Height = -1;

        public ASTC(int blockWidth, int blockHeight, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            if (!legitBlockSizes.Contains((blockWidth, blockHeight)))
                throw new Exception(blockWidth + "x" + blockHeight + " is an invalid block.");
            blockDim = (blockWidth, blockHeight);

            BitDepth = 128;
            BlockBitDepth = 128;

            this.byteOrder = byteOrder;

            FormatName = "ASTC" + blockWidth + "x" + blockHeight;
        }

        public IEnumerable<Color> Load(byte[] tex)
        {
            if (Width < 0 || Height < 0)
                throw new InvalidDataException("Height and Width has to be set for ASTC.");

            var astcFileName = "astcTmp.bin";
            var tgaFileName = "astcTmp.tga";

            ByteArrayToASTCFile(tex, astcFileName);
            var imgData = TranscodeASTCToTGA(astcFileName, tgaFileName);

            File.Delete(astcFileName);
            //File.Delete(tgaFileName);

            for (int i = 0; i < imgData.Length; i += 4)
                yield return Color.FromArgb(imgData[i + 3], imgData[i], imgData[i + 1], imgData[i + 2]);
        }

        private void ByteArrayToASTCFile(byte[] source, string fileName)
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true))
            {
                var tmp_width = (byteOrder == ByteOrder.LittleEndian) ?
                    new byte[] { (byte)(Width & 0xFF), (byte)((Width >> 8) & 0xFF), (byte)(Width >> 16) }
                    : new byte[] { (byte)(Width >> 16), (byte)((Width >> 8) & 0xFF), (byte)(Width & 0xFF) };
                var tmp_height = (byteOrder == ByteOrder.LittleEndian) ?
                    new byte[] { (byte)(Height & 0xFF), (byte)((Height >> 8) & 0xFF), (byte)(Height >> 16) }
                    : new byte[] { (byte)(Height >> 16), (byte)((Height >> 8) & 0xFF), (byte)(Height & 0xFF) };
                var tmp_depth = (byteOrder == ByteOrder.LittleEndian) ?
                    new byte[] { (byte)(depthDim & 0xFF), (byte)((depthDim >> 8) & 0xFF), (byte)(depthDim >> 16) }
                    : new byte[] { (byte)(depthDim >> 16), (byte)((depthDim >> 8) & 0xFF), (byte)(depthDim & 0xFF) };

                bw.Write(0x5ca1ab13);

                bw.Write((byte)blockDim.Item1);
                bw.Write((byte)blockDim.Item2);
                bw.Write((byte)depth);

                bw.Write(tmp_width);
                bw.Write(tmp_height);
                bw.Write(tmp_depth);

                bw.Write(source);
            }

            File.WriteAllBytes(fileName, ms.ToArray());
        }

        private byte[] TranscodeASTCToTGA(string astcFileName, string tgaFileName)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo
            {
                FileName = "astcenc.exe",
                Arguments = $"-d {astcFileName} {tgaFileName}",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();
            p.WaitForExit();

            if (!p.HasExited)
                p.Kill();

            byte[] imgData = null;
            using (var br = new BinaryReader(File.OpenRead(tgaFileName)))
            {
                imgData = new TgaImage(new BinaryReader(File.OpenRead(tgaFileName))).ImageBytes;
            }

            return imgData;
        }

        public byte[] Save(IEnumerable<Color> colors)
        {
            var astcencoder = new Support.ASTC.Encoder(blockDim.Item1, blockDim.Item2);

            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms))
                foreach (var color in colors)
                    astcencoder.Set(color, data =>
                    {
                        bw.Write(data);
                    });

            return ms.ToArray();
        }
    }
}
