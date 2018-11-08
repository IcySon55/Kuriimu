using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace archive_nintendo.PFS0
{
    public class PFS0Manager : IArchiveManager
    {
        private PFS0 _pfs0 = null;

        #region Properties

        // Information
        public string Name => "PFS0";
        public string Description => "PFS0";
        public string Extension => "*.nsp;*.pfs;*.pfs0";
        public string About => "This is the PFS0 manager for Karameru.";

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
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                try
                {
                    var magic = br.ReadString(4);
                    return magic == "PFS0";
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
                _pfs0 = new PFS0(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _pfs0.Save(FileInfo.Create());
                _pfs0.Close();
            }
            else
            {
                // Create the temp file
                _pfs0.Save(File.Create(FileInfo.FullName + ".tmp"));
                _pfs0.Close();
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
            _pfs0?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _pfs0.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo afi)
        {
            return false;
        }

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
