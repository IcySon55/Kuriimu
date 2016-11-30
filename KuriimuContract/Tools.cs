using System.Reflection;
using System.Windows.Forms;

namespace KuriimuContract
{
	public class Tools
	{
		public static void DoubleBuffer(Control ctrl, bool doubleBuffered)
		{
			ctrl.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, doubleBuffered, null);
		}
	}
}