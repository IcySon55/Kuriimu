using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_nintendo.NDSFS
{
    [FilePluginMetadata(Name = "NDSFS", Description = "Nintendo DS File System", Extension = "*.nds", Author = "onepiecefreak", About = "This is the NDS FS archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class NdsfsManager : IArchiveManager
    {
        private NDS _ndsfs = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 0x14) return Identification.False;
                br.BaseStream.Position = 0x14;
                if (Math.Pow(2, 17 + br.ReadByte()) == br.BaseStream.Length) return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _ndsfs = new NDS(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _ndsfs.Save(FileInfo.Create());
                _ndsfs.Close();
            }
            else
            {
                // Create the temp file
                _ndsfs.Save(File.Create(FileInfo.FullName + ".tmp"));
                _ndsfs.Close();
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
            _ndsfs?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _ndsfs.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
