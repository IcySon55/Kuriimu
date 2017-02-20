using System.Collections.Generic;
using System.Drawing;

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
		string GetKuriimuString(string rawString);
		string GetRawString(string kuriimuString);

		// Settings
		string Scene { get; set; }
		string PlayerName { get; set; }
		bool ShowWhitespace { get; set; }

		// Previewer
		IList<Bitmap> GeneratePages(IEntry entry);
		IEnumerable<string> GetScenes();
	}

	public sealed class DefaultGameHandler : IGameHandler
	{
		public string Name => "No Game";
		public Image Icon { get; }

		public bool HandlerCanGeneratePreviews => false;

		public string GetKuriimuString(string rawString) => rawString;
		public string GetRawString(string kuriimuString) => kuriimuString;

		public string Scene { get; set; } = string.Empty;
		public string PlayerName { get; set; } = string.Empty;
		public bool ShowWhitespace { get; set; } = false;

		public IList<Bitmap> GeneratePages(IEntry entry) => new List<Bitmap>();
		public IEnumerable<string> GetScenes() => new List<string>();

		public DefaultGameHandler(Image icon) { Icon = icon; }
	}
}