using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_nintendo.DDSFS
{
    public class DdsfsManager : IArchiveManager
    {
        private DDSFS _ddsfs = null;

        #region Properties

        // Information
        public string Name => "3DSFS";
        public string Description => "3DS File System";
        public string Extension => "*.3ds";
        public string About => "This is the 3DSFS archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 0x104) return false;
                br.BaseStream.Position = 0x100;
                return (br.ReadString(4) == "NCSD");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _ddsfs = new DDSFS(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _ddsfs.Save(FileInfo.Create());
                _ddsfs.Close();
            }
            else
            {
                // Create the temp file
                _ddsfs.Save(File.Create(FileInfo.FullName + ".tmp"));
                _ddsfs.Close();
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
            _ddsfs?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _ddsfs.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
