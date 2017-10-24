using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_gk2.arc1
{
    public class Arc1Manager : IArchiveManager
    {
        private ARC1 _arc1 = null;

        #region Properties

        // Information
        public string Name => "GK2 Arc1";
        public string Description => "Gyakuten Kenji 2 Archive 1";
        public string Extension => "*.bin";
        public string About => "This is the Arc1 archive manager for Karameru.";

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
                try
                {
                    var entry = br.ReadStruct<Entry>();
                    br.BaseStream.Position = entry.offset - 8;
                    return br.ReadUInt32() == br.BaseStream.Length && br.ReadUInt32() == 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _arc1 = new ARC1(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _arc1.Save(FileInfo.Create());
                _arc1.Close();
            }
            else
            {
                // Create the temp file
                _arc1.Save(File.Create(FileInfo.FullName + ".tmp"));
                _arc1.Close();
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
            _arc1?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _arc1.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
