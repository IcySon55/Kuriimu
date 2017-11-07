using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_enc
{
    public class archive_encManager : IArchiveManager
    {
        private ENC _enc = null;

        #region Properties

        // Information
        public string Name => "ENC";
        public string Description => "ENC Archive";
        public string Extension => "*.enc";
        public string About => "This is the ENC archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            try
            {
                using (var br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    var count = br.ReadUInt32();
                    uint offset = 0;
                    for (int i = 0; i < count; i++)
                    {
                        br.ReadUInt64();
                        var offset2 = br.ReadUInt32();
                        if (offset > offset2)
                            return false;

                        offset = offset2;
                    }

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _enc = new ENC(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _enc.Save(FileInfo.Create());
                _enc.Close();
            }
            else
            {
                // Create the temp file
                _enc.Save(File.Create(FileInfo.FullName + ".tmp"));
                _enc.Close();
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
            _enc?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _enc.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
