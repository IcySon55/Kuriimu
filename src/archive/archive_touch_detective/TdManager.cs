using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_td
{
    public class TdManager : IArchiveManager
    {
        private TD _td = null;

        #region Properties

        // Information
        public string Name => "TD";
        public string Description => "Touch Detective Archive";
        public string Extension => "*.bin";
        public string About => "This is the Touch Detective archive manager for Karameru.";

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
                var count = br.ReadUInt32();
                uint offset = 0;
                for (int i = 0; i < count; i++)
                {
                    var offset2 = br.ReadUInt32();
                    if (offset2 < offset)
                        return false;

                    br.ReadUInt32();

                    offset = offset2;
                }

                br.BaseStream.Position -= 4;
                var lastSize = br.ReadUInt32();
                return br.BaseStream.Length == offset * 4 + lastSize;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _td = new TD(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _td.Save(FileInfo.Create());
                _td.Close();
            }
            else
            {
                // Create the temp file
                _td.Save(File.Create(FileInfo.FullName + ".tmp"));
                _td.Close();
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
            _td?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _td.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
