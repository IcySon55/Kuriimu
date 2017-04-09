using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using archive_sarc.Properties;
using Cetera.Archive;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_sarc
{
    public class SarcAdapter : IArchiveManager
    {
        private FileInfo _fileInfo = null;
        private SARC _sarc = null;
        private SimpleSARC _ssarc = null;
        private byte Identifier = 0;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Simple ARChive";
        public string Extension => "*.sarc;*.arc";
        public string About => "This is the SARC archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => true;
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
                if (br.ReadString(4) == "SARC")
                {
                    br.BaseStream.Position = 6;
                    ushort ind = br.ReadUInt16();
                    if (ind != 0xfeff && ind != 0xfffe)
                        Identifier = 1;
                    return true;
                }

                return false;
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
            {
                if (Identifier == 0)
                    _sarc = new SARC(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
                else if (Identifier == 1)
                    _ssarc = new SimpleSARC(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
            }
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
                if (Identifier == 0)
                    _sarc.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
                else if (Identifier == 1)
                    _ssarc.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
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
                if (Identifier == 0)
                    return _sarc.Files;
                else
                    return _ssarc.Files;
            }
        }

        public bool AddFile(ArchiveFileInfo afi)
        {
            try
            {
                if (Identifier == 0)
                    _sarc.Files.Add(new SARC.SARCFileInfo()
                    {
                        FileName = afi.FileName,
                        State = afi.State,
                        FileData = afi.FileData
                    });
                else if (Identifier == 1)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
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
