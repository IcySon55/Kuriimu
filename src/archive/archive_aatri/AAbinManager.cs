using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;

namespace archive_aatri.aabin
{
    [FilePluginMetadata(Name = "AABin", Description = "Ace Attorney DS bin", Extension = "*.inc", Author = "onepiecefreak",
        About = "This is the Ace Attorney DS bin archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class AAbinManager : IArchiveManager
    {
        private AABIN _aabin = null;

        #region Properties

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => false;
        public bool CanSave => false;
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
                _aabin = new AABIN(File.OpenRead(filename));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _aabin.Save(File.Create(filename));
                _aabin.Close();
            }
            else
            {
                // Create the temp files
                _aabin.Save(File.Create(filename + ".tmp"));
                _aabin.Close();
                // Delete the originals
                FileInfo.Delete();
                File.Delete(filename);
                // Rename the temporary files
                File.Move(filename + ".tmp", filename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _aabin?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _aabin.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
