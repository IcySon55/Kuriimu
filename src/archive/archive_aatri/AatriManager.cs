using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_aatri.aatri
{
    public class AatriManager : IArchiveManager
    {
        private AATRI _aatri = null;

        #region Properties

        // Information
        public string Name => "AATRI";
        public string Description => "Ace Attorney Trilogy pack";
        public string Extension => "*.inc";
        public string About => "This is the archive_aatri archive manager for Karameru.";

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
            var incFilename = filename;
            var datFilename = filename.Remove(filename.Length - 3) + "dat";

            if (!File.Exists(incFilename) || !File.Exists(datFilename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(incFilename)))
            using (var brd = new BinaryReaderX(File.OpenRead(datFilename)))
            {
                if (br.BaseStream.Length < 0x18) return false;

                var offset = br.ReadInt32();
                brd.BaseStream.Position = offset;
                if (brd.ReadByte() != 0x11) return false;

                br.BaseStream.Position = 0x14;
                offset = br.ReadInt32();
                brd.BaseStream.Position = offset;
                return brd.ReadByte() == 0x11;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            var incFilename = filename;
            var datFilename = filename.Remove(filename.Length - 3) + "dat";

            if (FileInfo.Exists)
                _aatri = new AATRI(File.OpenRead(incFilename), File.OpenRead(datFilename));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            var incFilename = FileInfo.FullName;
            var datFilename = FileInfo.FullName.Remove(FileInfo.FullName.Length - 3) + "dat";

            // Save As...
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _aatri.Save(File.Create(incFilename), File.Create(datFilename));
                _aatri.Dispose();
            }
            else
            {
                // Create the temp files
                _aatri.Save(File.Create(incFilename + ".tmp"), File.Create(datFilename + ".tmp"));
                _aatri.Dispose();
                // Delete the originals
                FileInfo.Delete();
                File.Delete(datFilename);
                // Rename the temporary files
                File.Move(incFilename + ".tmp", incFilename);
                File.Move(datFilename + ".tmp", datFilename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _aatri?.Dispose();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _aatri.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
