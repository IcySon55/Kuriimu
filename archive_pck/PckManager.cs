using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using System.Drawing;
using System.IO;
using archive_pck.Properties;
using Cetera.Archive;

namespace archive_pck
{
    public class SarcAdapter : IArchiveManager
    {
        public class PckAfi : ArchiveFileInfo
        {
            public PCK.Entry pckEntry;
        }

        private FileInfo _fileInfo = null;
        private PCK _pck = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Level 5 PaCKage";
        public string Extension => "*.pck";
        public string About => "This is the PCK archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanAddDirectories => false;
        public bool CanRenameDirectories => false;
        public bool CanDeleteDirectories => false;
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
            if (filename.Contains(".pck"))
            {
                return true;
            }

            return false;
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _pck = new PCK(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
                _pck.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
            }
            catch (Exception)
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        public string Compression => "none";

        public bool FilesHaveVaryingCompressions => false;

        // Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                var files = new List<ArchiveFileInfo>();

                foreach (var node in _pck)
                {
                    var file = new ArchiveFileInfo();
                    file.Filesize = node.entry.length;
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

        public byte[] GetFile(ArchiveFileInfo afi)
        {
            return new byte[] { 64, 64, 64, 64 };
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
