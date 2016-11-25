using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace KuriimuContract
{
	public interface IControlCodeHandler
	{
		string Name { get; }
		Image Icon { get; }
		string GetString(byte[] text, Encoding encoding);
		byte[] GetBytes(string text, Encoding encoding);
	}
}