using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace KuriimuContract
{
	public interface IFileAdapter
	{
		// Information
		string Name { get; }
		string Description { get; } // i.e. return "Kuriimu Archive";
		string Extension { get; } // i.e. return "*.ext";
		string About { get; }

		// Feature Support
		bool FileHasExtendedProperties { get; } // Format provides an extended properties dialog?
		bool CanSave { get; } // Is saving supported?
		bool CanAddEntries { get; } // Is adding entries supported?
		bool CanRenameEntries { get; } // Is renaming entries supported?
		bool CanRemoveEntries { get; } // Is removing entries supported?
		bool EntriesHaveSubEntries { get; } // Do entries contain multiple text values?
		bool OnlySubEntriesHaveText { get; } // Set this to true if only the sub entries contain text data.
		bool EntriesHaveUniqueNames { get; } // Must entry names be unique?
		bool EntriesHaveExtendedProperties { get; } // Entries provides an extended properties dialog?

		// I/O
		FileInfo FileInfo { get; set; }
		LoadResult Load(string filename);
		SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...
		bool Identify(string filename); // Determines if the given file is opened by the plugin.

		// Entries
		IEnumerable<IEntry> Entries { get; }
		List<string> NameList { get; }
		string NameFilter { get; } // This must be a regular expression that the incoming names must match. Use @".*" to accept any charcter.
		int NameMaxLength { get; }

		// Features
		bool ShowProperties(Icon icon);
		IEntry NewEntry();
		bool AddEntry(IEntry entry);
		bool RenameEntry(IEntry entry, string newName);
		bool RemoveEntry(IEntry entry);
		bool ShowEntryProperties(IEntry entry, Icon icon);

		// Settings
		bool SortEntries { get; set; }
	}

	public interface IEntry : IComparable<IEntry>
	{
		Encoding Encoding { get; set; }
		string Name { get; set; }
		byte[] OriginalText { get; set; }
		string OriginalTextString { get; }
		byte[] EditedText { get; set; }
		string EditedTextString { get; }

		int MaxLength { get; }
		bool IsResizable { get; }

		List<IEntry> SubEntries { get; }
		bool IsSubEntry { get; }

		string ToString();
	}

	public enum LoadResult
	{
		Success,
		Failure,
		TypeMismatch,
		FileNotFound
	}

	public enum SaveResult
	{
		Success,
		Failure
	}
}