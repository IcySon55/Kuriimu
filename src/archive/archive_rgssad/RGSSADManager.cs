using Kontract.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Kontract.IO;

namespace archive_rgssad
{
    public class RGSSADManager : IArchiveManager
    {
        private RGSSAD _rgssad = null;

        #region Properties

        // Information
        public string Name => "RPG Maker RGASSAD";

        public string Description => "RPG Maker RGSSAD Encrypted Archive Format";
        public string Extension => "*.rgssad;*.rgss2a;*.rgss3a";
        public string About => "This is the RGSSAD archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;

        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            if (!File.Exists(filename)) return false;
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
                return br.ReadCStringA() == "RGSSAD";
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _rgssad = new RGSSAD(FileInfo.OpenRead());
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
