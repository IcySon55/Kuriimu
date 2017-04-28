using System.Collections.Generic;
using System.Drawing;
using System.IO;
using archive_afs.Properties;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_afs
{
    public class AfsManager : IArchiveManager
    {
        private AFS _afs = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "Archive File System";
        public string Extension => "*.afs;*.dat";
        public string About => "This is the AFS archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 3) return false;
                return br.ReadString(3) == "AFS";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _afs = new AFS(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            //_afs.Save(FileInfo.Create());
        }

        public void Unload()
        {
            // TODO: Implement closing open handles here
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _afs.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
