using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_asg_fnt
{
    public class FntManager : IArchiveManager
    {
        private FNT _fnt = null;

        #region Properties

        // Information
        public string Name => "FNT";
        public string Description => "font Archive from Azure Striker Gunvolt";
        public string Extension => "*.fnt";
        public string About => "This is the FNT archive from Azure Striker Gunvolt manager for Karameru.";

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
                var count = br.ReadInt32();
                br.BaseStream.Position += count * 4;
                var offset = br.ReadInt32();

                return br.BaseStream.Length == offset;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _fnt = new FNT(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _fnt.Save(FileInfo.Create());
                _fnt.Close();
            }
            else
            {
                // Create the temp file
                _fnt.Save(File.Create(FileInfo.FullName + ".tmp"));
                _fnt.Close();
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
            _fnt?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _fnt.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
