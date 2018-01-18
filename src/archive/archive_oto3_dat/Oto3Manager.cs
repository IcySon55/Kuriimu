using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_oto3_dat
{
    public class Oto3Manager : IArchiveManager
    {
        private OTO3 _oto3 = null;

        #region Properties

        // Information
        public string Name => "OTO3 DAT";
        public string Description => "Osawari Tantei 3 Archive";
        public string Extension => "*.dat";
        public string About => "This is the Osawari Tantei 3 archive manager for Karameru.";

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
                try
                {
                    var count = br.ReadInt32();
                    var nameBuf = br.ReadInt32();

                    var lastEntryOff = 0x10 + (0x8 + nameBuf) * (count - 1);
                    br.BaseStream.Position = lastEntryOff;

                    var off = br.ReadInt32();
                    var size = br.ReadInt32();

                    return br.BaseStream.Length == off + size;
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
                _oto3 = new OTO3(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _oto3.Save(FileInfo.Create());
                _oto3.Close();
            }
            else
            {
                // Create the temp file
                _oto3.Save(File.Create(FileInfo.FullName + ".tmp"));
                _oto3.Close();
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
            _oto3?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _oto3.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
