using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Kuriimu.Contract.UI;

namespace Kuriimu.Contract
{
    public static class Tools
    {
        public static string LoadFileFilters(IEnumerable<IFileAdapter> fileAdapters)
        {
            var alltypes = fileAdapters.Select(x => new { x.Description, Extension = x.Extension.ToLower() }).ToList();

            // Add two special cases at start and end
            if (alltypes.Count > 0) alltypes.Insert(0, new { Description = "All Supported Files", Extension = string.Join(";", alltypes.Select(x => x.Extension)) });
            alltypes.Add(new { Description = "All Files", Extension = "*.*" });

            return string.Join("|", alltypes.Select(x => $"{x.Description} ({x.Extension})|{x.Extension}"));
        }

        public static string LoadImageFilters(IEnumerable<IImageAdapter> imageAdapters)
        {
            var alltypes = imageAdapters.Select(x => new { x.Description, Extension = x.Extension.ToLower() }).ToList();

            // Add two special cases at start and end
            if (alltypes.Count > 0) alltypes.Insert(0, new { Description = "All Supported Files", Extension = string.Join(";", alltypes.Select(x => x.Extension)) });
            alltypes.Add(new { Description = "All Files", Extension = "*.*" });

            return string.Join("|", alltypes.Select(x => $"{x.Description} ({x.Extension})|{x.Extension}"));
        }

        public static string LoadArchiveFilters(IEnumerable<IArchiveManager> archiveManagers)
        {
            var alltypes = archiveManagers.Select(x => new { x.Description, Extension = x.Extension.ToLower() }).ToList();

            // Add two special cases at start and end
            if (alltypes.Count > 0) alltypes.Insert(0, new { Description = "All Supported Files", Extension = string.Join(";", alltypes.Select(x => x.Extension)) });
            alltypes.Add(new { Description = "All Files", Extension = "*.*" });

            return string.Join("|", alltypes.Select(x => $"{x.Description} ({x.Extension})|{x.Extension}"));
        }

        public static List<IGameHandler> LoadGameHandlers(string pluginPath, ToolStripDropDownButton tsb, Image noGameIcon, EventHandler selectedIndexChanged)
        {
            tsb.DropDownItems.Clear();

            List<IGameHandler> gameHandlers = new List<IGameHandler> { new DefaultGameHandler(noGameIcon) };
            gameHandlers.AddRange(PluginLoader<IGameHandler>.LoadPlugins(pluginPath, "game*.dll"));
            foreach (IGameHandler gameHandler in gameHandlers)
            {
                ToolStripMenuItem tsiGameHandler = new ToolStripMenuItem(gameHandler.Name, gameHandler.Icon, selectedIndexChanged);
                tsiGameHandler.Tag = gameHandler;
                tsb.DropDownItems.Add(tsiGameHandler);
            }

            return gameHandlers;
        }

        public static void LoadSupportedEncodings(ComboBox cmb, Encoding encoding)
        {
            List<ListItem> items = new List<ListItem>();
            foreach (EncodingInfo enc in Encoding.GetEncodings())
            {
                string name = enc.GetEncoding().EncodingName;
                if (name.Contains("ASCII") || name.Contains("Shift-JIS") || (name.Contains("Unicode") && !name.Contains("32")))
                    items.Add(new ListItem(name.Replace("US-", ""), enc.GetEncoding()));
            }
            items.Sort();

            cmb.DisplayMember = "Text";
            cmb.ValueMember = "Value";
            cmb.DataSource = items;
            cmb.SelectedValue = encoding;
        }

        public static TreeNode FindNodeByIEntry(this TreeView tre, IEntry entry)
        {
            TreeNode result = null;

            foreach (TreeNode node in tre.Nodes)
            {
                if (node.Tag == entry)
                    result = node;

                if (result == null)
                    foreach (TreeNode subNode in node.Nodes)
                        if (subNode.Tag == entry)
                        {
                            result = subNode;
                            break;
                        }

                if (result != null)
                    break;
            }

            return result;
        }

        public static void SelectNodeByIEntry(this TreeView tre, IEntry entry)
        {
            TreeNode result = null;

            foreach (TreeNode node in tre.Nodes)
            {
                if (node.Tag == entry)
                    result = node;

                if (result == null)
                    foreach (TreeNode subNode in node.Nodes)
                        if (subNode.Tag == entry)
                        {
                            result = subNode;
                            break;
                        }

                if (result != null)
                    break;
            }

            tre.SelectedNode = result;
        }

        public static string GetExtension(string path)
        {
            return Regex.Match(path, @"\..+$").Value;
        }

        public static void DoubleBuffer(Control ctrl, bool doubleBuffered)
        {
            ctrl.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, doubleBuffered, null);
        }
    }
}