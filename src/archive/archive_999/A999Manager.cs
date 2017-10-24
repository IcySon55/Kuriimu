using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_999
{
    public class A999Manager : IArchiveManager
    {
        private A999 _tng = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "Archive for 999 on PC";
        public string Extension => "*.bin";
        public string About => "This is the 999 archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 8) return false;
                return br.ReadUInt32() == 3621168824 && br.ReadUInt32() == 4257008638;
            }
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
