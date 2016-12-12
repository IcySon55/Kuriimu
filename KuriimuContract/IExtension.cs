using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace KuriimuContract
{
	public interface IExtension
	{
		string Name { get; }
		Image Icon { get; }
		void Show();
		string ToString();
	}
}