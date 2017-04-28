using System.Collections.Generic;
using System.Drawing;
using System.IO;
using archive_zar.Properties;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_zar
{
    public class ZarManager : IArchiveManager
    {
        private ZAR _zar = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Zelda ARchive";
        public string Extension => "*.zar";
        public string About => "This is the ZAR archive manager for Karameru.";

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
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 3) return false;
                return br.ReadString(3) == "ZAR";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _zar = new ZAR(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _zar.Save(FileInfo.Create());
                _zar.Close();
            }
            else
            {
                // Create the temp file
                _zar.Save(File.Create(FileInfo.FullName + ".tmp"));
                _zar.Close();
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
            _zar.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _zar.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
