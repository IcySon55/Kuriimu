using System;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace archive_hunex
{
    public class HEDManager : IArchiveManager
    {
        private HED _hed = null;

        #region Properties

        // Information
        public string Name => "HuneX HED";

        public string Description => "HuneX Engine HED Archive Format";
        public string Extension => "*.hed";
        public string About => "This is the HuneX HED archive manager for Karameru.";

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
            // There is no garunteed way to tell if a file is a
            var mrgFileName = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".mrg");
            if (Path.GetExtension(filename) != ".hed") return false;
            if (!File.Exists(filename) || !File.Exists(mrgFileName)) return false;
            return true;
        }

        public void Load(string filename)
        {
            // Both MRG and HED files must exist for HED extraction
            // NAM file is used to store the file names in order and is optional
            // CPK file is used for file protection in an unknown way and is optional

            FileInfo = new FileInfo(filename);
            var baseTerm = Path.GetFileNameWithoutExtension(filename);
            var namFileName = Path.Combine(Path.GetDirectoryName(filename), baseTerm + ".nam");
            var mrgFileName = Path.Combine(Path.GetDirectoryName(filename), baseTerm + ".mrg");
            var cpkFileName = Path.Combine(Path.GetDirectoryName(filename), baseTerm + ".cpk");

            Stream namStream = null;
            Stream mrgStream = null;
            Stream cpkStream = null;

            if (File.Exists(namFileName))
                namStream = File.OpenRead(namFileName);

            if (File.Exists(mrgFileName))
                mrgStream = File.OpenRead(mrgFileName);

            if (File.Exists(cpkFileName))
                cpkStream = File.OpenRead(cpkFileName);

            if (FileInfo.Exists)
            {
                _hed = new HED(FileInfo.OpenRead(), namStream, mrgStream, cpkStream, baseTerm);
            }
        }

        public void Save(string filename = "")
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ArchiveFileInfo> Files => _hed.Files;

        public void Unload()
        {
            _hed?.Close();
        }

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        public bool ShowProperties(Icon icon) => false;
    }
}
