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

            ToolStripMenuItem tsi;

            // CriWare
            tsi = new ToolStripMenuItem("CriWare Decompress", null, CriWare_Decompress);
            tsb.DropDownItems.Add(tsi);
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

            if (!PrepareFiles("Open a CriWare compressed file", "Save your CriWare decompressed file", ".decomp", out string openFile, out string saveFile)) return;

            try
            {
                using (var fs = File.OpenRead(openFile))
                using (var outFs = new BinaryWriterX(File.Create(saveFile)))
                    outFs.Write(CriWare.GetDecompressedBytes(fs));
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
