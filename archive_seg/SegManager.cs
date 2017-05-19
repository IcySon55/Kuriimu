using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_seg
{
    public class SegManager : IArchiveManager
    {
        private SEG _format = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "Seg Archive";
        public string Extension => "*.seg";
        public string About => "This is the SEG archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var binFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".bin");

            if (!File.Exists(filename) || !File.Exists(binFilename)) return false;
            
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 8) return false;
                br.BaseStream.Seek(-4, SeekOrigin.End);
                return br.ReadUInt32() == new FileInfo(binFilename).Length;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            var binFilename = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".bin");

            if (FileInfo.Exists)
                _format = new SEG(FileInfo.OpenRead(), File.OpenRead(binFilename));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);
            var binFilename = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".bin");

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _format.Save(FileInfo.Create(), File.Create(binFilename));
                _format.Close();
            }
            else
            {
                // Create the temp file(s)
                _format.Save(File.Create(FileInfo.FullName + ".tmp"), File.Create(binFilename + ".tmp"));
                _format.Close();
                // Delete the original(s)
                FileInfo.Delete();
                File.Delete(binFilename);
                // Rename the temporary file(s)
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
                File.Move(binFilename + ".tmp", binFilename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _format?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _format.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
