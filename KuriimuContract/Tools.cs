using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using KuriimuContract.Properties;

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

		public static void DoubleBuffer(Control ctrl, bool doubleBuffered)
		{
			ctrl.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, doubleBuffered, null);
		}
	}
}