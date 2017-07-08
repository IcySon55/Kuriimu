using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace archive_mt
{
    public class MTArcManager : IArchiveManager
    {
        private MTARC _format;

        #region Properties

        // Information
        public string Name => "MTARC";
        public string Description => "MT Framework Archive";
        public string Extension => "*.arc";
        public string About => "This is the MT Framework archive manager for Karameru.";

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
                if (br.BaseStream.Length < 4) return false;
                var magic = br.ReadString(4);
                return magic == "ARC" || magic == "\0CRA";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _format = new MTARC(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _format.Save(FileInfo.Create());
                _format.Close();
            }
            else
            {
                // Create the temp file
                _format.Save(File.Create(FileInfo.FullName + ".tmp"));
                _format.Close();
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
            _format?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _format.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
