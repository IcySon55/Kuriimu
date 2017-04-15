using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Kuriimu.Contract
{
    public interface IArchiveManager
    {
        // Information
        string Name { get; }
        string Description { get; } // i.e. return "Kuriimu Archive";
        string Extension { get; } // i.e. return "*.ext";
        string About { get; }

        // Feature Support
        bool ArchiveHasExtendedProperties { get; } // Format provides an extended properties dialog?
        bool CanAddFiles { get; } // Is the plugin able to add files?
        bool CanRenameFiles { get; } // Is the plugin able to rename files?
        bool CanReplaceFiles { get; } // Is the plugin able to replace files?
        bool CanDeleteFiles { get; } // Is the plugin able to delete files?
        bool CanSave { get; } // Is saving supported?

        // I/O
        FileInfo FileInfo { get; set; }
        bool Identify(string filename); // Determines if the given file is opened by the plugin.
        LoadResult Load(string filename);
        SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...
        void Unload(); // Instructs the archive manager to close open file handles.

        // Files
        IEnumerable<ArchiveFileInfo> Files { get; } // File list.
        bool AddFile(ArchiveFileInfo afi);
        bool DeleteFile(ArchiveFileInfo afi);

        // Features
        bool ShowProperties(Icon icon);
    }

    public class ArchiveFileInfo // This might need to be an interface.
    {
        public string FileName { get; set; } = string.Empty; // Complete filename including path and extension.
        public virtual Stream FileData { get; set; } = null; // Provides a stream to read the file data from.
        public virtual long? FileSize { get; set; } = null; // The length of the (uncompressed) stream
        public ArchiveFileState State { get; set; } = ArchiveFileState.Empty; // Dictates the state of the FileData stream.
    }

    public enum ArchiveFileState
    {
        Empty,
        Archived,
        Added,
        Replaced,
        Deleted
    }
}