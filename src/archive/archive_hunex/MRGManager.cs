using System;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace archive_hunex
{
    internal class MRGManager : IArchiveManager
    {
        private MRG _mrg = null;

        #region Properties

        // Information
        public string Name => "HuneX MRG";

        public string Description => "HuneX Engine MRG Archive Format";
        public string Extension => "*.mrg";
        public string About => "This is the HuneX MRG archive manager for Karameru.";

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
            if (!File.Exists(filename)) return false;
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
                return br.ReadString(6) == "mrgd00";
        }

        public void Load(string filename)
        {
            var baseTerm = Path.GetFileNameWithoutExtension(filename);
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _mrg = new MRG(FileInfo.OpenRead(), baseTerm);
            }
        }

        public void Save(string filename = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ArchiveFileInfo> Files => _mrg.Files;

        public void Unload()
        {
            _mrg?.Close();
        }

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        public bool ShowProperties(Icon icon) => false;
    }
}
