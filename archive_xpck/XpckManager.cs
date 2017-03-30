using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using System.Drawing;
using System.IO;
using archive_xpck.Properties;
using Cetera.Compression;

namespace archive_xpck
{
    public class SarcAdapter : IArchiveManager
    {
        public class XpckAfi : ArchiveFileInfo
        {
            XPCK.Entry pckEntry;
        }

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
                if (br.ReadString(4) == "XPCK") return true;

                br.BaseStream.Position = 0;
                byte[] decomp = CriWare.GetDecompressedBytes(br.BaseStream);
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
                //_xpck.Save(_fileInfo.FullName);
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

                foreach (var node in _xpck)
                {
                    var file = new ArchiveFileInfo();
                    //file.Filesize = node.entry.fileSize;
                    file.Filename = node.filename;
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
