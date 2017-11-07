using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Cetera.Hash;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.XFSA
{
    public class XfsakManager : IArchiveManager
    {
        private XFSA _xfsa = null;

        #region Properties

        // Information
        public string Name => "XFSA";
        public string Description => "Level 5 XFS Archive";
        public string Extension => "*.fa";
        public string About => "This is the XFSA archive manager for Karameru.";

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
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "XFSA";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _xfsa = new XFSA(File.OpenRead(FileInfo.FullName));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _xfsa.Save(FileInfo.Create());
                _xfsa.Close();
            }
            else
            {
                // Create the temp file
                _xfsa.Save(File.Create(FileInfo.FullName + ".tmp"));
                _xfsa.Close();
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
            _xfsa?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _xfsa.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
