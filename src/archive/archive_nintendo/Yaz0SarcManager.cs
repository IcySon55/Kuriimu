using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cetera.Hash;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.SARC
{
    public class Yaz0SarcManager : IArchiveManager
    {
        private SARC _sarc = null;

        #region Properties

        // Information
        public string Name => "Yaz0-SARC";
        public string Description => "Yaz0-Compressed NW4C Sorted ARChive";
        public string Extension => "*.szs";
        public string About => "This is the Yaz0-Compressed SARC archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => true;
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
                if (br.BaseStream.Length < 0x15) return false;
                if (br.ReadString(4) != "Yaz0") return false;
                br.BaseStream.Position = 0x11;
                return br.ReadString(4) == "SARC";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            using (var br = new BinaryReaderX(FileInfo.OpenRead()))
            {
                _sarc = new SARC(new MemoryStream(Yaz0.Decompress(new MemoryStream(br.ReadBytes((int)FileInfo.Length)), ByteOrder.BigEndian)));
            }
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                var ms = new MemoryStream();
                _sarc.Save(ms, true);
                _sarc.Close();
                using (var bw = new BinaryWriterX(FileInfo.Create()))
                {
                    bw.Write(Yaz0.Compress(ms, ByteOrder.BigEndian));
                }
            }
            else
            {
                // Create the temp file
                var ms = new MemoryStream();
                _sarc.Save(ms, true);
                _sarc.Close();
                using (var bw = new BinaryWriterX(File.Create(FileInfo.FullName + ".tmp")))
                {
                    bw.Write(Yaz0.Compress(ms, ByteOrder.BigEndian));
                }
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
            _sarc.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _sarc.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            _sarc.Files.Add(new SarcArchiveFileInfo
            {
                FileData = afi.FileData,
                FileName = afi.FileName,
                State = afi.State,
                hash = SimpleHash.Create(afi.FileName, _sarc.hashMultiplier)
            });
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
