﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.ARC0
{
    public class Arc0Manager : IArchiveManager
    {
        private ARC0 _arc0 = null;

        #region Properties

        // Information
        public string Name => "ARC0";
        public string Description => "Level 5 ARChive 0";
        public string Extension => "*.fa";
        public string About => "This is the ARC0 archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                var magic = br.ReadString(4);
                return magic == "ARC0";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _arc0 = new ARC0(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _arc0.Save(FileInfo.Create());
                _arc0.Close();
            }
            else
            {
                // Create the temp file
                _arc0.Save(File.Create(FileInfo.FullName + ".tmp"));
                _arc0.Close();
                // Delete the original
                FileInfo.Delete();
                // Rename the temporary file
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _arc0?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _arc0.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
