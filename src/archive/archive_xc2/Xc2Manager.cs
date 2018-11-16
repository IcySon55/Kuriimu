using Kontract.Interface;
using Kontract.IO;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace archive_xc2
{
    public class archive_xc2Manager : IArchiveManager
    {
        private XC2 _xc2 = null;

        #region Properties

        // Information
        public string Name => "XC2 ARD/ARH";
        public string Description => "XC2 main archive";
        public string Extension => "*.arh";
        public string About => "This is the XC2 main archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return (br.ReadString(4) == "arh1");
            }
        }

        public void Load(string filename)
        {
            //var ardFile = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) + ".ard";
            //if (!File.Exists(ardFile))
            //    throw new FileNotFoundException(ardFile);

            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _xc2 = new XC2(FileInfo.OpenRead(), null/*File.OpenRead(ardFile)*/);
        }

        public void Save(string filename = "")
        {
            //if (!string.IsNullOrEmpty(filename))
            //    FileInfo = new FileInfo(filename);

            //// Save As...
            //if (!string.IsNullOrEmpty(filename))
            //{
            //    _xc2.Save(FileInfo.Create());
            //    _xc2.Close();
            //}
            //else
            //{
            //    // Create the temp file
            //    _xc2.Save(File.Create(FileInfo.FullName + ".tmp"));
            //    _xc2.Close();
            //    // Delete the original
            //    FileInfo.Delete();
            //    // Rename the temporary file
            //    File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
            //}

            //// Reload the new file to make sure everything is in order
            //Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _xc2?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _xc2.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
