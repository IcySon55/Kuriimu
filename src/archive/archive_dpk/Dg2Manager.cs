using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using System.Drawing;
using Komponent.IO;
using System.IO;

namespace archive_dpk.DG2
{
    [FilePluginMetadata(Name = "DPK", Description = "Drakengard PacKage", Extension = "*.bin", Author = "onepiecefreak", About = "This is the DPK archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class Dg2Manager : IArchiveManager
    {
        private DG2 _dpk = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
                if (br.ReadString(4) == "dpk") return Identification.True;

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _dpk = new DG2(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _dpk.Save(FileInfo.Create());
                _dpk.Close();
            }
            else
            {
                // Create the temp file
                _dpk.Save(File.Create(FileInfo.FullName + ".tmp"));
                _dpk.Close();
                // Delete the original
                FileInfo.Delete();
                // Rename the temporary file
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
            _dpk?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _dpk.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
