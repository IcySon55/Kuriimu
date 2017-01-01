using System.Drawing;
using System.Text;

namespace KuriimuContract
{
	public interface IGameHandler
	{
		// Information
		string Name { get; }
		Image Icon { get; }

		// Feature Support
		bool HandlerCanGeneratePreviews { get; } // Is this handler capable of generating previews?

		// Features
		string GetString(byte[] text, Encoding encoding);
		byte[] GetBytes(string text, Encoding encoding);
		Bitmap GeneratePreview(byte[] text, Encoding encoding);
	}
}