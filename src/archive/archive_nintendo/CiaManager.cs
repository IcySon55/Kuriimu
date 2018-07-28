using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace archive_nintendo.CIA
{
    public class CiaManager : IArchiveManager
    {
        private CIA _cia = null;

        #region Properties

        // Information
        public string Name => "CIA";
        public string Description => "CTR Importable Archive";
        public string Extension => "*.cia";
        public string About => "This is the CIA archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => true;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => true;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadInt32() == 0x2020;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _cia = new CIA(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _cia.Save(FileInfo.Create());
                _cia.Close();
            }
            else
            {
                // Create the temp file
                _cia.Save(File.Create(FileInfo.FullName + ".tmp"));
                _cia.Close();
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
            _cia?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _cia.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            _cia.Files.Add(afi);
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi)
        {
            var toRemove = _cia.Files.Where(f => f.FileName.Replace('\\', '/') == afi.FileName.Replace('\\', '/'));
            foreach (var r in toRemove)
                r.State = ArchiveFileState.Deleted;
            return true;
        }

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
