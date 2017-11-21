using System;
using Kontract.Interface;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Drawing;

namespace archive_hunex
{
    [FilePluginMetadata(Name = "HuneX HED", Description = "HuneX Engine HED Archive Format", Extension = "*.hed", Author = "Sn0wCrack",
        About = "This is the HuneX HED archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class HEDManager : IArchiveManager
    {
        private HED _hed = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;

        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            return false;
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

        public void New()
        {

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
