using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using System.Drawing;
using System.IO;
using archive_sarc.Properties;
using Cetera.Archive;

namespace archive_sarc
{
    public class SarcAdapter : IArchiveManager
    {
        public class SarcAfi : ArchiveFileInfo
        {
            public SARC.State state;
            public SARC.SimplerSFATNode sFatNode;
            public SARC.SFATNode fatNode;
        }

        private FileInfo _fileInfo = null;
        private SARC _sarc = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Simple ARChive";
        public string Extension => "*.sarc;*.arc";
        public string About => "This is the SARC archive manager for Karameru.";

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
                string magic = br.ReadString(4);
                return magic == "SARC";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _sarc = new SARC(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
                _sarc.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
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

                foreach (var node in _sarc)
                {
                    var file = new ArchiveFileInfo();
                    //file.Filesize = (node.nodeEntry != null) ? node.nodeEntry.dataEnd - node.nodeEntry.dataStart : node.sNodeEntry.dataLength;
                    file.Filename = node.fileName;
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
