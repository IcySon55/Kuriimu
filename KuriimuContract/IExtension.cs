using System.Drawing;
using System.Windows.Forms;

namespace KuriimuContract
{
	public interface IExtension
	{
		string Name { get; }
		Image Icon { get; }
		Form CreateInstance();
	}
}