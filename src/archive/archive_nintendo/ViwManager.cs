using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;

namespace archive_nintendo.VIW
{
    public class ViwManager : IArchiveManager
    {
        private VIW _viw;

        #region Properties

        // Information
        public string Name => "VIW";
        public string Description => "VIW Archive";
        public string Extension => "*.viw;*.inf";
        public string About => "This is the VIW archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            try
            {
                FileInfo = new FileInfo(filename);
                var file2 = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".viw");
                var file3 = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));

                if (Path.GetExtension(filename) != ".inf")
                    FileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".inf"));

                if (FileInfo.Exists && File.Exists(file2) && File.Exists(file3))
                    _viw = new VIW(FileInfo.OpenRead(), File.OpenRead(file2), File.OpenRead(file3));
                _viw.Close();
                return true;
            }
            catch (Exception)
            {
                if (_viw != null) _viw.Close();
                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            var file2 = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".viw");
            var file3 = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));

            if (Path.GetExtension(filename) != ".inf")
                FileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".inf"));

            if (FileInfo.Exists && File.Exists(file2) && File.Exists(file3))
                _viw = new VIW(FileInfo.OpenRead(), File.OpenRead(file2), File.OpenRead(file3));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);
            var file2 = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".viw");
            var file3 = Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName));

            if (Path.GetExtension(FileInfo.Name) != ".inf")
                FileInfo = new FileInfo(Path.Combine(Path.GetDirectoryName(FileInfo.FullName), Path.GetFileNameWithoutExtension(FileInfo.FullName) + ".inf"));

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

