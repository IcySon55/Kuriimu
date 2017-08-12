using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_srtux
{
    public class SrtuxManager : IArchiveManager
    {
        private SRTUX _srtux = null;

        #region Properties

        // Information
        public string Name => "SRTUX";
        public string Description => "SRTUX Archive";
        public string Extension => "*.bin";
        public string About => "This is the SRTUX archive manager for Karameru.";

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
                    uint limit = br.ReadUInt32();
                    br.BaseStream.Position = 0;
                    uint precheck = 0;
                    uint length = 0;
                    while (br.BaseStream.Position < limit)
                    {
                        var check = br.ReadUInt32();
                        if (check == 0)
                        {
                            length = precheck;
                            break;
                        }
                        precheck = check;
                    }

                    br.BaseStream.Position = br.BaseStream.Length - 4;
                    return (length == br.BaseStream.Length) ? true : (br.ReadString(3) == "end");
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
                _srtux = new SRTUX(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _srtux.Save(FileInfo.Create());
                _srtux.Close();
            }
            else
            {
                // Create the temp file
                _srtux.Save(File.Create(FileInfo.FullName + ".tmp"));
                _srtux.Close();
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
            _srtux?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _srtux.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
