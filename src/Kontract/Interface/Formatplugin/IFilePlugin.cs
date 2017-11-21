using System.Drawing;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace Kontract.Interface
{
    public interface IFilePlugin
    {
        string Name { get; } // i.e. return "KUP";
        string Description { get; } // i.e. return "Kuriimu Archive";
        string Extension { get; } // i.e. return "*.ext";
        string Author { get; } // i.e. return "IcySon55";
        string About { get; } // i.e. return "This is the KUP text adapter for Kuriimu.";

        // Feature Support
        bool FileHasExtendedProperties { get; } // Format provides an extended properties dialog?
        bool CanCreateNew { get; } // Is creating new files supported?
        bool CanSave { get; } // Is saving supported?

        // I/O
        FileInfo FileInfo { get; set; }
        //bool Identify(string filename); // Determines if the given file is supported by the plugin.
        //void New();
        //void Load(string filename);
        //void Save(string filename = ""); // A non-blank filename is provided when using Save As...
        bool Identify(Stream file, string filename);
        void Load(string filename);
        void Save(string filename = "");
        void New();

        // Features
        bool ShowProperties(Icon icon);
    }

    public interface IFilePlugin2
    {
        // Feature Support
        bool FileHasExtendedProperties { get; } // Format provides an extended properties dialog?
        bool CanIdentify { get; } // Can the format be identified explicitly?
        bool CanCreateNew { get; } // Is creating new files supported?
        bool CanSave { get; } // Is saving supported?

        // I/O
        FileInfo FileInfo { get; set; }
        //bool Identify(string filename); // Determines if the given file is supported by the plugin.
        //void New();
        //void Load(string filename);
        //void Save(string filename = ""); // A non-blank filename is provided when using Save As...
        bool Identify(Stream file, string filename);
        void Load(string filename);
        void Save(string filename = "");
        void New();

        // Features
        bool ShowProperties(Icon icon);
    }

    [MetadataAttribute]
    public class FilePluginMetadataAttribute : Attribute
    {
        public string Name { get; set; } // i.e. return "KUP";
        public string Description { get; set; } // i.e. return "Kuriimu Archive";
        public string Extension { get; set; } // i.e. return "*.ext";
        public string Author { get; set; } // i.e. return "IcySon55";
        public string About { get; set; } // i.e. return "This is the KUP text adapter for Kuriimu.";
    }

    public interface IFilePluginMetadata
    {
        string Name { get; }
        string Description { get; }
        string Extension { get; }
        string Author { get; }
        string About { get; }
    }
}
