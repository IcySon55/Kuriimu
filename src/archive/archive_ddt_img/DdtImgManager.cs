using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_ddt_img
{
    [FilePluginMetadata(Name = "DDT_IMG", Description = "Shin Megami Tensei DDT/IMG Archive", Extension = "*.ddt", Author = "IcySon55", About = "This is the DDT/IMG archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class DdtImgManager : IArchiveManager
    {
        private DDTIMG _format = null;

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
            var imgFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".img");
            if (!File.Exists(filename) || !File.Exists(imgFilename)) return Identification.False;

            try
            {
                var tmp = new DDTIMG(File.OpenRead(filename), File.OpenRead((imgFilename)));
                tmp.Close();
                return Identification.True;
            }
            catch (Exception)
            {
                return Identification.False;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);
            var imgFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".img");

            if (FileInfo.Exists)
                _format = new DDTIMG(FileInfo.OpenRead(), File.OpenRead((imgFilename)));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);
            var imgFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".img");

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _format.Save(FileInfo.Create(), File.Create(imgFilename));
                _format.Close();
            }
            else
            {
                // Create the temp file
                _format.Save(File.Create(FileInfo.FullName + ".tmp"), File.Create(imgFilename + ".tmp"));
                _format.Close();
                // Delete the original
                FileInfo.Delete();
                File.Delete(imgFilename);
                // Rename the temporary file
                File.Move(FileInfo.FullName + ".tmp", FileInfo.FullName);
                File.Move(imgFilename + ".tmp", imgFilename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void New()
        {

        }

        public void Unload()
        {
            _format?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _format.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
