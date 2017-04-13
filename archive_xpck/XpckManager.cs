using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using archive_xpck.Properties;
using Cetera.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Hash;

namespace archive_xpck
{
    public class XpckManager : IArchiveManager
    {
        private FileInfo _fileInfo = null;
        private XPCK _xpck = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Level 5 eXtractable PaCKage";
        public string Extension => "*.xa;*.xc;*.xf;*.xk;*.xl;*.xr;*.xv";
        public string About => "This is the XPCK archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo
        {
            get
            {
                return _fileInfo;
            }
            set
            {
                _fileInfo = value;
            }
        }

        #endregion

        public bool Identify(string filename)
        {
            //Console.WriteLine(Crc32.Create(new byte[] { 0x30, 0x30, 0x30, 0x2e, 0x61, 0x74, 0x72 }));
            //return false;

            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                if (br.ReadString(4) == "XPCK") return true;

                br.BaseStream.Position = 0;
                byte[] decomp;
                try { decomp = CriWare.GetDecompressedBytes(br.BaseStream); } catch { return false; }
                return new BinaryReaderX(new MemoryStream(decomp)).ReadString(4) == "XPCK";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _xpck = new XPCK(_fileInfo.FullName);
            else
                result = LoadResult.FileNotFound;

            return result;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (filename.Trim() != string.Empty)
                _fileInfo = new FileInfo(filename);

            try
            {
                // Save As...
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    _xpck.Save(File.Create(_fileInfo.FullName));
                }
                else
                {
                    // Create the temp file
                    _xpck.Save(File.Create(_fileInfo.FullName + ".tmp"));
                    // Delete the original
                    _fileInfo.Delete();
                    // Rename the temporary file
                    File.Move(_fileInfo.FullName + ".tmp", _fileInfo.FullName);
                }

                // Reload the new file to make sure everything is in order
                Load(_fileInfo.FullName);
            }
            catch (Exception)
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        public void Unload()
        {

        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                return _xpck.Files;
            }
        }

        public bool AddFile(ArchiveFileInfo afi)
        {
            try
            {
                _xpck.Files.Add(new XPCKFileInfo()
                {
                    Entry = new Entry()
                    {
                        crc32 = Crc32.Create(Encoding.ASCII.GetBytes(afi.FileName)),
                        ID = (ushort)(_xpck.Files.Count * 8),
                        fileSize = (int)afi.FileData.Length
                    },
                    FileData = afi.FileData,
                    FileName = afi.FileName,
                    State = afi.State
                });
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi)
        {
            return false;
        }

        // Features
        public bool ShowProperties(Icon icon)
        {
            return false;
        }
    }
}
