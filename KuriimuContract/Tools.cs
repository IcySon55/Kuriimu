using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using KuriimuContract.Properties;
using System.Text;

namespace KuriimuContract
{
	public class Tools
	{
		public static Dictionary<string, IGameHandler> LoadGameHandlers(ToolStripDropDownButton tsb, Image noGameIcon, EventHandler selectedIndexChanged)
		{
			tsb.DropDownItems.Clear();
			ToolStripMenuItem tsiNoGame = new ToolStripMenuItem("No Game", noGameIcon, selectedIndexChanged);
			tsb.DropDownItems.Add(tsiNoGame);
			tsb.Text = tsiNoGame.Text;
			tsb.Image = tsiNoGame.Image;

			Dictionary<string, IGameHandler> gameHandlers = new Dictionary<string, IGameHandler>();
			foreach (IGameHandler gameHandler in PluginLoader<IGameHandler>.LoadPlugins(Settings.Default.PluginDirectory, "game*.dll"))
			{
				gameHandlers.Add(gameHandler.Name, gameHandler);

				ToolStripMenuItem tsiGameHandler = new ToolStripMenuItem(gameHandler.Name, gameHandler.Icon, selectedIndexChanged);
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

		public static void DoubleBuffer(Control ctrl, bool doubleBuffered)
		{
			ctrl.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, doubleBuffered, null);
		}
	}
}