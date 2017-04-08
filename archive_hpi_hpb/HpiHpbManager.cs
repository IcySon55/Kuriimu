using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private FileInfo _fileInfo = null;
        private HPIHPB _hpihpb = null;

        #region Properties

        // Information
        public string Name => Settings.Default.PluginName;
        public string Description => "Atlus Archive (Etrian Odyssey Series)";
        public string Extension => "*.hpi";
        public string About => "This is the HPI/HPB archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
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
            String hpiFilename = filename;
            String hpbFilename = filename.Remove(filename.Length - 1) + "B";
            if (!File.Exists(hpiFilename) || !File.Exists(hpbFilename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(hpiFilename)))
            {
                return br.BaseStream.Length >= 4 && br.ReadString(4) == "HPIH";
            }
        }

        public LoadResult Load(string filename)
        {
            var hpiFilename = filename;
            var hpbFilename = filename.Remove(filename.Length - 1) + "B";

            _fileInfo = new FileInfo(filename);

            if (!File.Exists(hpiFilename) || !File.Exists(hpbFilename))
            {
                return LoadResult.FileNotFound;
            }

            _hpihpb = new HPIHPB(File.OpenRead(hpiFilename), File.OpenRead(hpbFilename));
            return LoadResult.Success;
        }

        public SaveResult Save(string filename)
        {
            SaveResult result = SaveResult.Success;

            if (!string.IsNullOrWhiteSpace(filename))
                _fileInfo = new FileInfo(filename);

            var hpiFilename = _fileInfo.FullName;
            var hpbFilename = _fileInfo.FullName.Remove(_fileInfo.FullName.Length - 1) + "B";

            try
            {
                // Save As...
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    _hpihpb.Save(File.Create(hpiFilename), File.Create(hpbFilename));
                    _hpihpb.Dispose();
                }
                else
                {
                    // Create the temp files
                    _hpihpb.Save(File.Create(hpiFilename + ".tmp"), File.Create(hpbFilename + ".tmp"));
                    _hpihpb.Dispose();
                    // Delete the originals
                    _fileInfo.Delete();
                    File.Delete(hpbFilename);
                    // Rename the temporary files
                    File.Move(hpiFilename + ".tmp", hpiFilename);
                    File.Move(hpbFilename + ".tmp", hpbFilename);
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
        public IEnumerable<ArchiveFileInfo> Files => _hpihpb;

        public bool AddFile(ArchiveFileInfo afi)
        {
            try
            {
                _hpihpb.Add(new HpiHpbAfi
                {
                    FileName = afi.FileName,
                    FileData = afi.FileData,
                    State = afi.State
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
