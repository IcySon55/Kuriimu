using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_umsbt
{
    public class PlainUmsbtManager : IArchiveManager
    {
        private PlainUMSBT _plainUmsbt = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "PlainUMSBT Archive";
        public string Extension => "*.umsbt";
        public string About => "This is the PlainUMSBT archive manager for Karameru.";

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
            using (var br=new BinaryReaderX(File.OpenRead(filename)))
            {
                try
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
                catch(Exception)
                {
                    return false;
                }
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _plainUmsbt = new PlainUMSBT(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _plainUmsbt.Save(FileInfo.Create());
                _plainUmsbt.Close();
            }
            else
            {
                // Create the temp file
                _plainUmsbt.Save(File.Create(FileInfo.FullName + ".tmp"));
                _plainUmsbt.Close();
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
            _plainUmsbt?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _plainUmsbt.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
