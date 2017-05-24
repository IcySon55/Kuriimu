using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_seg
{
    public class SegManager : IArchiveManager
    {
        private SEG _seg = null;
        private bool _hasSize = false;

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
                var tmp = br.ReadUInt32();
                while (tmp == 0 && br.BaseStream.Position >= 0)
                {
                    br.BaseStream.Position -= 8;
                    if (br.BaseStream.Position < 0) return false;
                    tmp = br.ReadUInt32();
                }
                return tmp == new FileInfo(binFilename).Length;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            var binFilename = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".bin");
            var sizeFilename = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + "size.bin");
            _hasSize = File.Exists(sizeFilename);

            if (FileInfo.Exists)
                _seg = new SEG(FileInfo.OpenRead(), File.OpenRead(binFilename), _hasSize ? File.OpenRead(sizeFilename) : null);
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);
            var binFilename = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".bin");
            var sizeFilename = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + "size.bin");

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _seg.Save(FileInfo.Create(), File.Create(binFilename), _hasSize ? File.Create(sizeFilename) : null);
                _seg.Close();
            }
            else
            {
                if (!_hasSize)
                {
                    // Create the temp file(s)
                    _seg.Save(File.Create(FileInfo.FullName + ".tmp"), File.Create(binFilename + ".tmp"));
                    _seg.Close();
                    // Delete the original(s)
                    FileInfo.Delete();
                    File.Delete(binFilename);
                    // Rename the temporary file(s)
                    File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
                    File.Move(binFilename + ".tmp", binFilename);
                }
                else
                {
                    // Create the temp file(s)
                    _seg.Save(File.Create(FileInfo.FullName + ".tmp"), File.Create(binFilename + ".tmp"), File.Create(sizeFilename + ".tmp"));
                    _seg.Close();
                    // Delete the original(s)
                    FileInfo.Delete();
                    File.Delete(binFilename);
                    File.Delete(sizeFilename);
                    // Rename the temporary file(s)
                    File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
                    File.Move(binFilename + ".tmp", binFilename);
                    File.Move(sizeFilename + ".tmp", sizeFilename);
                }
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _seg?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _seg.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
