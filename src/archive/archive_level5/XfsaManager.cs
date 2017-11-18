using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_level5.XFSA
{
    [FilePluginMetadata(Name = "XFSA", Description = "Level 5 XFS Archive", Extension = "*.fa", Author = "onepiecefreak",
        About = "This is the XFSA archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class XfsakManager : IArchiveManager
    {
        private XFSA _xfsa = null;

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
                if (br.ReadString(4) == "XFSA") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _xfsa = new XFSA(File.OpenRead(FileInfo.FullName));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _xfsa.Save(FileInfo.Create());
                _xfsa.Close();
            }
            else
            {
                // Create the temp file
                _xfsa.Save(File.Create(FileInfo.FullName + ".tmp"));
                _xfsa.Close();
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
            _xfsa?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _xfsa.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
