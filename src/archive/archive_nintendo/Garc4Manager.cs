using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.GARC4
{
    public class Garc4Manager : IArchiveManager
    {
        private GARC4 _garc4 = null;

        #region Properties

        // Information
        public string Name => "GARC4";
        public string Description => "General ARChive v.4";
        public string Extension => "*.garc";
        public string About => "This is the GARC4 archive manager for Karameru.";

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
                if (br.ReadString(4) != "CRAG") return false;
                if (br.BaseStream.Length < 0xc) return false;
                br.BaseStream.Position = 0xb;
                var version = br.ReadByte();
                return version == 4;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _garc4 = new GARC4(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _garc4.Save(FileInfo.Create());
                _garc4.Close();
            }
            else
            {
                // Create the temp file
                _garc4.Save(File.Create(FileInfo.FullName + ".tmp"));
                _garc4.Close();
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
            _garc4?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _garc4.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
