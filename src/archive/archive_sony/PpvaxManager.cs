using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_sony.PPVAX
{
    class PpvaxManager : IArchiveManager
    {
        private PPVAX _ppvax;

        #region Properties

        // Information
        public string Name => "PPVAX";
        public string Description => "Persona 1 Audio Binary";
        public string Extension => "*.bin";
        public string About => "This is the PPVAX archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanReplaceFiles => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            if (!File.Exists(filename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length >= 4)
                {
                    if (br.ReadInt32() == 2048)
                        return true;
                    else
                        br.BaseStream.Position = 0;

                    for (var i = 0; i < 6; i++)
                        if (br.ReadInt32() != 0)
                            return false;

                    if (br.ReadInt32() == 2048)
                        return true;
                }

                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            _ppvax = new PPVAX(File.OpenRead(filename));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _ppvax.Save(FileInfo.Create());
                _ppvax.Close();
            }
            else
            {
                // Create the temp file
                _ppvax.Save(File.Create(FileInfo.FullName + ".tmp"));
                _ppvax.Close();
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
            _ppvax?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _ppvax.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
