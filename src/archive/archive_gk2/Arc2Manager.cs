using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_gk2.arc2
{
    [FilePluginMetadata(Name = "GK2 Arc2", Description = "Gyakuten Kenji 2 Archive 2", Extension = "*.bin", Author = "onepiecefreak", About = "This is the Arc2 archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class Arc2Manager : IArchiveManager
    {
        private ARC2 _arc2 = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            return false;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _arc2 = new ARC2(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _arc2.Save(FileInfo.Create());
                _arc2.Close();
            }
            else
            {
                // Create the temp file
                _arc2.Save(File.Create(FileInfo.FullName + ".tmp"));
                _arc2.Close();
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
            _arc2?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _arc2.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
