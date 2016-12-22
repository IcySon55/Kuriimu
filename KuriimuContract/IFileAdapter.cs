using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		bool CanRemoveEntries { get; } // Is removing entries supported?
		bool EntriesHaveUniqueNames { get; } // Must entry names be unique?
		bool EntriesHaveExtendedProperties { get; } // Entries provides an extended properties dialog?

		// I/O
		FileInfo FileInfo { get; set; }
		bool Identify(string filename); // Determines if the given file is opened by the plugin.
		LoadResult Load(string filename);
		SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...

		// Entries
		bool HasEntries { get; }
		List<IEntry> Entries { get; }
		List<string> NameList { get; }

		// Features
		bool ShowProperties(Icon icon);
		IEntry NewEntry();
		bool AddEntry(IEntry entry);
		bool RemoveEntry(IEntry entry);
		bool ShowEntryProperties(IEntry entry, Icon icon);
	}

	public interface IEntry : IComparable<IEntry>
	{
		Encoding Encoding { get; set; }
		string Name { get; set; }
		byte[] OriginalText { get; set; }
		string OriginalTextString { get; }
		byte[] EditedText { get; set; }
		string EditedTextString { get; }

		bool IsResizable { get; }
		int MaxLength { get; }

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