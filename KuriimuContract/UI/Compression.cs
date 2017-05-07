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

        public static bool PrepareFiles(string openCaption, string saveCaption, string saveExtension, out string openFile, out string saveFile)
        {
            openFile = string.Empty;
            saveFile = string.Empty;

            var ofd = new OpenFileDialog
            {
                Title = openCaption,
                Filter = "All Files (*.*)|*.*"
            };

            if (ofd.ShowDialog() != DialogResult.OK) return false;

            var sfd = new SaveFileDialog()
            {
                Title = saveCaption,
                FileName = Path.GetFileName(ofd.FileName) + saveExtension,
                Filter = "All Files (*.*)|*.*"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return false;

            openFile = ofd.FileName;
            saveFile = sfd.FileName;

            return true;
        }

        public static void CriWare_Decompress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open a CriWare compressed file...", "Save your decompressed CriWare file...", ".decomp", out string openFile, out string saveFile)) return;

            try
            {
                using (var fs = File.OpenRead(openFile))
                using (var outFs = new BinaryWriterX(File.Create(saveFile)))
                    outFs.Write(CriWare.GetDecompressedBytes(fs));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully decompressed {Path.GetFileName(openFile)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void LZ11_Compress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open a decompressed LZ11 file...", "Save your compressed LZ11 file...", ".lz11", out string openFile, out string saveFile)) return;

            try
            {
                using (var fs = File.OpenRead(openFile))
                using (var outFs = new BinaryWriterX(File.Create(saveFile)))
                    outFs.Write(LZ11.Compress(fs));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully compressed {Path.GetFileName(saveFile)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void LZ11_Decompress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!PrepareFiles("Open an LZ11 compressed file...", "Save your decompressed LZ11 file...", ".decomp", out string openFile, out string saveFile)) return;

            try
            {
                using (var fs = File.OpenRead(openFile))
                using (var outFs = new BinaryWriterX(File.Create(saveFile)))
                    outFs.Write(LZ11.Decompress(fs));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully decompressed {Path.GetFileName(openFile)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
