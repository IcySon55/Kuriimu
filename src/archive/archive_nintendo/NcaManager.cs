using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace archive_nintendo.NCA
{
    public class NcaManager : IArchiveManager
    {
        private NCA _nca = null;

        #region Properties

        // Information
        public string Name => "NCA";
        public string Description => "Nintendo Content Archive";
        public string Extension => "*.nca";
        public string About => "This is the NCA manager for Karameru.";

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
                    br.BaseStream.Position = 0x200;
                    var magic = br.ReadString(4);
                    return magic == "NCA2" || magic == "NCA3";
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
                _nca = new NCA(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _nca.Save(FileInfo.Create());
                _nca.Close();
            }
            else
            {
                // Create the temp file
                _nca.Save(File.Create(FileInfo.FullName + ".tmp"));
                _nca.Close();
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
            _nca?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _nca.Files;

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
