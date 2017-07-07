using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_pak
{
    public class archive_pakManager : IArchiveManager
    {
        private PAK _pak = null;

        #region Properties

        // Information
        public string Name => "PAK";
        public string Description => "PAcKage";
        public string Extension => "*.pak";
        public string About => "This is the PAK archive manager for Karameru.";

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
            try
            {
                using (var br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    var fileCount = br.ReadUInt16();
                    br.BaseStream.Position = br.ReadUInt16() + (fileCount - 1) * 8;
                    return (br.ReadInt32() + br.ReadInt32() == br.BaseStream.Length);
                }
            }
            catch
            {
                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _pak = new PAK(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _pak.Save(FileInfo.Create());
                _pak.Close();
            }
            else
            {
                // Create the temp file
                _pak.Save(File.Create(FileInfo.FullName + ".tmp"));
                _pak.Close();
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
            _pak?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _pak.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
