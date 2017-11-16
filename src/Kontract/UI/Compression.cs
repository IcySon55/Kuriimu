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
    public static class CompressionTools
    {
        static ToolStripMenuItem AddCompressionTab(ToolStripMenuItem compressTab, Lazy<ICompressionCollection, ICompressionMetaData> compression, string modTabPath = "", bool compTab = true, int count = 0)
        {
            var tabPath = (modTabPath != "") ? modTabPath : compression.Metadata.TabPathCompress;

            if (tabPath == "") return compressTab;

            string[] parts = compTab ?
                (tabPath.Contains(',')) ?
                    tabPath.Split(',')[0].Split('/') :
                    tabPath.Split('/') :
                compression.Metadata.TabPathDecompress.Split('/');

            if (count == parts.Length - 1)
            {
                if (compTab) compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Compress));
                else compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Decompress));
                compressTab.DropDownItems[compressTab.DropDownItems.Count - 1].Tag = compression;
                if (tabPath.Contains(',')) compressTab.DropDownItems[compressTab.DropDownItems.Count - 1].Name = tabPath.Split(',')[1];
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
                    AddCompressionTab((ToolStripMenuItem)duplicate, compression, modTabPath, compTab, count + 1);
                }
                else
                {
                    compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null));
                    AddCompressionTab((ToolStripMenuItem)compressTab.DropDownItems[compressTab.DropDownItems.Count - 1], compression, modTabPath, compTab, count + 1);
                }
            }

            return compressTab;
        }

        static ToolStripMenuItem AddCompressionTab(ToolStripMenuItem compressTab, Lazy<ICompression, ICompressionMetaData> compression, bool compTab = true, int count = 0)
        {
            if (compTab)
            {
                if (compression.Metadata.TabPathCompress == "") return compressTab;
            }
            else
            {
                if (compression.Metadata.TabPathDecompress == "") return compressTab;
            }

            string[] parts = compTab ?
                (compression.Metadata.TabPathCompress.Contains(',')) ?
                    compression.Metadata.TabPathCompress.Split(',')[0].Split('/') :
                    compression.Metadata.TabPathCompress.Split('/') :
                compression.Metadata.TabPathDecompress.Split('/');

            if (count == parts.Length - 1)
            {
                if (compTab) compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Compress));
                else compressTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Decompress));
                compressTab.DropDownItems[compressTab.DropDownItems.Count - 1].Tag = compression;
                if (compression.Metadata.TabPathCompress.Contains(',')) compressTab.DropDownItems[compressTab.DropDownItems.Count - 1].Name = compression.Metadata.TabPathCompress.Split(',')[1];
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

        public static void LoadCompressionTools(ToolStripMenuItem tsb, List<Lazy<ICompression, ICompressionMetaData>> compressions, List<Lazy<ICompressionCollection, ICompressionMetaData>> compressionColls)
        {
            tsb.DropDownItems.Clear();

            tsb.DropDownItems.Add(new ToolStripMenuItem("Compress", null));
            tsb.DropDownItems.Add(new ToolStripMenuItem("Decompress", null));

            var compressTab = (ToolStripMenuItem)tsb.DropDownItems[0];
            var decompressTab = (ToolStripMenuItem)tsb.DropDownItems[1];

            //Adding compression collections
            for (int i = 0; i < compressionColls.Count; i++)
            {
                if (CheckFormatting(compressionColls[i].Metadata.TabPathCompress))
                {
                    var orig = compressionColls[i].Metadata.TabPathCompress;
                    var parts = compressionColls[i].Metadata.TabPathCompress.Split(';');
                    for (int j = 0; j < parts.Count(); j++)
                    {
                        compressTab = AddCompressionTab(compressTab, compressionColls[i], parts[j]);
                    }
                }
                decompressTab = AddCompressionTab(decompressTab, compressionColls[i], "", false);
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

            FileStream openFile;
            FileStream saveFile;

            ICompressionMetaData meta;
            ICompression comp = null;
            ICompressionCollection compColl = null;

            try
            {
                var tag = (Lazy<ICompression, ICompressionMetaData>)tsi.Tag;
                meta = tag.Metadata;
                comp = tag.Value;
                if (!Shared.PrepareFiles("Open a " + tag.Metadata.Name + " compressed file...", "Save your decompressed file...", ".decomp", out openFile, out saveFile)) return;
            }
            catch
            {
                var tag = (Lazy<ICompressionCollection, ICompressionMetaData>)tsi.Tag;
                meta = tag.Metadata;
                compColl = tag.Value;
                if (!Shared.PrepareFiles("Open a " + tag.Metadata.Name + " compressed file...", "Save your decompressed file...", ".decomp", out openFile, out saveFile)) return;
            }

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    if (meta.TabPathCompress.Contains(','))
                        outFs.Write(compColl.Decompress(openFile, 0));
                    else
                        outFs.Write(comp.Decompress(openFile, 0));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Successfully decompressed {Path.GetFileName(openFile.Name)}.", tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Compress(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;

            FileStream openFile;
            FileStream saveFile;

            ICompressionMetaData meta;
            ICompression comp = null;
            ICompressionCollection compColl = null;

            try
            {
                var tag = (Lazy<ICompression, ICompressionMetaData>)tsi.Tag;
                meta = tag.Metadata;
                comp = tag.Value;
                if (!Shared.PrepareFiles("Open a decompressed " + tag.Metadata.Name + " file...", "Save your compressed file...", ".comp", out openFile, out saveFile, true)) return;
            }
            catch
            {
                var tag = (Lazy<ICompressionCollection, ICompressionMetaData>)tsi.Tag;
                meta = tag.Metadata;
                compColl = tag.Value;
                if (!Shared.PrepareFiles("Open a decompressed " + tag.Metadata.Name + " file...", "Save your compressed file...", ".comp", out openFile, out saveFile, true)) return;
            }

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    if (meta.TabPathCompress.Contains(','))
                    {
                        Byte.TryParse(tsi.Name, out var method);
                        compColl.SetMethod(method);
                        outFs.Write(compColl.Compress(openFile));
                    }
                    else
                    {
                        outFs.Write(comp.Compress(openFile));
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