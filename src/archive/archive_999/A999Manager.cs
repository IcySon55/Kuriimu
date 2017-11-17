using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_999
{
    [FilePluginMetadata(Name = "999 Archive", Description = "Archive for 999 on PC", Extension = "*.bin", Author = "onepiecefreak;Neobeo",
        About = "This is the 999 archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class A999Manager : IArchiveManager
    {
        private A999 _tng = null;

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

        public Identification Identify(Stream input, string filename)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                if (br.BaseStream.Length < 8) return Identification.False;
                if (br.ReadUInt32() == 3621168824 && br.ReadUInt32() == 4257008638) return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _tng = new A999(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            //_tng.Save(FileInfo.Create());
        }

        public void New()
        {

        }

        public void Unload()
        {
            // TODO: Implement closing open handles here
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _tng.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
