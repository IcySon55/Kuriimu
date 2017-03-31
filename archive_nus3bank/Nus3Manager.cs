using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using archive_nus3bank.Properties;
using KuriimuContract;
using System.IO;
using System.Drawing;
using Cetera.Compression;

namespace archive_nus3bank
{
    public sealed class Nus3Manager : IArchiveManager
    {
        public class CtpkAfi : ArchiveFileInfo
        {
            //public NUS3.NodeEntry nodeEntry;
        }

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
            catch (Exception)
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                var files = new List<ArchiveFileInfo>();

                foreach (var node in _nus3)
                {
                    var file = new ArchiveFileInfo();
                    //file.Filesize = node.entry.size;
                    file.FileName = node.filename;
                    file.FileData = node.FileData;
                    files.Add(file);
                }

                return files;
            }
        }

        public bool AddFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool RenameFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool ReplaceFile(ArchiveFileInfo afi)
        {
            return false;
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
