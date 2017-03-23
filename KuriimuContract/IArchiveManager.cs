using System;
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
		bool CanSave { get; } // Is saving supported?

		// I/O
		FileInfo FileInfo { get; set; }
		bool Identify(string filename); // Determines if the given file is opened by the plugin.
		LoadResult Load(string filename);
		SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...

		// Files
		CompressionType Compression { get; set; } // Compression type used by the archive (can be overriden by individual files)
		IEnumerable<ArchiveFileInfo> Files { get; } // File list.
		bool AddFile(ArchiveFileInfo afi);
		byte[] GetFile(ArchiveFileInfo afi);
		bool DeleteFile(ArchiveFileInfo afi);

		// Features
		bool ShowProperties(Icon icon);
	}
	public enum CompressionType
	{
		None,
		LZ10,
		LZ11,
		GZIP
	}

	public enum LocationType
	{
		Archive,
		Storage
	}

	public class ArchiveFileInfo // This might need to be an interface.
	{
		public string Filename { get; set; } // Complete filename including path and extension.
		public long Filesize { get; set; }

		public CompressionType Compression { get; set; } // Compression type used by the file.
		public long CompressedSize { get; set; } // Size of file when compressed.

		public LocationType Location { get; set; } // Where the current file is stored.
		public FileInfo FileInfo { get; set; }

		public ArchiveFileInfo()
		{
			Filename = string.Empty;
			Filesize = 0;

			Compression = CompressionType.None;
			CompressedSize = 0;

			Location = LocationType.Archive;
			FileInfo = null;
		}

		public ArchiveFileInfo(FileInfo fi)
		{
			Filename = fi.Name;
			Filesize = fi.Length;

			Compression = CompressionType.None;
			CompressedSize = fi.Length;

			Location = LocationType.Storage;
			FileInfo = fi;
		}
	}
}