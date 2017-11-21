using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_fnt
{
    [FilePluginMetadata(Name = "FNT", Description = "Whatever FNT should mean", Extension = "*.fnt", Author = "onepiecefreak", About = "This is the FNT archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class FntManager : IArchiveManager
    {
        private FNT _fnt = null;

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
                _fnt = new FNT(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _fnt.Save(FileInfo.Create());
                _fnt.Close();
            }
            else
            {
                // Create the temp file
                _fnt.Save(File.Create(FileInfo.FullName + ".tmp"));
                _fnt.Close();
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
            _fnt?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _fnt.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
