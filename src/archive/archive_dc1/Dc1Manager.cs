using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_dc1
{
    public class Dc1Manager : IArchiveManager
    {
        private DC1 _dc1 = null;

        #region Properties

        // Information
        public string Name => "DC1";
        public string Description => "Da Capo 1 Archive";
        public string Extension => "*.bin";
        public string About => "This is the DC1 archive manager for Karameru.";

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
            using (var br=new BinaryReaderX(File.OpenRead(filename),true))
            {
                if (br.BaseStream.Length<3) return false;
                return br.ReadString(3) == "DC1";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _dc1 = new DC1(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _dc1.Save(FileInfo.Create());
                _dc1.Close();
            }
            else
            {
                // Create the temp file
                _dc1.Save(File.Create(FileInfo.FullName + ".tmp"));
                _dc1.Close();
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
            _dc1?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _dc1.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
