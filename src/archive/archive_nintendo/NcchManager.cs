using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace archive_nintendo.NCCH
{
    public class NcchManager : IArchiveManager
    {
        private NCCH _ncch = null;

        #region Properties

        // Information
        public string Name => "NCCH";
        public string Description => "Nintendo Content Container Header";
        public string Extension => "*.bin;*.ncch;*.cxi;*.cfa";
        public string About => "This is the NCCH manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => true;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => true;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                try
                {
                    br.BaseStream.Position = 0x100;
                    return br.ReadString(4) == "NCCH";
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
                _ncch = new NCCH(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _ncch.Save(FileInfo.Create());
                _ncch.Close();
            }
            else
            {
                // Create the temp file
                _ncch.Save(File.Create(FileInfo.FullName + ".tmp"));
                _ncch.Close();
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
            _ncch?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _ncch.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            var exist = _ncch.Files.Where(f => f.FileName.Replace('\\', '/') == afi.FileName.Replace('\\', '/')).Any();
            if (!exist)
                _ncch.Files.Add(afi);
            else
                return false;

            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi)
        {
            var toDelete = _ncch.Files.Where(f => f == afi);
            foreach (var d in toDelete)
                d.State = ArchiveFileState.Deleted;

            return true;
        }

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
