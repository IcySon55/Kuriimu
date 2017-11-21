using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_dqxi
{
    [FilePluginMetadata(Name = "PACK", Description = "Dragon Quest XI PACK", Extension = "*.pack", Author = "onepiecefreak", About = "This is the PACK archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class PackManager : IArchiveManager
    {
        private PACK _pack = null;

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
                if (br.ReadString(4) != "PACK") return false;
                var size = br.ReadInt32();

                return (size == br.BaseStream.Length);
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _pack = new PACK(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _pack.Save(FileInfo.Create());
                _pack.Close();
            }
            else
            {
                // Create the temp file
                _pack.Save(File.Create(FileInfo.FullName + ".tmp"));
                _pack.Close();
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
            _pack?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _pack.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
