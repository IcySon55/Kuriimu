using System;
using System.IO;
using System.Windows.Forms;
using Kuriimu.Compression;
using Kuriimu.IO;
using Kuriimu.CTR;

namespace Kuriimu.UI
{
    public static class EncryptionTools
    {
        public static void LoadEncryptionTools(ToolStripMenuItem tsb)
        {
            ToolStripMenuItem tsb2;
            ToolStripMenuItem tsb3;
            tsb.DropDownItems.Clear();

            // 3DS
            tsb.DropDownItems.Add(new ToolStripMenuItem("3DS", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[0];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem(".3ds", null, Decrypt));
            tsb3.DropDownItems[0].Tag = Types.normal;
            tsb3.DropDownItems.Add(new ToolStripMenuItem(".cia", null, Decrypt));
            tsb3.DropDownItems[1].Tag = Types.CIA;
            /*tsb3.DropDownItems.Add(new ToolStripMenuItem("BOSS", null, Decrypt));
            tsb3.DropDownItems[2].Tag = Types.BOSS;*/
        }

        public static void Decrypt(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var name = (tsi.Tag.ToString() == "normal") ? "3DS" : tsi.Tag.ToString();

            if (!PrepareFiles("Open an encrypted " + name + " file...", "Save your decrypted file...", ".dec", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                using (var openBr = new BinaryReaderX(openFile))
                using (var outFs = new BinaryWriterX(saveFile))
                {
                    var engine = new AesEngine();
                    switch (tsi.Tag)
                    {
                        case Types.normal:
                            openBr.BaseStream.CopyTo(outFs.BaseStream);
                            openBr.BaseStream.Position = 0;
                            outFs.BaseStream.Position = 0;
                            engine.DecryptGameNCSD(openBr.BaseStream, outFs.BaseStream);
                            break;
                        case Types.CIA:
                            openBr.BaseStream.CopyTo(outFs.BaseStream);
                            openBr.BaseStream.Position = 0;
                            outFs.BaseStream.Position = 0;
                            engine.DecryptCIA(openBr.BaseStream, outFs.BaseStream);
                            break;
                            /*case Types.BOSS:
                                outFs.Write(engine.DecryptBOSS(openBr.ReadBytes((int)openBr.BaseStream.Length)));
                                break;*/
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            MessageBox.Show($"Successfully decrypted {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + saveExtension,
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

        public enum Types : byte
        {
            normal,
            CIA,
            BOSS
        }
    }
}
