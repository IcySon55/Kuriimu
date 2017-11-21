using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_tarc
{
    [FilePluginMetadata(Name = "TARC", Description = "T ARchive", Extension = "*.tarc", Author = "", About = "This is the TARC archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class TbafManager : IArchiveManager
    {
        private TBAF _tbaf = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => true;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                return (br.ReadString(4) == "TBAF");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _tbaf = new TBAF(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _tbaf.Save(FileInfo.Create());
                _tbaf.Close();
            }
            else
            {
                // Create the temp file
                _tbaf.Save(File.Create(FileInfo.FullName + ".tmp"));
                _tbaf.Close();
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
            _tbaf?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _tbaf.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
