using System.Drawing;
using System.Text;

namespace KuriimuContract
{
	public interface IGameHandler
	{
		string Name { get; }
		Image Icon { get; }
		string GetString(byte[] text, Encoding encoding);
		byte[] GetBytes(string text, Encoding encoding);
		Bitmap GeneratePreview(byte[] text, Encoding encoding);
	}
}