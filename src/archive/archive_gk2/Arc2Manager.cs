using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_gk2.arc2
{
    public class Arc2Manager : IArchiveManager
    {
        private ARC2 _arc2 = null;

        #region Properties

        // Information
        public string Name => "GK2 Arc2";
        public string Description => "Gyakuten Kenji 2 Archive 2";
        public string Extension => "*.bin";
        public string About => "This is the Arc2 archive manager for Karameru.";

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
                    var limit = br.ReadUInt32();
                    if (limit >= br.BaseStream.Length) return false;

                    var off = limit;
                    while (br.BaseStream.Position < limit)
                    {
                        var off2 = br.ReadUInt32();
                        if (off2 <= off) return false;

                        off = off2;
                    }

                    return true;
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
                _arc2 = new ARC2(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _arc2.Save(FileInfo.Create());
                _arc2.Close();
            }
            else
            {
                // Create the temp file
                _arc2.Save(File.Create(FileInfo.FullName + ".tmp"));
                _arc2.Close();
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
            _arc2?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _arc2.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
