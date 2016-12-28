using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kuriimu
{
	static class Program
	{
#if DEBUG
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool AllocConsole();
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool FreeConsole();
#endif

		[STAThread]
		static void Main(string[] args)
		{
#if DEBUG
			AllocConsole();
#endif

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new frmMain(args));

#if DEBUG
			FreeConsole();
#endif
		}
	}
}