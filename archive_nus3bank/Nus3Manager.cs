using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using archive_nus3bank.Properties;
using Cetera.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_nus3bank
{
    public sealed class Nus3Manager : IArchiveManager
    {
        private FileInfo _fileInfo = null;
        private NUS3 _nus3 = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "NUS3BANK Sound Archive";
        public string Extension => "*.nus3bank";
        public string About => "This is the NUS3BANK archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

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
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                if (br.ReadString(4) == "NUS3")
                {
                    return true;
                }
                else
                {
                    try
                    {
                        br.BaseStream.Position = 0;
                        byte[] decomp = ZLib.Decompress(br.ReadBytes((int)br.BaseStream.Length));
                        using (var br2 = new BinaryReaderX(new MemoryStream(decomp)))
                        {
                            if (br.BaseStream.Length < 4) return false;
                            if (br.ReadString(4) == "NUS3")
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                };

                return false;
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _nus3 = new NUS3(_fileInfo.FullName);
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
                //_nus3.Save(_fileInfo.FullName);
            }
            catch
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        public void Unload()
        {
            // TODO: Implement closing open handles here
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                foreach (var node in _nus3)
                {
                    yield return new ArchiveFileInfo
                    {
                        FileName = node.filename,
                        FileData = node.FileData
                    };
                }
            }
        }

        public bool AddFile(ArchiveFileInfo afi)
        {
            throw new NotSupportedException();
        }

        public bool RenameFile(ArchiveFileInfo afi)
        {
            throw new NotSupportedException();
        }

        public bool ReplaceFile(ArchiveFileInfo afi)
        {
            throw new NotSupportedException();
        }

        public bool DeleteFile(ArchiveFileInfo afi)
        {
            throw new NotSupportedException();
        }

        // Features
        public bool ShowProperties(Icon icon)
        {
            return false;
        }
    }
}
