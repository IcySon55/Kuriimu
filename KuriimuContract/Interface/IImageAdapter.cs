using System.Drawing;
using System.IO;

namespace Kuriimu.Contract
{
    public interface IImageAdapter : IPlugin
    {
        // Feature Support
        bool FileHasExtendedProperties { get; } // Format provides an extended properties dialog?
        bool CanSave { get; } // Is saving supported?

        // I/O
        FileInfo FileInfo { get; set; }
        bool Identify(string filename); // Determines if the given file is opened by the plugin.
        void Load(string filename);
        void Save(string filename = ""); // A non-blank filename is provided when using Save As...

        // Images
        Bitmap Bitmap { get; set; }

        // Features
        bool ShowProperties(Icon icon);
    }
}
