using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_vap
{
    public class VapManager : IArchiveManager
    {
        private VAP _vap = null;

        #region Properties

        // Information
        public string Name => "VAP";
        public string Description => "V Archive Package";
        public string Extension => "*.vap";
        public string About => "This is the VAP archive manager for Karameru.";

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
                var fileCount = br.ReadInt32();
                br.BaseStream.Position = 0xc + (fileCount - 1) * 0x8;

                var offset = br.ReadInt32();
                var size = br.ReadInt32();

                return br.BaseStream.Length == offset + size;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _vap = new VAP(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _vap.Save(FileInfo.Create());
                _vap.Close();
            }
            else
            {
                // Create the temp file
                _vap.Save(File.Create(FileInfo.FullName + ".tmp"));
                _vap.Close();
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
            _vap?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _vap.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
