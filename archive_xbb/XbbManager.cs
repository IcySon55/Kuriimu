using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Hash;
using System.Text;

namespace archive_xbb
{
    public class archive_xbbManager : IArchiveManager
    {
        private XBB _xbb = null;

        #region Properties

        // Information
        public string Name => "XBB";
        public string Description => "Whatever XBB should mean";
        public string Extension => "*.xbb";
        public string About => "This is the XBB archive manager for Karameru.";

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
            using (var br=new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return (br.ReadString(3) == "XBB");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _xbb = new XBB(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _xbb.Save(FileInfo.Create());
                _xbb.Close();
            }
            else
            {
                // Create the temp file
                _xbb.Save(File.Create(FileInfo.FullName + ".tmp"));
                _xbb.Close();
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
            _xbb?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _xbb.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
