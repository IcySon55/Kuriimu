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
            tsb.DropDownItems.Clear();

            // CriWare
            tsb.DropDownItems.Add(new ToolStripMenuItem("CriWare Decompress", null, CriWare_Decompress));

            // LZ11
            tsb.DropDownItems.Add(new ToolStripMenuItem("LZ11 Compress", null, LZ11_Compress));
            tsb.DropDownItems.Add(new ToolStripMenuItem("LZ11 Decompress", null, LZ11_Decompress));
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

        public static void CriWare_Decompress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open a CriWare compressed file...", "Save your decompressed CriWare file...", ".decomp", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(CriWare.GetDecompressedBytes(openFile));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully decompressed {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void LZ11_Compress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open a decompressed LZ11 file...", "Save your compressed LZ11 file...", ".lz11", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(LZ11.Compress(openFile));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully compressed {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void LZ11_Decompress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open an LZ11 compressed file...", "Save your decompressed LZ11 file...", ".decomp", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(LZ11.Decompress(openFile));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully decompressed {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
