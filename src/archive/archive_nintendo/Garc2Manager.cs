using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.GARC2
{
    [FilePluginMetadata(Name = "GARC2", Description = "General ARChive v.2", Extension = "*.garc", Author = "onepiecefreak", About = "This is the GARC2 archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class Garc2Manager : IArchiveManager
    {
        private GARC2 _garc2 = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4) return Identification.False;
                if (br.ReadString(4) != "CRAG") return Identification.False;
                if (br.BaseStream.Length < 0xc) return Identification.False;
                br.BaseStream.Position = 0xb;
                var version = br.ReadByte();
                if (version == 2) return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _garc2 = new GARC2(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _garc2.Save(FileInfo.Create());
                _garc2.Close();
            }
            else
            {
                // Create the temp file
                _garc2.Save(File.Create(FileInfo.FullName + ".tmp"));
                _garc2.Close();
                // Delete the original
                FileInfo.Delete();
                // Rename the temporary file
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _garc2?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _garc2.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
