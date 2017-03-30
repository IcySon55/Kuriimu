using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace KuriimuContract
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

		// Files
		IEnumerable<ArchiveFileInfo> Files { get; } // File list.
		bool AddFile(ArchiveFileInfo afi);
		bool RenameFile(ArchiveFileInfo afi);
		bool ReplaceFile(ArchiveFileInfo afi);
		bool DeleteFile(ArchiveFileInfo afi);

		// Features
		bool ShowProperties(Icon icon);
	}

	public class ArchiveFileInfo // This might need to be an interface.
	{
		public string Filename { get; set; } // Complete filename including path and extension.
		public virtual Stream FileData { get; set; } // Provides a stream to read the file data from.
		public ArchiveFileState State { get; set; } // Dictates the state of the FileData stream.

		public ArchiveFileInfo()
		{
			Filename = string.Empty;
			FileData = null;
			State = ArchiveFileState.Empty;
		}
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