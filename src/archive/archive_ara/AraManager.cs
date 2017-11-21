using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_ara
{
    [FilePluginMetadata(Name = "ARA", Description = "Angelique Retour Archive", Extension = "*.bin", Author = "IcySon55", About = "This is the ARA archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class AraManager : IArchiveManager
    {
        private ARA _ara = null;

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
            var arcFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".arc");
            if (!File.Exists(filename) || !File.Exists(arcFilename)) return false;
            using (var br = new BinaryReaderX(stream, true))
                return (br.ReadString(3) == "PAA") ;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            var arcFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".arc");

            if (FileInfo.Exists)
                _ara = new ARA(FileInfo.OpenRead(), File.OpenRead(arcFilename));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);
            var arcFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".arc");

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _ara.Save(FileInfo.Create(), File.Create(arcFilename));
                _ara.Close();
            }
            else
            {
                // Create the temp file
                _ara.Save(File.Create(FileInfo.FullName + ".tmp"), File.Create(arcFilename + ".tmp"));
                _ara.Close();
                // Delete the original
                FileInfo.Delete();
                File.Delete(arcFilename);
                // Rename the temporary file
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
                File.Move(arcFilename + ".tmp", arcFilename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _ara?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _ara.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
