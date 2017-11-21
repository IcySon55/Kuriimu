using Kontract.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Komponent.IO;

namespace archive_rgssad
{
    [FilePluginMetadata(Name = "RPG Maker RGASSAD", Description = "RPG Maker RGSSAD Encrypted Archive Format", Extension = "*.rgssad;*.rgss2a;*.rgss3a",
        Author = "Sn0wCrack", About = "This is the RGSSAD archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class RGSSADManager : IArchiveManager
    {
        private RGSSAD _rgssad = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;

        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => true;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            if (!File.Exists(filename)) return false;
            using (var br = new BinaryReaderX(stream, true))
                return (br.ReadCStringA() == "RGSSAD");
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _rgssad = new RGSSAD(FileInfo.OpenRead());
        }

        public void New()
        {

        }

        public void Save(string filename = "")
        {
            return;
        }

        public IEnumerable<ArchiveFileInfo> Files => _rgssad.Files;

        public void Unload()
        {
            _rgssad?.Close();
        }

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        public bool ShowProperties(Icon icon) => false;
    }
}
