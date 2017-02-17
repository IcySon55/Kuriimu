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

		// Previewer
		IEnumerable<Bitmap> Pages { get; }
		void GeneratePages(IEntry entry);
	}

	public sealed class DefaultGameHandler : IGameHandler
	{
		public string Name => "No Game";
		public Image Icon { get; }

		public bool HandlerCanGeneratePreviews => false;

		public string GetKuriimuString(string rawString) => rawString;
		public string GetRawString(string kuriimuString) => kuriimuString;

		public IEnumerable<Bitmap> Pages => new List<Bitmap>();
		public void GeneratePages(IEntry entry) { }

		public int Page { get; set; } = 1;
		public int PageCount => 1;

		public DefaultGameHandler(Image icon) { Icon = icon; }
	}
}