using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_bfp
{
    public class BfpManager : IArchiveManager
    {
        private BFP _bfp = null;

        #region Properties

        // Information
        public string Name => "BFP";
        public string Description => "Binary File Package";
        public string Extension => "*.bfp";
        public string About => "This is the BFP archive manager for Karameru.";

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
            using (var br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                return br.ReadString(4) == "RTFP";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _bfp = new BFP(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _bfp.Save(FileInfo.Create());
                _bfp.Close();
            }
            else
            {
                // Create the temp file
                _bfp.Save(File.Create(FileInfo.FullName + ".tmp"));
                _bfp.Close();
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
            _bfp?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _bfp.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
