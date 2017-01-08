using System.Drawing;

namespace KuriimuContract
{
	public interface IExtension
	{
		string Name { get; }
		Image Icon { get; }
		void Show();
	}
}