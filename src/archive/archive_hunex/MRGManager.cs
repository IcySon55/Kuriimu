using System;
using System.ComponentModel.Composition;
using Kontract.Interface;
using Komponent.IO;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace archive_hunex
{
    [FilePluginMetadata(Name = "HuneX MRG", Description = "HuneX Engine MRG Archive Format", Extension = "*.mrg", Author = "Sn0wCrack",
        About = "This is the HuneX MRG archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    internal class MRGManager : IArchiveManager
    {
        private MRG _mrg = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;

        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            if (!File.Exists(filename)) return Identification.False;
            using (var br = new BinaryReaderX(stream, true))
                if (br.ReadString(6) == "mrgd00") return Identification.True;

            return Identification.False;
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

        public void New()
        {

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
