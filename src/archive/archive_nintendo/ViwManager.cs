using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;

namespace archive_nintendo.VIW
{
    [FilePluginMetadata(Name = "VIW", Description = "VIW Archive", Extension = "*.viw", Author = "IcySon55", About = "This is the VIW archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class ViwManager : IArchiveManager
    {
        private VIW _viw;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            return false;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            var file2 = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".inf");
            var file3 = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));

            if (FileInfo.Exists && File.Exists(file2) && File.Exists(file3))
                _viw = new VIW(FileInfo.OpenRead(), File.OpenRead(file2), File.OpenRead(file3));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);
            var file2 = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".inf");
            var file3 = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName));

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _viw.Save(FileInfo.Create(), File.Create(file2), File.Create(file3));
                _viw.Close();
            }
            else
            {
                // Create the temp file
                _viw.Save(File.Create(FileInfo.FullName + ".tmp"), File.Create(file2 + ".tmp"), File.Create(file3 + ".tmp"));
                _viw.Close();
                // Delete the original
                FileInfo.Delete();
                File.Delete(file2);
                File.Delete(file3);
                // Rename the temporary file
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
                File.Move(file2 + ".tmp", file2);
                File.Move(file3 + ".tmp", file3);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _viw?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _viw.Files;
        public bool AddFile(ArchiveFileInfo afi) => false;
        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}

