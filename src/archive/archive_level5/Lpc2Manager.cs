using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_level5.LPC2
{
    public class Lpc2Manager : IArchiveManager
    {
        private LPC2 _lpc2 = null;

        #region Properties

        // Information
        public string Name => "LPC2";
        public string Description => "Level 5 LPC2 Archive";
        public string Extension => "*.cani";
        public string About => "This is the LPC2 archive manager for Karameru.";

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
                br.BaseStream.Position = 5;
                return br.ReadString(4) == "LPC2";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _lpc2 = new LPC2(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _lpc2.Save(FileInfo.FullName);
                _lpc2.Close();
            }
            else
            {
                // Create the temp file
                _lpc2.Save(FileInfo.FullName + ".tmp");
                _lpc2.Close();
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
            _lpc2?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _lpc2.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
