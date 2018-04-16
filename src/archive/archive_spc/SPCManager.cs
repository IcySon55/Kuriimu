using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_spc
{
    public class archive_pakManager : IArchiveManager
    {
        private SPC _spc = null;

        #region Properties

        // Information
        public string Name => "SPC";
        public string Description => "SPC";
        public string Extension => "*.spc";
        public string About => "This is the SPC archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

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
                _spc = new SPC(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _spc.Save(FileInfo.Create());
                _spc.Close();
            }
            else
            {
                // Create the temp file
                _spc.Save(File.Create(FileInfo.FullName + ".tmp"));
                _spc.Close();
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
            _spc?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _spc.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
