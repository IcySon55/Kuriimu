using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_sony.PSARC
{
    class PsarcManager : IArchiveManager
    {
        private PSARC _psar;

        #region Properties

        // Information
        public string Name => "PSARC";
        public string Description => "PlayStation Archive";
        public string Extension => "*.psarc";
        public string About => "This is the PSARC archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanReplaceFiles => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            if (!File.Exists(filename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.BaseStream.Length >= 4 && br.ReadString(4) == "PSAR";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            _psar = new PSARC(File.OpenRead(filename));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _psar.Save(FileInfo.Create());
                _psar.Close();
            }
            else
            {
                // Create the temp file
                _psar.Save(File.Create(FileInfo.FullName + ".tmp"));
                _psar.Close();
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
            _psar?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _psar.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
