using System;
using System.IO;
using System.Windows.Forms;
using Kontract.CTR;
using Kontract.Encryption;
using Kontract.IO;
using Kontract;

namespace Kontract.UI
{
    public static class EncryptionTools
    {
        public static void LoadEncryptionTools(ToolStripMenuItem tsb)
        {
            ToolStripMenuItem tsb2;
            ToolStripMenuItem tsb3;
            ToolStripMenuItem tsb4;
            tsb.DropDownItems.Clear();

            //General
            tsb.DropDownItems.Add(new ToolStripMenuItem("General", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[0];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Blowfish", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("CBC", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[0];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Encrypt", null, Encrypt));
            tsb4.DropDownItems[0].Tag = Types.BlowFishCBC;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null, Decrypt));
            tsb4.DropDownItems[1].Tag = Types.BlowFishCBC;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("EBC", null));
            tsb4 = (ToolStripMenuItem)tsb3.DropDownItems[1];
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Encrypt", null, Encrypt));
            tsb4.DropDownItems[0].Tag = Types.BlowFishECB;
            tsb4.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null, Decrypt));
            tsb4.DropDownItems[1].Tag = Types.BlowFishECB;

            // 3DS
            tsb.DropDownItems.Add(new ToolStripMenuItem("3DS", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[1];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem(".3ds", null, Decrypt));
            tsb3.DropDownItems[0].Tag = Types.Normal;
            tsb3.DropDownItems.Add(new ToolStripMenuItem(".cia", null, Decrypt));
            tsb3.DropDownItems[1].Tag = Types.CIA;
            /*tsb3.DropDownItems.Add(new ToolStripMenuItem("BOSS", null, Decrypt));
            tsb3.DropDownItems[2].Tag = Types.BOSS;*/

            //Mobile
            tsb.DropDownItems.Add(new ToolStripMenuItem("Mobile", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[2];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("MT Framework", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Encrypt", null, Encrypt));
            tsb3.DropDownItems[0].Tag = Types.MTMobile;
            tsb3.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null, Decrypt));
            tsb3.DropDownItems[1].Tag = Types.MTMobile;

            //Switch
            tsb.DropDownItems.Add(new ToolStripMenuItem("Switch", null));
            tsb2 = (ToolStripMenuItem)tsb.DropDownItems[3];
            tsb2.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null));
            tsb3 = (ToolStripMenuItem)tsb2.DropDownItems[0];
            tsb3.DropDownItems.Add(new ToolStripMenuItem(".xci", null, Decrypt));
            tsb3.DropDownItems[0].Tag = Types.NSW_XCI;
            tsb3.DropDownItems.Add(new ToolStripMenuItem(".nca", null, Decrypt));
            tsb3.DropDownItems[0].Tag = Types.NSW_NCA;
        }

        public static void Decrypt(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var name = (tsi.Tag.ToString() == "normal") ? "3DS" : tsi.Tag.ToString();

            if (!Shared.PrepareFiles("Open an encrypted " + name + " file...", "Save your decrypted file...", ".dec", out FileStream openFile, out FileStream saveFile)) return;

            bool show = true;

            try
            {
                using (var openBr = new BinaryReaderX(openFile))
                using (var outFs = new BinaryWriterX(saveFile))
                {
                    switch (tsi.Tag)
                    {
                        case Types.BlowFishCBC:
                            var key = InputBox.Show("Input decryption key:", "Decrypt Blowfish");

                            if (key == String.Empty) throw new Exception("Key can't be empty!");
                            var bf = new BlowFish(key);
                            outFs.Write(bf.Decrypt_CBC(openBr.ReadAllBytes()));
                            break;
                        case Types.BlowFishECB:
                            key = InputBox.Show("Input decryption key:", "Decrypt Blowfish");

                            if (key == String.Empty) throw new Exception("Key can't be empty!");
                            bf = new BlowFish(key);
                            outFs.Write(bf.Decrypt_ECB(openBr.ReadAllBytes()));
                            break;
                        case Types.MTMobile:
                            var key1 = InputBox.Show("Input 1st decryption key:", "Decrypt MTMobile");
                            var key2 = InputBox.Show("Input 2nd decryption key:", "Decrypt MTMobile");

                            if (key1 == String.Empty || key2 == String.Empty) throw new Exception("Keys can't be empty!");
                            outFs.Write(MTFramework.Decrypt(openBr.BaseStream, key1, key2));
                            break;
                        case Types.Normal:
                            var engine = new AesEngine();
                            openBr.BaseStream.CopyTo(outFs.BaseStream);
                            openBr.BaseStream.Position = 0;
                            outFs.BaseStream.Position = 0;
                            engine.DecryptGameNCSD(openBr.BaseStream, outFs.BaseStream);
                            break;
                        case Types.CIA:
                            engine = new AesEngine();
                            openBr.BaseStream.CopyTo(outFs.BaseStream);
                            openBr.BaseStream.Position = 0;
                            outFs.BaseStream.Position = 0;
                            engine.DecryptCIA(openBr.BaseStream, outFs.BaseStream);
                            break;
                        /*case Types.BOSS:
                            outFs.Write(engine.DecryptBOSS(openBr.ReadBytes((int)openBr.BaseStream.Length)));
                            break;*/

                        case Types.NSW_XCI:
                            openBr.BaseStream.CopyTo(outFs.BaseStream);
                            openBr.BaseStream.Position = 0;
                            outFs.BaseStream.Position = 0;

                            var xci_header_key = Hexlify(InputBox.Show("Input XCI Header Key:", "Decrypt XCI"));
                            var nca_header_key = Hexlify(InputBox.Show("Input NCA Header Key:", "Decrypt XCI"));
                            Switch.DecryptXCI(openBr.BaseStream, outFs.BaseStream, xci_header_key, nca_header_key);

                            MessageBox.Show("XCI Header and all NCA headers were decrypted successfully!", "Decryption Success", MessageBoxButtons.OK);
                            show = false;
                            break;
                        case Types.NSW_NCA:
                            break;
                    }
                }

                if (show)
                    MessageBox.Show($"Successfully decrypted {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(saveFile.Name);
            }
        }

        public static byte[] Hexlify(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static void Encrypt(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            if (!Shared.PrepareFiles("Open a decrypted " + tsi?.Tag + " file...", "Save your encrypted file...", ".dec", out var openFile, out var saveFile, true)) return;

            try
            {
                using (var openFs = new BinaryReaderX(openFile))
                using (var outFs = new BinaryWriterX(saveFile))
                    switch (tsi?.Tag)
                    {
                        case Types.BlowFishCBC:
                            var key = InputBox.Show("Input encryption key:", "Encrypt Blowfish");

                            if (key == String.Empty) throw new Exception("Key can't be empty!");
                            var bf = new BlowFish(key);
                            outFs.Write(bf.Encrypt_CBC(openFs.ReadAllBytes()));
                            break;
                        case Types.BlowFishECB:
                            key = InputBox.Show("Input encryption key:", "Encrypt Blowfish");

                            if (key == String.Empty) throw new Exception("Key can't be empty!");
                            bf = new BlowFish(key);
                            outFs.Write(bf.Encrypt_ECB(openFs.ReadAllBytes()));
                            break;
                        case Types.MTMobile:
                            var key1 = InputBox.Show("Input 1st encryption key:", "Encrypt MTMobile");
                            var key2 = InputBox.Show("Input 2nd encryption key:", "Encrypt MTMobile");

                            if (key1 == String.Empty || key2 == String.Empty) throw new Exception("Keys can't be empty!");
                            outFs.Write(MTFramework.Encrypt(openFs.BaseStream, key1, key2));
                            break;
                    }

                MessageBox.Show($"Successfully encrypted {Path.GetFileName(openFile.Name)}.", tsi.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(saveFile.Name);
            }
        }

        public enum Types
        {
            //3DS
            Normal,
            CIA,
            BOSS,

            //Mobile
            BlowFishCBC,
            BlowFishECB,
            MTMobile,

            //Switch
            NSW_XCI,
            NSW_NCA
        }
    }
}
