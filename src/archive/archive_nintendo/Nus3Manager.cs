using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.NUS3
{
    [FilePluginMetadata(Name = "NUS3", Description = "NUS3BANK Sound Archive", Extension = "*.nus3bank", Author = "", About = "This is the NUS3BANK archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public sealed class Nus3Manager : IArchiveManager
    {
        private NUS3 _nus3 = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream))
            {
                if (br.BaseStream.Length < 4) return Identification.False;
                if (br.ReadString(4) == "NUS3") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _nus3 = new NUS3(File.OpenRead(FileInfo.FullName));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _nus3.Save(FileInfo.FullName);
                _nus3.Close();
            }
            else
            {
                // Create the temp file
                _nus3.Save(FileInfo.FullName + ".tmp");
                _nus3.Close();
                // Delete the original
                FileInfo.Delete();
                // Rename the temporary file
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _nus3?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _nus3.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
