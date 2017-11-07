using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.SimpleSARC
{
    public class SimpleSarcManager : IArchiveManager
    {
        private SimpleSARC _sarc = null;

        #region Properties

        // Information
        public string Name => "SSARC";
        public string Description => "Simple ARChive";
        public string Extension => "*.sarc";
        public string About => "This is the Simple SARC archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
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
                if (br.BaseStream.Length < 4 || br.ReadString(4) != "SARC") return false;
                br.ReadInt16();
                var ind = br.ReadUInt16();
                return ind != 0xfeff && ind != 0xfffe;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _sarc = new SimpleSARC(FileInfo.OpenRead());
        }

        public void Save(string filename)
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

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
