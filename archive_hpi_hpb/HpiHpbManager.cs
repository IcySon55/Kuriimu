using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using archive_hpi_hpb.Properties;
using Kuriimu.Contract;
using Kuriimu.IO;

//Notes:
//Load in the class constructor
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
            public HPIHPB.Node node;
            public override Stream FileData => base.FileData ?? node.GetUncompressedStream();
        }

        private FileInfo _fileInfo = null;
        private HPIHPB _hpihpb = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Atlus Archive (for EOV)";
        public string Extension => "*.hpi;*.hpb";
        public string About => "This is the HPI/HPB archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => true;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
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
            String hpiFilename = filename.Remove(filename.Length - 1) + "i";
            String hpbFilename = filename.Remove(filename.Length - 1) + "b";
            if (!File.Exists(hpiFilename) || !File.Exists(hpbFilename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(hpiFilename)))
            {
                return br.BaseStream.Length >= 4 && br.ReadString(4) == "HPIH";
            }
        }

        public LoadResult Load(string filename)
        {
            var hpiFilename = filename.Remove(filename.Length - 1) + "i";
            var hpbFilename = filename.Remove(filename.Length - 1) + "b";

            _fileInfo = new FileInfo(filename);

            if (!File.Exists(hpiFilename) || !File.Exists(hpbFilename))
            {
                return LoadResult.FileNotFound;
            }

            _hpihpb = new HPIHPB(hpiFilename, hpbFilename);
            return LoadResult.Success;
        }

        public SaveResult Save(string filename)
        {
            SaveResult result = SaveResult.Success;

            if (!string.IsNullOrWhiteSpace(filename))
                _fileInfo = new FileInfo(filename);

            try
            {
                // Save As...
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    _hpihpb.Save(File.Create(_fileInfo.FullName + ".hpi"), File.Create(_fileInfo.FullName + ".hpb"), Files);
                    _hpihpb.Dispose();
                }
                else
                {
                    // Create the temp file
                    _hpihpb.Save(File.Create(_fileInfo.FullName + ".hpi.tmp"), File.Create(_fileInfo.FullName + ".hpb.tmp"), Files);
                    _hpihpb.Dispose();
                    // Delete the original
                    _fileInfo.Delete();
                    // Rename the temporary file
                    File.Move(_fileInfo.FullName + "hpi.tmp", _fileInfo.FullName);
                }

                // Reload the new file to make sure everything is in order
                Load(_fileInfo.FullName);
            }
            catch
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        public void Unload()
        {
            _hpihpb.Dispose();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _hpihpb.Select(node => new HpiHpbAfi { node = node, FileName = node.filename });

        public bool AddFile(ArchiveFileInfo afi)
        {
            try
            {
                _hpihpb.Add(new HPIHPB.Node
                {
                    filename = afi.FileName,
                    fileData = afi.FileData
                });
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool RenameFile(ArchiveFileInfo afi)
        {
            throw new NotSupportedException();
        }

        public bool ReplaceFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo afi)
        {
            throw new NotSupportedException();
        }

        // Features
        public bool ShowProperties(Icon icon)
        {
            return false;
        }
    }
}
