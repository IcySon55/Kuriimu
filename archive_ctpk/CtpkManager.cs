using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using archive_ctpk.Properties;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Hash;

namespace archive_ctpk
{
    public class CTPKAdapter : IArchiveManager
    {
        private FileInfo _fileInfo = null;
        private CTPK _ctpk = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "CTR Texture PacKage";
        public string Extension => "*.ctpk";
        public string About => "This is the CTPK archive manager for Karameru.";

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
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "CTPK";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _ctpk = new CTPK(_fileInfo.FullName);
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
                _ctpk.Save(_fileInfo.FullName);
            }
            catch (Exception)
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
                return _ctpk.Files;
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
