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

//Notes:
//Load in the class constructor
//Don't use using directive if you want to leave a handle open!!
//If compressed Files are given in the archive:
// -> make a new ArchiveFileInfo derived from the original
// -> set all needed parameters in there
// -> create at least one more stream in it (maybe named compStream)
// -> override get of FileData and compress the compStream there
// -->> To get the files then in IEnumerable Files, create a list of this derived AFI
//After replacing formerly compressed files:
// -> DO NOT override the set of FileData
// -> In the Save/Save As... function that is called, recompress everything depending on Afi.State
// -->> If State is Replaced or Added=Recompress and evaluate all sizes (offset, compSize etc.)
// -->> If State==Archived just copy over compStream but reevaluate offset

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
                    byte[] header = new byte[0x20]; compStream.Read(header, 0, 0x20);
                    byte[] comp = new byte[(int)entry.fileSize - 0x20]; compStream.Read(comp, 0, (int)entry.fileSize - 0x20);

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
        public bool CanAddFiles => true;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanAddDirectories => false;
        public bool CanRenameDirectories => false;
        public bool CanDeleteDirectories => false;
        public bool CanSave => true;
        public bool CanReplaceFiles => true;

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

            //get other filename
            String otherFilename;
            if (filename.EndsWith(".HPI"))
                otherFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename).Split('.')[0] + ".HPB";
            else if (filename.EndsWith(".HPB"))
                otherFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename).Split('.')[0] + ".HPI";
            else return false;

            if (!File.Exists(otherFilename)) return false;

            using (var br = new KuriimuContract.BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "HPIH";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            String otherFilename;
            if (filename.EndsWith(".HPI"))
                otherFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename).Split('.')[0] + ".HPB";
            else
                otherFilename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename).Split('.')[0] + ".HPI";

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists && File.Exists(otherFilename))
                _hpihpb = new HPIHPB(_fileInfo.FullName, otherFilename);
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
                // Save As...
                if (filename.Trim() != string.Empty)
                {
                    //_hpihpb.Save(File.Create(_fileInfo.FullName + ".hpi"), File.Create(_fileInfo.FullName + ".hpb"));
                    _hpihpb.Close();
                }
                else
                {
                    // Create the temp file
                    //_hpihpb.Save(File.Create(_fileInfo.FullName + ".hpi.tmp"), File.Create(_fileInfo.FullName + ".hpb.tmp"));
                    _hpihpb.Close();
                    // Delete the original
                    File.Delete(_fileInfo.FullName);
                    // Rename the temporary file
                    File.Move(_fileInfo.FullName + "hpi.tmp", _fileInfo.FullName);
                }

                // Reload the new file to make sure everything is in order
                Load(_fileInfo.FullName);
            }
            catch (Exception)
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        public void Unload()
        {
            _hpihpb.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                var files = new List<HpiHpbAfi>();

                foreach (var node in _hpihpb.nodes)
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
            try
            {
                _hpihpb.nodes.Add(new HPIHPB.Node()
                {
                    filename = afi.FileName,
                    entry = new HPIHPB.Entry()
                    {
                        offset = 0,
                        fileSize = (uint)afi.FileData.Length
                    },
                    fileData = new SubStream(afi.FileData, 0, afi.FileData.Length)
                });
            }
            catch (Exception)
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
