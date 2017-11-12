using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Kontract.Interface
{
    public interface ITextAdapter : IFilePlugin
    {
        // Feature Support
        bool CanAddEntries { get; } // Is adding entries supported?
        bool CanRenameEntries { get; } // Is renaming entries supported?
        bool CanDeleteEntries { get; } // Is deleting entries supported?
        bool CanSortEntries { get; } // Should the option to sort entries even be allowed?
        bool EntriesHaveSubEntries { get; } // Do entries contain multiple text values?
        bool EntriesHaveUniqueNames { get; } // Must entry names be unique?
        bool EntriesHaveExtendedProperties { get; } // Entries provide an extended properties dialog?

        // I/O
        LoadResult Load(string filename, bool autoBackup = false);
        SaveResult Save(string filename = ""); // A non-blank filename is provided when using Save As...

        // Entries
        IEnumerable<TextEntry> Entries { get; }
        IEnumerable<string> NameList { get; }
        string LineEndings { get; }
        string NameFilter { get; } // This must be a regular expression that the incoming names must match. Use @".*" to accept any charcter.
        int NameMaxLength { get; }

        // Features
        TextEntry NewEntry();
        bool AddEntry(TextEntry entry);
        bool RenameEntry(TextEntry entry, string newName);
        bool DeleteEntry(TextEntry entry);
        bool ShowEntryProperties(TextEntry entry, Icon icon);

        // Settings
        bool SortEntries { get; set; }
    }

    // named this way instead of ITextEntry because we're going to turn it into a class
    public interface TextEntry
    {
        string Name { get; set; }
        string OriginalText { get; }
        string EditedText { get; set; }
        int MaxLength { get; }

        TextEntry ParentEntry { get; set; } // Reference to the parent entry
        bool IsSubEntry { get; } // Determines whether this entry is a sub entry
        bool HasText { get; } // Determines whether this entry can be edited in the Editor

        List<TextEntry> SubEntries { get; }
    }
}
