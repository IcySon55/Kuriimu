using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Cetera.Hash;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.SARC
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
        public bool FileHasExtendedProperties => false;
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

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _sarc = new SARC(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _sarc.Save(FileInfo.Create());
                _sarc.Close();
            }
            else
            {
                // Create the temp file
                _sarc.Save(File.Create(FileInfo.FullName + ".tmp"));
                _sarc.Close();
                // Delete the original
                FileInfo.Delete();
                // Rename the temporary file
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _sarc?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _sarc.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            _sarc.Files.Add(new SarcArchiveFileInfo
            {
                FileData = afi.FileData,
                FileName = afi.FileName,
                State = afi.State,
                Hash = SimpleHash.Create(afi.FileName, _sarc.hashMultiplier)
            });
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
