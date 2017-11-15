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
        [ImportMany(typeof(ICompressionCollection))]
        public List<ICompressionCollection> compressionColls;

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

            string[] parts = compTab ?
                (compression.TabPathCompress.Contains(',')) ?
                    compression.TabPathCompress.Split(',')[0].Split('/') :
                    compression.TabPathCompress.Split('/') :
                compression.TabPathDecompress.Split('/');

            if (count == parts.Length - 1)
            {
                compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Compress));
                compressTab.DropDownItems[compressTab.DropDownItems.Count - 1].Tag = compression;
                if (compression.TabPathCompress.Contains(',')) compressTab.DropDownItems[compressTab.DropDownItems.Count - 1].Name = compression.TabPathCompress.Split(',')[1];
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

            var loadedComps = new CompressionLoad();
            var compressions = loadedComps.compressions;
            var compressionColls = loadedComps.compressionColls;

            //Adding compression collections
            for (int i = 0; i < compressionColls.Count; i++)
            {
                if (CheckFormatting(compressionColls[i].TabPathCompress))
                {
                    var orig = compressionColls[i].TabPathCompress;
                    var parts = compressionColls[i].TabPathCompress.Split(';');
                    for (int j = 0; j < parts.Count(); j++)
                    {
                        compressionColls[i].TabPathCompress = parts[j];
                        compressTab = AddCompressionTab(compressTab, compressionColls[i]);
                    }
                    compressionColls[i].TabPathCompress = orig;
                }
                decompressTab = AddCompressionTab(decompressTab, compressionColls[i], false);
            }

            //Adding single compressions
            for (int i = 0; i < compressions.Count; i++)
            {
                compressTab = AddCompressionTab(compressTab, compressions[i]);
                decompressTab = AddCompressionTab(decompressTab, compressions[i], false);
            }
        }

        static bool CheckFormatting(string check)
        {
            if (check.Split(';').Last() == "") return false;
            var compParts = check.Split(';');

            if (compParts.Select(p => p.Contains(',')).Count() != compParts.Count()) return false;
            var nums = compParts.Select(p => p.Split(',')[1]);

            if (nums.Select(n => Int32.TryParse(n, out int t) ? t : -1).Select(n2 => n2 != -1).Count() != nums.Count()) return false;

            return true;
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
                    if (tag.TabPathCompress.Contains(','))
                    {
                        var coll = (ICompressionCollection)tag;
                        Byte.TryParse(tsi.Name, out var method);
                        coll.SetMethod(method);
                        outFs.Write(coll.Compress(openFile));
                    }
                    else
                    {
                        outFs.Write(tag.Compress(openFile));
                    }
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