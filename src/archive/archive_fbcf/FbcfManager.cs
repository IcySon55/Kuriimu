using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

//h.a.n.d Inc

namespace archive_fbcf
{
    public class FbcfManager : IArchiveManager
    {
        private FBCF _fbcf = null;

        #region Properties

        // Information
        public string Name => "FBCF";
        public string Description => "FBCF Archive";
        public string Extension => "*.fbc";
        public string About => "This is the FBCF archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                br.BaseStream.Position = 0x30;
                return br.ReadString(4) == "FBCF";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _fbcf = new FBCF(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _fbcf.Save(FileInfo.Create());
                _fbcf.Close();
            }
            else
            {
                // Create the temp file
                _fbcf.Save(File.Create(FileInfo.FullName + ".tmp"));
                _fbcf.Close();
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
            _fbcf?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _fbcf.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
