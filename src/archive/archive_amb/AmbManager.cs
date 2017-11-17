using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_amb
{
    [FilePluginMetadata(Name = "AMB", Description = "Whatever AMB means", Extension = "*.amb", Author = "onepiecefreak", About = "This is the AMB archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class AmbManager : IArchiveManager
    {
        private AMB _amb = null;

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
            using (var br = new BinaryReaderX(stream,true))
            {
                if (br.BaseStream.Length < 4) return Identification.False;
                var magic = br.ReadString(4);
                if (magic == "#AMB") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _amb = new AMB(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _amb.Save(FileInfo.Create());
                _amb.Close();
            }
            else
            {
                // Create the temp file
                _amb.Save(File.Create(FileInfo.FullName + ".tmp"));
                _amb.Close();
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
            _amb?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _amb.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
