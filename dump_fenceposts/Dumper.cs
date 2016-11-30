using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KuriimuContract;
using System.Drawing;
using dump_fenceposts.Properties;
using System.Windows.Forms;

namespace dump_fenceposts
{
	public class Dumper : IDumper
	{
		#region Properties

		public string Name
		{
			get { return Settings.Default.PluginName; }
		}

		public Image Icon
		{
			get { return Resources.icon; }
		}

		#endregion

		public DialogResult ShowDialog()
		{
			Fenceposts configure = new Fenceposts();
			configure.StartPosition = FormStartPosition.CenterParent;
			return configure.ShowDialog();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}