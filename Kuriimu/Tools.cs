using System;
using System.Linq;
using KuriimuContract;
using System.Windows.Forms;
using System.Reflection;

namespace Kuriimu
{
	class UnusedTools
	{
		public static string ToLittleEndian(long value)
		{
			byte[] bytes = BitConverter.GetBytes((uint)value);
			bytes.Reverse();
			string retval = "";
			foreach (byte b in bytes)
				retval += b.ToString("X2");
			return retval;
		}
	}
}