using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using Cetera.Hash;

namespace archive_bg4
{
    public class Bg4Manager : IArchiveManager
    {
        private BG4 _bg4 = null;

        #region Properties

        // Information
        public string Name => "BG4";
        public string Description => "Whatever BG4 means";
        public string Extension => "*.dat";
        public string About => "This is the BG4 archive manager for Karameru.";

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
                return br.ReadString(3) == "BG4";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _bg4 = new BG4(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _bg4.Save(FileInfo.FullName);
                _bg4.Close();
            }
            else
            {
                // Create the temp file
                _bg4.Save(FileInfo.FullName + ".tmp");
                _bg4.Close();
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
            _bg4?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _bg4.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
