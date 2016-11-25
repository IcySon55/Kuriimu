using System;
using System.Linq;
using KuriimuContract;

namespace Kuriimu
{
	class Tools
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