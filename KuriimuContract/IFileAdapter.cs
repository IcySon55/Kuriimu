using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		bool CanSave { get; }
		bool CanAddEntries { get; }
		bool CanRemoveEntries { get; }

		// I/O
		FileInfo TargetFile { get; set; }
		LoadResult Load(string filename);
		SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...

		// Entries
		bool HasEntries { get; }
		List<IEntry> Entries { get; }

		// Features
		bool AddEntry(IEntry entry);
		bool RemoveEntry(IEntry entry);
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