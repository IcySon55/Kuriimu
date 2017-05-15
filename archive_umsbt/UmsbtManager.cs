using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_umsbt
{
    public class UmsbtManager : IArchiveManager
    {
        private UMSBT _umsbt = null;
        bool plain = false;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "UMSBT Archive";
        public string Extension => "*.umsbt";
        public string About => "This is the UMSBT archive manager for Karameru.";

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
            // TODO: Make this way more robust
            if (filename.EndsWith(".umsbt")) return true;

            //PlainMSBT
            using (var br=new BinaryReaderX(File.OpenRead(filename)))
            {
                br.BaseStream.Position = 0x38;
                var dataOffset = br.ReadUInt32();
                var size = br.ReadUInt32();
                var totalSize = dataOffset;
                while (size != 0 && br.BaseStream.Position < dataOffset)
                {
                    totalSize += size;
                    while (totalSize % 0x80 != 0) totalSize++;
                    br.BaseStream.Position += 0x3c;
                    size = br.ReadUInt32();
                }

                return br.BaseStream.Length == totalSize;
            }
        }

        public void Load(string filename)
        {
            //determine plainMSBT
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                br.BaseStream.Position = 0x38;
                var dataOffset = br.ReadUInt32();
                var size = br.ReadUInt32();
                var totalSize = dataOffset;
                while (size != 0 && br.BaseStream.Position < dataOffset)
                {
                    totalSize += size;
                    while (totalSize % 0x80 != 0) totalSize++;
                    br.BaseStream.Position += 0x3c;
                    size = br.ReadUInt32();
                }

                plain = (br.BaseStream.Length == totalSize);
            }

            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _umsbt = new UMSBT(FileInfo.OpenRead(), plain);
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _umsbt.Save(FileInfo.Create(), plain);
                _umsbt.Close();
            }
            else
            {
                // Create the temp file
                _umsbt.Save(File.Create(FileInfo.FullName + ".tmp"), plain);
                _umsbt.Close();
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
            _umsbt?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _umsbt.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
