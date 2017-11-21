using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.DDSFS
{
    [FilePluginMetadata(Name = "3DSFS", Description = "3DS File System", Extension = "*.3ds", Author = "", About = "This is the 3DSFS archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class DdsfsManager : IArchiveManager
    {
        private DDSFS _ddsfs = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => true;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
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

        public void New()
        {

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
