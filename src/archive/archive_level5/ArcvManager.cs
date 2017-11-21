using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_level5.ARCV
{
    [FilePluginMetadata(Name = "ARCV", Description = "Level 5 ARChive V", Extension = "*.arc", Author = "onepiecefreak", About = "This is the ARCV archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class ArcvManager : IArchiveManager
    {
        private ARCV _arcv = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => true;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4) return false;
                return (br.ReadString(4) == "ARCV");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _arcv = new ARCV(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _arcv.Save(FileInfo.Create());
                _arcv.Close();
            }
            else
            {
                // Create the temp file
                _arcv.Save(File.Create(FileInfo.FullName + ".tmp"));
                _arcv.Close();
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
            _arcv?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _arcv.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
