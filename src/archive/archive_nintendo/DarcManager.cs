using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.DARC
{
    [FilePluginMetadata(Name = "DARC", Description = "Default ARChive", Extension = "*.arc", Author = "", About = "This is the DARC archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class DarcManager : IArchiveManager
    {
        private DARC _darc = null;

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
                return (br.ReadString(4) == "darc");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _darc = new DARC(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _darc.Save(FileInfo.Create());
                _darc.Close();
            }
            else
            {
                // Create the temp file
                _darc.Save(File.Create(FileInfo.FullName + ".tmp"));
                _darc.Close();
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
            _darc?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _darc.Files.Where(afi => !afi.Entry.IsFolder);

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
