using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

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
		bool CanDeleteEntries { get; } // Is deleting entries supported?
		bool CanSortEntries { get; } // Should the option to sort entries even be allowed?
		bool EntriesHaveSubEntries { get; } // Do entries contain multiple text values?
		bool EntriesHaveUniqueNames { get; } // Must entry names be unique?
		bool EntriesHaveExtendedProperties { get; } // Entries provides an extended properties dialog?

		// I/O
		FileInfo FileInfo { get; set; }
		string LineEndings { get; }
		bool Identify(string filename); // Determines if the given file is opened by the plugin.
		LoadResult Load(string filename);
		SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...

		// Entries
		IEnumerable<IEntry> Entries { get; }
		IEnumerable<string> NameList { get; }
		string NameFilter { get; } // This must be a regular expression that the incoming names must match. Use @".*" to accept any charcter.
		int NameMaxLength { get; }

		// Features
		bool ShowProperties(Icon icon);
		IEntry NewEntry();
		bool AddEntry(IEntry entry);
		bool RenameEntry(IEntry entry, string newName);
		bool DeleteEntry(IEntry entry);
		bool ShowEntryProperties(IEntry entry, Icon icon);

		// Settings
		bool SortEntries { get; set; }
	}

	public interface IEntry : IComparable<IEntry>
	{
		string Name { get; set; }
		string OriginalText { get; }
		string EditedText { get; set; }
		int MaxLength { get; }

		IEntry ParentEntry { get; set; } // Reference to the parent entry
		bool IsSubEntry { get; } // Determines whether this entry is a sub entry
		bool HasText { get; } // Determines whether this entry can be edited in the Editor

		List<IEntry> SubEntries { get; }

		string ToString(); // This will appear in the entry list in the Editor
	}
}