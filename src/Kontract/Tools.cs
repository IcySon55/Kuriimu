using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Kontract.Interface;

namespace Kontract
{
    public static class Tools
    {
        public static string LoadFilters(IEnumerable<IFilePlugin> plugins)
        {
            var alltypes = plugins.Select(x => new { x.Description, Extension = x.Extension.ToLower() }).OrderBy(o => o.Description).ToList();

            // Add two special cases at start and end
            if (alltypes.Count > 0) alltypes.Insert(0, new { Description = "All Supported Files", Extension = string.Join(";", alltypes.Select(x => x.Extension).Distinct()) });
            alltypes.Add(new { Description = "All Files", Extension = "*.*" });

            return string.Join("|", alltypes.Select(x => $"{x.Description} ({x.Extension})|{x.Extension}"));
        }

        public static List<IGameHandler> LoadGameHandlers(string pluginPath, ToolStripDropDownButton tsb, System.Drawing.Image noGameIcon, EventHandler selectedIndexChanged)
        {
            tsb.DropDownItems.Clear();

            var gameHandlers = new List<IGameHandler> { new DefaultGameHandler(noGameIcon) };
            var gameHandlerPlugins = PluginLoader<IGameHandler>.LoadPlugins(pluginPath, "game*.dll").ToList();
            gameHandlerPlugins.Sort((lhs, rhs) => string.Compare(lhs.Name, rhs.Name, StringComparison.Ordinal));
            gameHandlers.AddRange(gameHandlerPlugins);
            foreach (var gameHandler in gameHandlers)
            {
                var tsiGameHandler = new ToolStripMenuItem(gameHandler.Name, gameHandler.Icon, selectedIndexChanged) {Tag = gameHandler};
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

        public static TreeNode FindNodeByTextEntry(this TreeView tre, TextEntry entry)
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

        public static void SelectNodeByTextEntry(this TreeView tre, TextEntry entry)
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

        public static void SelectNodeByNodeName(this TreeView tre, string name)
        {
            TreeNode result = null;

            foreach (TreeNode node in tre.Nodes)
            {
                if (node.Text == name)
                    result = node;

                if (result == null)
                    foreach (TreeNode subNode in node.Nodes)
                        if (subNode.Text == name)
                        {
                            result = subNode;
                            break;
                        }

                if (result != null)
                    break;
            }

            tre.SelectedNode = result;
        }

        public static void DoubleBuffer(Control ctrl, bool doubleBuffered)
        {
            ctrl.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, doubleBuffered, null);
        }
    }
}