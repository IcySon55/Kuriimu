using System.Collections.Generic;
using System.Drawing;

namespace Kuriimu.Contract
{
    public interface IGameHandler
    {
        // Information
        string Name { get; }
        Image Icon { get; }

        // Feature Support
        bool HandlerHasSettings { get; } // Does the handler provide a custom settings dialog?
        bool HandlerCanGeneratePreviews { get; } // Is the handler capable of generating previews?

        // String Handling
        string GetKuriimuString(string rawString);
        string GetRawString(string kuriimuString);

        // Features
        bool ShowSettings(Icon icon); // Show the settings dialog
        IList<Bitmap> GeneratePreviews(TextEntry entry);
    }

    public sealed class DefaultGameHandler : IGameHandler
    {
        public string Name => "No Game";
        public Image Icon { get; }

        public bool HandlerHasSettings => false;
        public bool HandlerCanGeneratePreviews => false;

        public string GetKuriimuString(string rawString) => rawString;
        public string GetRawString(string kuriimuString) => kuriimuString;

        public bool ShowSettings(Icon icon) => false;
        public IList<Bitmap> GeneratePreviews(TextEntry entry) => new List<Bitmap>();

        public DefaultGameHandler(Image icon) { Icon = icon; }
    }
}
