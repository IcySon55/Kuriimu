using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_skb
{
    public class SKBManager : IArchiveManager
    {
        private SKB _skb = null;

        #region Properties

        // Information
        public string Name => "SKB";
        public string Description => "Senran Kagura Burst Archive";
        public string Extension => "*.bin";
        public string About => "This is the SKB archive manager for Karameru.";

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
                var t = br.ReadBytes(0x18);
                return t[0] == 0xe && t[8] == 4 && t[12] == 4 && t[16] == 4 && t[20] == 0x80;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _skb = new SKB(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _skb.Save(FileInfo.Create());
                _skb.Close();
            }
            else
            {
                // Create the temp file
                _skb.Save(File.Create(FileInfo.FullName + ".tmp"));
                _skb.Close();
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
            _skb?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _skb.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
