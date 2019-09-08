using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.G4PK
{
    class G4PKManager : IArchiveManager
    {
        private G4PK _g4pk = null;

        #region Properties

        // Information
        public string Name => "G4PK";
        public string Description => "Level 5 G4 package";
        public string Extension => "*.g4pkm;*.g4pk";
        public string About => "This is the G4PK archive manager for Karameru.";

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
                return br.ReadString(4) == "G4PK";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _g4pk = new G4PK(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _g4pk.Save(FileInfo.FullName);
                _g4pk.Close();
            }
            else
            {
                // Create the temp file
                _g4pk.Save(FileInfo.FullName + ".tmp");
                _g4pk.Close();
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
            _g4pk?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _g4pk.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
