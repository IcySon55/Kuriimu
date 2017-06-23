using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_fnt
{
    public class FntManager : IArchiveManager
    {
        private FNT _fnt = null;

        #region Properties

        // Information
        public string Name => "FNT";
        public string Description => "Whatever FNT should mean";
        public string Extension => "*.fnt";
        public string About => "This is the FNT archive manager for Karameru.";

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
            try
            {
                using (var br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    if (br.BaseStream.Length < 4) return false;
                    var entrycount = br.ReadInt32();
                    if (br.BaseStream.Length < 4 + entrycount * 0x4) return false;
                    br.BaseStream.Position = 4 + entrycount * 0x4;
                    return (br.ReadInt32() == br.BaseStream.Length);
                }
            } catch
            {
                return false;
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
