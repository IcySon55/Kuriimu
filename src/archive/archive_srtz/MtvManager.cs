using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;

namespace archive_srtz.MTV
{
    [FilePluginMetadata(Name = "MTV", Description = "Mtv Archives", Extension = "*.bin", Author = "IcySon55,onepiecefreak", About = "This is the MtV archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class MtvManager : IArchiveManager
    {
        private MTV _mtv = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        private static List<string> _supportedFiles = new List<string>
        {
            "AIDData.bin",
            "compdata.bn",
            "HSFC.BIN",
            "MtV_Item.BIN",
            "MtVZknKW.bin",
            "MtVZknPt.bin",
            "MtVZknRt.bin",
            "MtV_BGc.bin",
            "MtV_ProP.BIN",
            "MtV_ProS.BIN",
            "NisVData.bin",
            "stage.bin",
            "veff2dx.bin",
            "KvMData.bin"
        };

        public Identification Identify(Stream stream, string filename)
        {
            if (!File.Exists(filename)) return Identification.False;
            if (_supportedFiles.Contains(new FileInfo(filename).Name)) return Identification.True;

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _mtv = new MTV(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _mtv.Save(FileInfo.Create());
                _mtv.Close();
            }
            else
            {
                // Create the temp file(s)
                _mtv.Save(File.Create(FileInfo.FullName + ".tmp"));
                _mtv.Close();
                // Delete the original(s)
                FileInfo.Delete();
                // Rename the temporary file(s)
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _mtv?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _mtv.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
