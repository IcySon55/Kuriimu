using KuriimuContract.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace KuriimuContract
{
	public static class Tools
	{
		public static string LoadFileFilters(IEnumerable<IFileAdapter> fileAdapters)
		{
			var alltypes = fileAdapters.Select(x => new { x.Description, Ext = x.Extension.ToLower() }).ToList();

			// Add two special cases at start and end
			alltypes.Insert(0, new { Description = "All Supported Files", Ext = string.Join(";", alltypes.Select(x => x.Ext)) });
			alltypes.Add(new { Description = "All Files", Ext = "*.*" });

			return string.Join("|", alltypes.Select(x => $"{x.Description} ({x.Ext})|{x.Ext}"));
		}

		public static List<IGameHandler> LoadGameHandlers(ToolStripDropDownButton tsb, Image noGameIcon, EventHandler selectedIndexChanged)
		{
			tsb.DropDownItems.Clear();

			var gameHandlers = new List<IGameHandler> { new DefaultGameHandler { Icon = noGameIcon } };
			gameHandlers.AddRange(PluginLoader<IGameHandler>.LoadPlugins(Settings.Default.PluginDirectory, "game*.dll"));
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
			cmb.DisplayMember = "Text";
			cmb.ValueMember = "Value";
			cmb.DataSource = (from enc in Encoding.GetEncodings()
							  let name = enc.DisplayName
							  where name.Contains("ASCII")
								 || name.Contains("Shift-JIS")
								 || (name.Contains("Unicode") && !name.Contains("32"))
							  let newname = name.Replace("US-", "")
							  orderby newname
							  select new ListItem(newname, enc.GetEncoding()))
							  .ToList();
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

		public static void DoubleBuffer(Control ctrl, bool doubleBuffered)
		{
			ctrl.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, doubleBuffered, null);
		}
	}
}