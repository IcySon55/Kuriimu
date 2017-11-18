using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;

namespace archive_level5.PCK
{
    [FilePluginMetadata(Name = "PCK", Description = "Level 5 PaCKage", Extension = "*.pck", Author = "onepiecefreak",
        About = "This is the PCK archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class PckManager : IArchiveManager
    {
        private PCK _pck = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            return Identification.Raw;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _pck = new PCK(FileInfo.OpenRead(), filename);
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _pck.Save(FileInfo.Create());
                _pck.Close();
            }
            else
            {
                // Create the temp file
                _pck.Save(File.Create(FileInfo.FullName + ".tmp"));
                _pck.Close();
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
            _pck?.Close();
        }

        //Files
        public IEnumerable<ArchiveFileInfo> Files => _pck.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
