using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_irarc
{
    [FilePluginMetadata(Name = "IRARC", Description = "IR ARChive", Extension = "*.irlst", Author = "onepiecefreak", About = "This is the IRARC archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class IrarcManager : IArchiveManager
    {
        private IRARC _irarc = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => false;
        public bool CanSave => false;
        public bool CanReplaceFiles => true;
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

            var irlstFilename = filename;
            var irarcFilename = filename.Remove(filename.Length - 5) + "irarc";

            _irarc = new IRARC(File.OpenRead(irlstFilename), File.OpenRead(irarcFilename));
        }

        public void Save(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            var irlstFilename = filename;
            var irarcFilename = filename.Remove(filename.Length - 5) + "irarc";

            // Save As...
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _irarc.Save(File.Create(irlstFilename), File.Create(irarcFilename));
                _irarc.Close();
            }
            else
            {
                // Create the temp files
                _irarc.Save(File.Create(irlstFilename + ".tmp"), File.Create(irarcFilename + ".tmp"));
                _irarc.Close();
                // Delete the originals
                FileInfo.Delete();
                File.Delete(irarcFilename);
                // Rename the temporary files
                File.Move(irlstFilename + ".tmp", irlstFilename);
                File.Move(irarcFilename + ".tmp", irarcFilename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _irarc?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _irarc.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
