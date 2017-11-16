using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace Kontract.UI
{
    public static class HashTools
    {
        static ToolStripMenuItem AddCreateTab(ToolStripMenuItem createTab, Lazy<IHash, IHashMetadata> hash, int count = 0)
        {
            if (hash.Metadata.TabPathCreate == "") return createTab;

            string[] parts = hash.Metadata.TabPathCreate.Split('/');

            if (count == parts.Length - 1)
            {
                createTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Create));
                createTab.DropDownItems[createTab.DropDownItems.Count - 1].Tag = hash;
                if (hash.Metadata.TabPathCreate.Contains(',')) createTab.DropDownItems[createTab.DropDownItems.Count - 1].Name = hash.Metadata.TabPathCreate.Split(',')[1];
            }
            else
            {
                ToolStripItem duplicate = null;
                for (int i = 0; i < createTab.DropDownItems.Count; i++)
                {
                    if (createTab.DropDownItems[i].Text == parts[count])
                    {
                        duplicate = createTab.DropDownItems[i];
                        break;
                    }
                }
                if (duplicate != null)
                {
                    AddCreateTab((ToolStripMenuItem)duplicate, hash, count + 1);
                }
                else
                {
                    createTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null));
                    AddCreateTab((ToolStripMenuItem)createTab.DropDownItems[createTab.DropDownItems.Count - 1], hash, count + 1);
                }
            }

            return createTab;
        }

        public static void LoadHashTools(ToolStripMenuItem tsb, List<Lazy<IHash, IHashMetadata>> hashes)
        {
            tsb.DropDownItems.Clear();

            tsb.DropDownItems.Add(new ToolStripMenuItem("Create", null));

            var createTab = (ToolStripMenuItem)tsb.DropDownItems[0];

            //Adding single hashes
            for (int i = 0; i < hashes.Count; i++)
            {
                createTab = AddCreateTab(createTab, hashes[i]);
            }
        }

        public static void Create(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var tag = (Lazy<IHash, IHashMetadata>)tsi.Tag;

            if (!Shared.PrepareFiles("Open a file...", "Save your hashed value...", ".hash", out var openFile, out var saveFile)) return;

            try
            {
                using (var inFs = new BinaryReaderX(openFile))
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(tag.Value.Create(inFs.ReadAllBytes(), 0));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Successfully hashed {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
