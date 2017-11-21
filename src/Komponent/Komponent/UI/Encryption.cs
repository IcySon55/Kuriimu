using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Linq;
using Komponent.IO;
using Komponent.Interface;

namespace Komponent.UI
{
    public static class EncryptionTools
    {
        static ToolStripMenuItem AddEncryptionTab(ToolStripMenuItem encryptTab, Lazy<IEncryption, IEncryptionMetadata> encryption, bool encTab = true, int count = 0)
        {
            if (encTab)
            {
                if (encryption.Metadata.TabPathEncrypt == "") return encryptTab;
            }
            else
            {
                if (encryption.Metadata.TabPathDecrypt == "") return encryptTab;
            }

            string[] parts = encTab ? encryption.Metadata.TabPathEncrypt.Split('/') : encryption.Metadata.TabPathDecrypt.Split('/');

            if (count == parts.Length - 1)
            {
                if (encTab) encryptTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Encrypt));
                else encryptTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Decrypt));
                encryptTab.DropDownItems[encryptTab.DropDownItems.Count - 1].Tag = encryption;
                if (encryption.Metadata.TabPathEncrypt.Contains(',')) encryptTab.DropDownItems[encryptTab.DropDownItems.Count - 1].Name = encryption.Metadata.TabPathEncrypt.Split(',')[1];
            }
            else
            {
                ToolStripItem duplicate = null;
                for (int i = 0; i < encryptTab.DropDownItems.Count; i++)
                {
                    if (encryptTab.DropDownItems[i].Text == parts[count])
                    {
                        duplicate = encryptTab.DropDownItems[i];
                        break;
                    }
                }
                if (duplicate != null)
                {
                    AddEncryptionTab((ToolStripMenuItem)duplicate, encryption, encTab, count + 1);
                }
                else
                {
                    encryptTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null));
                    AddEncryptionTab((ToolStripMenuItem)encryptTab.DropDownItems[encryptTab.DropDownItems.Count - 1], encryption, encTab, count + 1);
                }
            }

            return encryptTab;
        }

        public static void LoadEncryptionTools(ToolStripMenuItem tsb, List<Lazy<IEncryption, IEncryptionMetadata>> encryptions)
        {
            tsb.DropDownItems.Clear();

            tsb.DropDownItems.Add(new ToolStripMenuItem("Encrypt", null));
            tsb.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null));

            var encryptTab = (ToolStripMenuItem)tsb.DropDownItems[0];
            var decryptTab = (ToolStripMenuItem)tsb.DropDownItems[1];

            //Adding single encryptions
            for (int i = 0; i < encryptions.Count; i++)
            {
                encryptTab = AddEncryptionTab(encryptTab, encryptions[i]);
                decryptTab = AddEncryptionTab(decryptTab, encryptions[i], false);
            }
        }

        public static void Decrypt(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var tag = (Lazy<IEncryption, IEncryptionMetadata>)tsi.Tag;

            if (!Shared.PrepareFiles("Open an encrypted " + tag.Metadata.Name + " file...", "Save your decrypted file...", ".dec", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                var decrypt = tag.Value.Decrypt(openFile);
                saveFile.Write(decrypt, 0, decrypt.Length);

                MessageBox.Show($"Successfully decrypted {Path.GetFileName(openFile.Name)}.", tsi.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(saveFile.Name);
            }
        }

        public static void Encrypt(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var tag = (Lazy<IEncryption, IEncryptionMetadata>)tsi.Tag;

            if (!Shared.PrepareFiles("Open a decrypted " + tag.Metadata.Name + " file...", "Save your encrypted file...", ".enc", out var openFile, out var saveFile, true)) return;

            try
            {
                var encrypt = tag.Value.Encrypt(openFile);
                saveFile.Write(encrypt, 0, encrypt.Length);

                MessageBox.Show($"Successfully encrypted {Path.GetFileName(openFile.Name)}.", tsi.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(saveFile.Name);
            }
        }
    }
}
