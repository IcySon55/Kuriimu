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
using Cetera.IO;

namespace archive_hpi_hpb
{
    public class HpiHpbAdapter : IArchiveManager
    {
        public class HpiHpbAfi : ArchiveFileInfo
        {
            public HPIHPB.Entry entry;
            public SubStream compStream;
            public override Stream FileData
            {
                get
                {
                    byte[] header = new byte[0x20];
                    compStream.Read(header, 0, 0x20);
                    byte[] comp = new byte[(int)entry.fileSize - 0x20];
                    compStream.Read(comp, 0, (int)entry.fileSize - 0x20);

                    using (Cetera.IO.BinaryReaderX br = new Cetera.IO.BinaryReaderX(new MemoryStream(header)))
                    {
                        if (br.ReadStruct<KuriimuContract.Magic>() == "ACMP")
                        {
                            br.BaseStream.Position = 0x10;
                            Stream t = new MemoryStream(RevLZ77.Decompress(comp, br.ReadUInt32()));
                            return t;
                        }
                        else
                        {
                            return compStream;
                        }
                    }
                }
            }
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
            /*using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                byte[] decomp = RevLZ77.Compress(br.ReadBytes((int)br.BaseStream.Length));
                var t = File.OpenWrite(Path.GetDirectoryName(filename) + "\\t.comp");
                t.Write(decomp, 0, decomp.Length);
                t.Close();
            }

            return false;*/

            //get HPB filename
            String hpbFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename).Split('.')[0] + ".HPB";
            if (File.Exists(hpbFilename) == false)
            {
                return false;
            }

            using (var br = new KuriimuContract.BinaryReaderX(File.OpenRead(filename)))
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

        public void Unload()
        {
            // TODO: Implement closing open handles here
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                var files = new List<HpiHpbAfi>();

                foreach (var node in _hpihpb)
                {
                    var file = new HpiHpbAfi();

                    file.entry = node.entry;
                    file.FileName = node.filename;
                    file.compStream = node.fileData;

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
