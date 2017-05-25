using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_nus3bank
{
    public sealed class Nus3Manager : IArchiveManager
    {
        private NUS3 _nus3 = null;
        private bool _isZlibCompressed = false;

        #region Properties

        // Information
        public string Name => "NUS3";
        public string Description => "NUS3BANK Sound Archive";
        public string Extension => "*.nus3bank";
        public string About => "This is the NUS3BANK archive manager for Karameru.";

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
                if (br.BaseStream.Length < 4) return false;
                if (br.ReadString(4) == "NUS3") return true;

                try
                {
                    br.BaseStream.Position = 0;
                    byte[] decomp = ZLib.Decompress(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length)));
                    using (var br2 = new BinaryReaderX(new MemoryStream(decomp)))
                    {
                        if (br.BaseStream.Length < 4) return false;
                        if (br.ReadString(4) == "NUS3")
                        {
                            _isZlibCompressed = true;
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _nus3 = new NUS3(FileInfo.FullName, _isZlibCompressed);
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _nus3.Save(FileInfo.FullName, _isZlibCompressed);
                _nus3.Close();
            }
            else
            {
                // Create the temp file
                _nus3.Save(FileInfo.FullName + ".tmp", _isZlibCompressed);
                _nus3.Close();
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
            _nus3?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _nus3.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
