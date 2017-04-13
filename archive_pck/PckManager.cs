using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using archive_pck.Properties;
using Kuriimu.Contract;
using Kuriimu.IO;
using Cetera.Hash;

namespace archive_pck
{
    public class PckManager : IArchiveManager
    {
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
            return filename.EndsWith(".pck");
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

        public void Unload()
        {
            // TODO: Implement closing open handles here
        }

        //Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                return _pck.Files;
            }
        }

        public bool AddFile(ArchiveFileInfo afi)
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
