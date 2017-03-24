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
		bool CanDeleteFiles { get; } // Is the plugin able to delete files?
		bool CanAddDirectories { get; } // Is the plugin able to add directories?
		bool CanRenameDirectories { get; } // Is the plugin able to rename directories?
		bool CanDeleteDirectories { get; } // Is the plugin able to delete directories?
		bool CanSave { get; } // Is saving supported?

		// I/O
		FileInfo FileInfo { get; set; }
		bool Identify(string filename); // Determines if the given file is opened by the plugin.
		LoadResult Load(string filename);
		SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...

		// Files
		string Compression { get; } // Compression type used by the archive (can be overriden by individual files)
		bool FilesHaveVaryingCompressions { get; } // Do files in this archive have verying compression formats?
		IEnumerable<ArchiveFileInfo> Files { get; } // File list.
		bool AddFile(ArchiveFileInfo afi);
		byte[] GetFile(ArchiveFileInfo afi);
		bool DeleteFile(ArchiveFileInfo afi);

		// Features
		bool ShowProperties(Icon icon);
	}

	public class ArchiveFileInfo // This might need to be an interface.
	{
		public string Filename { get; set; } // Complete filename including path and extension.
		public string Compression { get; } // Compression type used by the file.

		public long Filesize { get; set; } // Size of the file.
		public long CompressedSize { get; set; } // Size of file when compressed.

		public FileInfo FileInfo { get; set; } // If this is not null, then the file is on storage media

		public ArchiveFileInfo()
		{
			Filename = string.Empty;
			Filesize = 0;

			Compression = "(none)";
			CompressedSize = 0;

			FileInfo = null;
		}

		public ArchiveFileInfo(FileInfo fi)
		{
			Filename = fi.Name;
			Filesize = fi.Length;

			Compression = "(none)";
			CompressedSize = 0;

			FileInfo = fi;
		}
	}
}