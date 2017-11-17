using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_aatri.aatri
{
    [FilePluginMetadata(Name = "AATri", Description = "Ace Attorney Trilogy pack", Extension = "*.inc", Author = "onepiecefreak",
        About = "This is the AATri archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class AatriManager : IArchiveManager
    {
        private AATRI _aatri = null;

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
            var incFilename = filename;
            var datFilename = filename.Remove(filename.Length - 3) + "dat";

            if (!File.Exists(incFilename) || !File.Exists(datFilename)) return Identification.False;

            using (var br = new BinaryReaderX(stream, true))
            using (var brd = new BinaryReaderX(File.OpenRead(datFilename)))
            {
                if (br.BaseStream.Length < 0x18) return Identification.False;

                var offset = br.ReadInt32();
                brd.BaseStream.Position = offset;
                if (brd.ReadByte() != 0x11) return Identification.False;

                br.BaseStream.Position = 0x14;
                offset = br.ReadInt32();
                brd.BaseStream.Position = offset;
                if (brd.ReadByte() == 0x11) return Identification.True;
            }

            return Identification.False;
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
                _aatri.Close();
            }
            else
            {
                // Create the temp files
                _aatri.Save(File.Create(incFilename + ".tmp"), File.Create(datFilename + ".tmp"));
                _aatri.Close();
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

        public void New()
        {

        }

        public void Unload()
        {
            _aatri?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _aatri.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
