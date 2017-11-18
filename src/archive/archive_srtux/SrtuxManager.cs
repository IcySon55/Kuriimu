using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_srtux
{
    [FilePluginMetadata(Name = "SRTUX", Description = "SRTUX Archive", Extension = "*.bin", Author = "", About = "This is the SRTUX archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class SrtuxManager : IArchiveManager
    {
        private SRTUX _srtux = null;

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
                br.BaseStream.Position = br.BaseStream.Length - 4;
                if (br.ReadString(3) == "end") return Identification.True;
            }

            return Identification.Raw;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _srtux = new SRTUX(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _srtux.Save(FileInfo.Create());
                _srtux.Close();
            }
            else
            {
                // Create the temp file
                _srtux.Save(File.Create(FileInfo.FullName + ".tmp"));
                _srtux.Close();
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
            _srtux?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _srtux.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
