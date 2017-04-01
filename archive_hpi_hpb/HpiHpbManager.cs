using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using System.Drawing;
using System.IO;
using archive_hpi_hpb.Properties;
using Cetera.Compression;

namespace archive_hpi_hpb
{
    public class HpiHpbAdapter : IArchiveManager
    {
        public class HpiHpbAfi : ArchiveFileInfo
        {
            public HPIHPB.Entry entry;
        }

        private FileInfo _fileInfo = null;
        private HPIHPB _hpihpb = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Atlus Archive (for EOV)";
        public string Extension => "*.HPI;*.HPB";
        public string About => "This is the HPI/HPB archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanAddDirectories => false;
        public bool CanRenameDirectories => false;
        public bool CanDeleteDirectories => false;
        public bool CanSave => false;
        public bool CanReplaceFiles => false;

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
            //throw new Exception("Test");
            /*using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                byte[] decomp = LZ77.Compress(br.ReadBytes((int)br.BaseStream.Length));
                File.OpenWrite(Path.GetDirectoryName(filename) + "\\t.comp").Write(decomp, 0, decomp.Length);
            }

            return false;*/

            //get HPB filename
            String hpbFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename).Split('.')[0] + ".HPB";
            if (File.Exists(hpbFilename) == false)
            {
                return false;
            }

            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "HPIH";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            String hpbFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename).Split('.')[0] + ".HPB";

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _hpihpb = new HPIHPB(_fileInfo.FullName, hpbFilename);
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
                //_hpihpb.Save(_fileInfo.FullName);
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

                foreach (var node in _hpihpb)
                {
                    var file = new ArchiveFileInfo();
                    //file.Filesize = node.entry.fileSize;
                    file.FileName = node.filename;
                    file.FileData = node.fileData;
                    files.Add(file);
                }

                return files;
            }
        }

        public bool RenameFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool ReplaceFile(ArchiveFileInfo afi)
        {
            return false;
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
