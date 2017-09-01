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
            ToolStripMenuItem tsb4;
            ToolStripMenuItem tsb5;
            tsb.DropDownItems.Clear();

            //--------General---------
            tsb.DropDownItems.Add(new ToolStripMenuItem("General", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[0];
            //  GZip
            tsb2.DropDownItems.Add(new ToolStripMenuItem("GZip", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.GZip;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.GZip;
            //  ZLib
            tsb2.DropDownItems.Add(new ToolStripMenuItem("ZLib", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[1];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.ZLib;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.ZLib;

            //-------Nintendo---------
            tsb.DropDownItems.Add(new ToolStripMenuItem("Nintendo", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[0];

            //  Compress
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Compress", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("LZ10", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[0];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.NLZ10;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.NLZ10;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("LZ11", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[1];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.NLZ11;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.NLZ11;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("LZ60", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[2];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.NLZ60;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.NLZ60;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("LZ77", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[3];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.LZ77;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.LZ77;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("RevLZ77", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[4];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.RevLZ77;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.RevLZ77;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Huffman", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[5];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("4Bit", null));
            tsb5 = (ToolStripMenuItem)tsb4.DropDownItems[0];
            tsb5.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb5.DropDownItems[0].Tag = Compression.NHuff4;
            tsb5.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb5.DropDownItems[1].Tag = Compression.NHuff4;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("8Bit", null));
            tsb5 = (ToolStripMenuItem)tsb4.DropDownItems[1];
            tsb5.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb5.DropDownItems[0].Tag = Compression.NHuff8;
            tsb5.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb5.DropDownItems[1].Tag = Compression.NHuff8;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("RLE", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[6];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.NRLE;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.NRLE;

            //  Decompress
            //tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            //tsb3.DropDownItems[1].Tag = Compression.Nintendo;

            //  LZ10
            /*tsb2.DropDownItems.Add(new ToolStripMenuItem("LZ10", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.ZLib;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.ZLib;

            //  Decompress
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb2.DropDownItems[0].Tag = Compression;
            //    LZ77
            tsb3.DropDownItems.Add(new ToolStripMenuItem("LZ77", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[0];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.LZ77;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.LZ77;
            //    RevLZ77
            tsb3.DropDownItems.Add(new ToolStripMenuItem("RevLZ77", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[1];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.RevLZ77;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.RevLZ77;

            //  Huffman
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Huffman", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];

            //3DS
            tsb.DropDownItems.Add(new ToolStripMenuItem("3DS", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[0];

            // Level5
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.Level5.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.Level5;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.Level5;

            // GZip
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.GZip.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[1];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.GZip;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.GZip;

            // Huffman
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Huffman", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[2];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("4bit", null));
            tsb3.DropDownItems.Add(new ToolStripMenuItem("8bit", null));

            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[0];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.Huff4;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.Huff4;

            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[1];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb4.DropDownItems[0].Tag = Compression.Huff8;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb4.DropDownItems[1].Tag = Compression.Huff8;

            // LZ10
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.LZ10.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[3];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.LZ10;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.LZ10;

            // LZ11
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.LZ11.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[4];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.LZ11;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.LZ11;

            // LZ77
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.LZ77.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[5];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[0].Tag = Compression.LZ77;

            // LZ77 Backwards
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.RevLZ77.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[6];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.RevLZ77;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.RevLZ77;

            //LZECD
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.LZECD.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[7];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[0].Tag = Compression.LZECD;

            // LZSS
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.LZSS.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[8];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[0].Tag = Compression.LZSS;

            // RLE
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.RLE.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[9];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.RLE;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.RLE;

            // ZLib
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.ZLib.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[10];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Compress", null, Compress));
            tsb3.DropDownItems[0].Tag = Compression.ZLib;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[1].Tag = Compression.ZLib;


            //PS2
            tsb.DropDownItems.Add(new ToolStripMenuItem("PS2", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[1];

            // LZSS
            tsb2.DropDownItems.Add(new ToolStripMenuItem(Compression.LZSSVLE.ToString(), null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decompress", null, Decompress));
            tsb3.DropDownItems[0].Tag = Compression.LZSSVLE;*/
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
                        case Compression.Level5:
                            outFs.Write(Level5.Decompress(openFile));
                            break;
                        case Compression.GZip:
                            outFs.Write(GZip.Decompress(openFile));
                            break;
                        case Compression.NHuff4:
                            outFs.Write(Huffman.Decompress(openFile, 4));
                            break;
                        case Compression.NHuff8:
                            outFs.Write(Huffman.Decompress(openFile, 8));
                            break;
                        case Compression.NLZ10:
                            outFs.Write(LZ10.Decompress(openFile));
                            break;
                        case Compression.NLZ11:
                            outFs.Write(LZ11.Decompress(openFile));
                            break;
                        case Compression.LZ77:
                            outFs.Write(LZ77.Decompress(openFile));
                            break;
                        case Compression.LZ10VLE:
                            outFs.Write(LZSSVLE.Decompress(openFile));
                            break;
                        case Compression.RevLZ77:
                            outFs.Write(RevLZ77.Decompress(openFile));
                            break;
                        case Compression.LZECD:
                            outFs.Write(LZECD.Decompress(openFile));
                            break;
                        case Compression.NRLE:
                            outFs.Write(RLE.Decompress(openFile));
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
                        case Compression.Level5:
                            outFs.Write(Level5.Compress(openFile, Level5.Method.NoCompression));
                            break;
                        /*case Compression.L5LZSS:
                            outFs.Write(Level5.Compress(openFile, 1));
                            break;
                        case Compression.L5Huff4:
                            outFs.Write(Level5.Compress(openFile, 2));
                            break;
                        case Compression.L5Huff8:
                            outFs.Write(Level5.Compress(openFile, 3));
                            break;
                        case Compression.L5RLE:
                            outFs.Write(Level5.Compress(openFile, 4));
                            break;*/
                        case Compression.GZip:
                            outFs.Write(GZip.Compress(openFile));
                            break;
                        case Compression.NHuff4:
                            outFs.Write(Huffman.Compress(openFile, 4));
                            break;
                        case Compression.NHuff8:
                            outFs.Write(Huffman.Compress(openFile, 8));
                            break;
                        case Compression.NLZ10:
                            outFs.Write(LZ10.Compress(openFile));
                            break;
                        case Compression.NLZ11:
                            outFs.Write(LZ11.Compress(openFile));
                            break;
                        /*case Compression.LZSS:
                            outFs.Write(LZSS.Compress(openFile));
                            break;*/
                        case Compression.RevLZ77:
                            outFs.Write(RevLZ77.Compress(openFile));
                            break;
                        case Compression.NRLE:
                            outFs.Write(RLE.Compress(openFile));
                            break;
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
            ZLib,
            GZip,

            Level5,
            Nintendo,

            L5Huff4,
            L5Huff8,
            L5LZ10,
            L5RLE,

            NLZ10,
            NLZ11,
            NLZ60,
            NHuff4,
            NHuff8,
            NRLE,

            LZ77,
            RevLZ77,
            LZ10VLE,
            LZECD,
        }
    }
}