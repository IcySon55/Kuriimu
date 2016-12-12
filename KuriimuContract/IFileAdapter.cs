﻿using System;
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
		bool CanSave { get; } // Is saving supported?.
		bool CanAddEntries { get; } // Is adding entries supported?
		bool CanRemoveEntries { get; } // Is removing entries supported?
		bool EntriesHaveUniqueNames { get; } // Must entry names be unique
		bool EntriesHaveExtendedProperties { get; } // Do entries have extra data

		// I/O
		FileInfo TargetFile { get; set; }
		LoadResult Load(string filename);
		SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...

		// Entries
		bool HasEntries { get; }
		List<IEntry> Entries { get; }
		List<string> NameList { get; }

		// Features
		bool AddEntry(IEntry entry);
		bool RemoveEntry(IEntry entry);
		void EntryProperties(IEntry entry, Icon icon);
	}

	public interface IEntry : IComparable<IEntry>
	{
		Encoding Encoding { get; set; }
		string Name { get; set; }
		byte[] OriginalText { get; set; }
		byte[] EditedText { get; set; }

		string ToString();
		string GetOriginalString();
		string GetEditedString();
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