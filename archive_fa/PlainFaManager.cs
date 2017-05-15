using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_fa
{
    public class PlainFaManager : IArchiveManager
    {
        private PlainFA _fa = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "Level 5 Plain File Archive";
        public string Extension => "*.fa";
        public string About => "This is the PlainFA archive manager for Karameru.";

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
                if (br.BaseStream.Length < 8) return false;
                return br.ReadBytes(4).SequenceEqual(new byte[] { 0xF7, 0x08, 0x0, 0x0 });
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _fa = new PlainFA(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _fa.Save(FileInfo.Create());
                _fa.Close();
            }
            else
            {
                // Create the temp file
                _fa.Save(File.Create(FileInfo.FullName + ".tmp"));
                _fa.Close();
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
            _fa?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _fa.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
