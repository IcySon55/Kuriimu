using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Archive;
using Cetera.Compression;
using System.IO.Compression;

namespace archive_sarc
{
    public class ZLibSarcManager : IArchiveManager
    {
        private SARC _sarc = null;

        #region Properties

        // Information
        public string Name => "ZLib-SARC";
        public string Description => "ZLib-Compressed Simple ARChive";
        public string Extension => "*.zlib";
        public string About => "This is the ZLib-Compressed SARC archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
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
                if (br.BaseStream.Length < 10) return false;
                br.ReadBytes(4);
                if (br.ReadByte() != 0x78) return false;
                var b = br.ReadByte();
                if (b != 0x01 && b != 0x9C && b != 0xDA) return false;
                try
                {
                    using (var br2 = new BinaryReaderX(new DeflateStream(br.BaseStream, CompressionMode.Decompress)))
                    {
                        return br2.ReadString(4) == "SARC";
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        public LoadResult Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            if (!FileInfo.Exists) return LoadResult.FileNotFound;

            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                br.ReadBytes(4);
                _sarc = new SARC(new MemoryStream(ZLib.Decompress(br.ReadBytes((int)FileInfo.Length - 4))));
                return LoadResult.Success;
            }
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            try
            {
                var ms = new MemoryStream();
                _sarc.Save(ms, true);
                using (var bw = new BinaryWriterX(FileInfo.Create()))
                {
                    bw.Write(BitConverter.GetBytes((int)ms.Length).Reverse().ToArray());
                    bw.Write(ZLib.Compress(ms.ToArray()));
                }
            }
            catch
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        // nothing to unload
        public void Unload()
        {
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _sarc.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            _sarc.Files.Add(afi);
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => throw new NotSupportedException();

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
