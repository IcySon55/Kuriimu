using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_rdp
{
    public class archive_pakManager : IArchiveManager
    {
        private RESRDP _rdp = null;

        #region Properties

        // Information
        public string Name => "RDP";
        public string Description => "RDP";
        public string Extension => "*.res";
        public string About => "This is the RDP archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var rdpPath = Path.Combine(Path.GetDirectoryName(filename), "package", ".rdp");
            if (!File.Exists(rdpPath))
                return false;
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            using (var br2 = new BinaryReaderX(File.OpenRead(rdpPath)))
                return br.ReadString(4) == "Pres" && br2.ReadString(4) == "rdp ";
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            var rdpPath = Path.Combine(Path.GetDirectoryName(filename), "package", ".rdp");

            if (FileInfo.Exists && File.Exists(rdpPath))
                _rdp = new RESRDP(FileInfo.OpenRead(), File.OpenRead(rdpPath));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _rdp.Save(FileInfo.Create());
                _rdp.Close();
            }
            else
            {
                // Create the temp file
                _rdp.Save(File.Create(FileInfo.FullName + ".tmp"));
                _rdp.Close();
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
            _rdp?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _rdp.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
