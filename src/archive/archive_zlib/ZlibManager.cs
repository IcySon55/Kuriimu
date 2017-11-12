﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Text;
using Kontract.Compression;

namespace archive_zlib
{
    public class ZlibManager : IArchiveManager
    {
        private ZLIB _zlib = null;

        #region Properties

        // Information
        public string Name => "ZLIB";
        public string Description => "ZLib compressed with 4 byte decompressed size";
        public string Extension => "*.zlib";
        public string About => "This is the ZLIB archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename), ByteOrder.BigEndian))
            {
                try
                {
                    var decompSize = br.ReadUInt32();
                    var decomp = ZLib.Decompress(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length - 4)));
                    return decompSize == decomp.Length;
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
                _zlib = new ZLIB(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _zlib.Save(FileInfo.Create());
                _zlib.Close();
            }
            else
            {
                // Create the temp file
                _zlib.Save(File.Create(FileInfo.FullName + ".tmp"));
                _zlib.Close();
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
            _zlib?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _zlib.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
