﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_dqxi
{
    public class PackManager : IArchiveManager
    {
        private PACK _pack = null;

        #region Properties

        // Information
        public string Name => "PACK";
        public string Description => "Dragon Quest XI PACK";
        public string Extension => "*.pack";
        public string About => "This is the PACK archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
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
                if (br.ReadString(4) != "PACK") return false;
                var size = br.ReadInt32();

                return size == br.BaseStream.Length;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _pack = new PACK(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _pack.Save(FileInfo.Create());
                _pack.Close();
            }
            else
            {
                // Create the temp file
                _pack.Save(File.Create(FileInfo.FullName + ".tmp"));
                _pack.Close();
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
            _pack?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _pack.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
