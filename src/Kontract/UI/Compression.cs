using System;
using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Windows.Forms;
using Kontract.IO;
using Kontract.Interface;
using System.Linq;

namespace Kontract.UI
{
    class CompressionLoad
    {
        [ImportMany(typeof(ICompression))]
        public List<ICompression> compressions;

        public CompressionLoad()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    public static class CompressionTools
    {
        static ToolStripMenuItem AddCompressionTab(ToolStripMenuItem compressTab, ICompression compression, bool compTab = true, int count = 0)
        {
            if (compTab)
            {
                if (compression.TabPathCompress == "") return compressTab;
            }
            else
            {
                if (compression.TabPathDecompress == "") return compressTab;
            }

            string[] parts = compTab ? compression.TabPathCompress.Split('/') : compression.TabPathDecompress.Split('/');

            if (count == parts.Length - 1)
            {
                compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Compress));
                compressTab.DropDownItems[compressTab.DropDownItems.Count - 1].Tag = compression;
            }
            else
            {
                ToolStripItem duplicate = null;
                for (int i = 0; i < compressTab.DropDownItems.Count; i++)
                {
                    if (compressTab.DropDownItems[i].Text == parts[count])
                    {
                        duplicate = compressTab.DropDownItems[i];
                        break;
                    }
                }
                if (duplicate != null)
                {
                    AddCompressionTab((ToolStripMenuItem)duplicate, compression, compTab, count + 1);
                }
                else
                {
                    compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null));
                    AddCompressionTab((ToolStripMenuItem)compressTab.DropDownItems[compressTab.DropDownItems.Count - 1], compression, compTab, count + 1);
                }
            }

            return compressTab;
        }

        public static void LoadCompressionTools(ToolStripMenuItem tsb)
        {
            tsb.DropDownItems.Clear();

            tsb.DropDownItems.Add(new ToolStripMenuItem("Compress", null));
            tsb.DropDownItems.Add(new ToolStripMenuItem("Decompress", null));

            var compressTab = (ToolStripMenuItem)tsb.DropDownItems[0];
            var decompressTab = (ToolStripMenuItem)tsb.DropDownItems[1];

            var compressions = new CompressionLoad().compressions;

            for (int i = 0; i < compressions.Count; i++)
            {
                compressTab = AddCompressionTab(compressTab, compressions[i]);
                decompressTab = AddCompressionTab(decompressTab, compressions[i], false);
            }
        }

        public static void Decompress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var tag = (ICompression)tsi.Tag;

            if (!Shared.PrepareFiles("Open a " + tag.Name + " compressed file...", "Save your decompressed file...", ".decomp", out var openFile, out var saveFile)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(tag.Decompress(openFile, 0));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Successfully decompressed {Path.GetFileName(openFile.Name)}.", tsi.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Compress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var tag = (ICompression)tsi.Tag;

            if (!Shared.PrepareFiles("Open a decompressed " + tag.Name + " file...", "Save your compressed file...", ".decomp", out var openFile, out var saveFile, true)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(tag.Compress(openFile));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Successfully compressed {Path.GetFileName(openFile.Name)}.", tsi.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}