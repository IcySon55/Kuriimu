using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Archive;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_sarc
{
    public class SarcManager : IArchiveManager
    {
        private SARC _sarc = null;

        #region Properties

        // Information
        public string Name => "SARC";
        public string Description => "NW4C Sorted ARChive";
        public string Extension => "*.sarc;*.arc";
        public string About => "This is the SARC archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => true;
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
                if (br.BaseStream.Length < 4 || br.ReadString(4) != "SARC") return false;
                br.ReadInt16();
                var ind = br.ReadUInt16();
                return ind == 0xfeff || ind == 0xfffe;
            }
        }

        public LoadResult Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            if (!FileInfo.Exists) return LoadResult.FileNotFound;

            _sarc = new SARC(FileInfo.OpenRead());
            return LoadResult.Success;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            try
            {
                _sarc.Save(FileInfo.Create());
            }
            catch
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        public void Unload()
        {
            // TODO: Implement closing open handles here
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _sarc.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            _sarc.Files.Add(afi);
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => throw new NotSupportedException();

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
