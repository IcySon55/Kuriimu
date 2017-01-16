using System.Drawing;
using System.Windows.Forms;
using ext_director.Properties;
using KuriimuContract;

namespace ext_director
{
	public sealed class Director : IExtension
	{
		#region Properties

		public string Name => Settings.Default.PluginName;

		public Image Icon => Resources.icon;

		#endregion

		public Form CreateInstance()
		{
			frmExtension configure = new frmExtension();
			configure.StartPosition = FormStartPosition.CenterParent;
			return configure;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}