using System;
using System.IO;
using System.Windows.Forms;
using Kuriimu.Compression;
using Kuriimu.IO;

namespace Kuriimu.UI
{
    public static class CompressionTools
    {
        public static void LoadCompressionTools(ToolStripMenuItem tsb)
        {
            ToolStripMenuItem tsb2;
            ToolStripMenuItem tsb3;
            tsb.DropDownItems.Clear();

            // CriWare
            tsb.DropDownItems.Add(new ToolStripMenuItem("CriWare", null));
            tsb2=(ToolStripMenuItem)tsb.DropDownItems[0];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.CriWare;

            // GZip
            tsb.DropDownItems.Add(new ToolStripMenuItem("GZip", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[1];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.GZip;
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Compression", null, Compress));
            tsb2.DropDownItems[1].Tag = Compression.GZip;

            // Huffman
            tsb.DropDownItems.Add(new ToolStripMenuItem("Huffman", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[2];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("4bit", null));
            tsb2.DropDownItems.Add(new ToolStripMenuItem("8bit", null));

            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb3.DropDownItems[0].Tag = Compression.Huff4;
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[1];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb3.DropDownItems[0].Tag = Compression.Huff8;

            // LZ10
            tsb.DropDownItems.Add(new ToolStripMenuItem("LZ10", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[3];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.LZ10;
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Compression", null, Compress));
            tsb2.DropDownItems[1].Tag = Compression.LZ10;

            // LZ11
            tsb.DropDownItems.Add(new ToolStripMenuItem("LZ11", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[4];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.LZ11;
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Compression", null, Compress));
            tsb2.DropDownItems[1].Tag = Compression.LZ11;

            // LZ77
            tsb.DropDownItems.Add(new ToolStripMenuItem("LZ77", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[5];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.LZ77;

            // LZ77 Backwards
            tsb.DropDownItems.Add(new ToolStripMenuItem("RevLZ77", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[6];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.RevLZ77;
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Compression", null, Compress));
            tsb2.DropDownItems[1].Tag = Compression.RevLZ77;

            // LZSS
            //tsb.DropDownItems.Add(new ToolStripMenuItem("LZ11", null, LZ11_Compress));

            // RLE
            tsb.DropDownItems.Add(new ToolStripMenuItem("RLE", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[7];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.RLE;
            /*tsb2.DropDownItems.Add(new ToolStripMenuItem("Compression", null, Compress));
            tsb2.DropDownItems[1].Tag = Compression.RLE;*/

            // ZLib
            tsb.DropDownItems.Add(new ToolStripMenuItem("ZLib", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[8];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompression", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression.ZLib;
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Compression", null, Compress));
            tsb2.DropDownItems[1].Tag = Compression.ZLib;
        }

        public static bool PrepareFiles(string openCaption, string saveCaption, string saveExtension, out FileStream openFile, out FileStream saveFile)
        {
            openFile = null;
            saveFile = null;

            var ofd = new OpenFileDialog
            {
                Title = openCaption,
                Filter = "All Files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != DialogResult.OK) return false;
            openFile = File.OpenRead(ofd.FileName);

            var sfd = new SaveFileDialog()
            {
                Title = saveCaption,
                FileName = Path.GetFileName(ofd.FileName) + saveExtension,
                Filter = "All Files (*.*)|*.*"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                openFile.Dispose();
                return false;
            }
            saveFile = File.Create(sfd.FileName);

            return true;
        }

        public static void Decompress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open a " + tsi.Tag.ToString() + " compressed file...", "Save your decompressed file...", ".decomp", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    switch (tsi.Tag)
                    {
                        case Compression.CriWare:
                            outFs.Write(CriWare.GetDecompressedBytes(openFile));
                            break;
                        case Compression.GZip:
                            outFs.Write(GZip.Decompress(openFile));
                            break;
                        case Compression.Huff4:
                            outFs.Write(Huffman.Decompress(openFile, 4, Huffman.GetDecompressedSize(openFile)));
                            break;
                        case Compression.Huff8:
                            outFs.Write(Huffman.Decompress(openFile, 8, Huffman.GetDecompressedSize(openFile)));
                            break;
                        case Compression.LZ10:
                            outFs.Write(LZ10.Decompress(openFile));
                            break;
                        case Compression.LZ11:
                            outFs.Write(LZ11.Decompress(openFile));
                            break;
                        case Compression.LZ77:
                            outFs.Write(LZ77.Decompress(openFile));
                            break;
                        /*case Compression.LZSS:
                            outFs.Write(LZSS.Decompress(openFile, LZSS.GetDecompressedSize(openFile)));
                            break;*/
                        case Compression.RevLZ77:
                            outFs.Write(RevLZ77.Decompress(openFile));
                            break;
                        case Compression.RLE:
                            outFs.Write(RLE.Decompress(openFile, RLE.GetDecompressedLength(openFile)));
                            break;
                        case Compression.ZLib:
                            outFs.Write(ZLib.Decompress(openFile));
                            break;
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully decompressed {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Compress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open a decompressed " + tsi.Tag.ToString() + "file...", "Save your compressed file...", ".comp", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    switch (tsi.Tag)
                    {
                        /*case Compression.CriLZSS:
                            outFs.Write(CriWare.Compress(openFile, 1));
                            break;
                        case Compression.CriHuff4:
                            outFs.Write(CriWare.Compress(openFile, 2));
                            break;
                        case Compression.CriHuff8:
                            outFs.Write(CriWare.Compress(openFile, 3));
                            break;
                        case Compression.CriRLE:
                            outFs.Write(CriWare.Compress(openFile, 4));
                            break;*/
                        case Compression.GZip:
                            outFs.Write(GZip.Compress(openFile));
                            break;
                        /*case Compression.Huff4:
                            outFs.Write(Huffman.Compress(openFile, 4));
                            break;
                        case Compression.Huff8:
                            outFs.Write(Huffman.Compress(openFile, 8));
                            break;*/
                        case Compression.LZ10:
                            outFs.Write(LZ10.Compress(openFile));
                            break;
                        case Compression.LZ11:
                            outFs.Write(LZ11.Compress(openFile));
                            break;
                        /*case Compression.LZSS:
                            outFs.Write(LZSS.Compress(openFile));
                            break;*/
                        case Compression.RevLZ77:
                            outFs.Write(RevLZ77.Compress(openFile));
                            break;
                        /*case Compression.RLE:
                            outFs.Write(RLE.Compress(openFile));
                            break;*/
                        case Compression.ZLib:
                            outFs.Write(ZLib.Compress(openFile));
                            break;
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully compressed {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public enum Compression : short
        {
            CriWare,
            CriHuff4,
            CriHuff8,
            CriLZSS,
            CriRLE,
            GZip,
            Huff4,
            Huff8,
            LZ10,
            LZ11,
            LZ77,
            RevLZ77,
            LZSS,
            RLE,
            ZLib
        }
    }
}
