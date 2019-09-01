using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract.IO;

namespace archive_l7c
{
    /* Namco Bandai on PSVita (Taiko Drum, Tales of series) */
    public class L7cManager : IArchiveManager
    {
        private L7C _l7c = null;

        #region Settings

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "L7c Archive";
        public string Extension => "*.l7c";
        public string About => "This is the L7c archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;
        public bool CanReplaceFiles => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            if (!File.Exists(filename))
                return false;

            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4)
                    return false;
                return br.ReadUInt32() == 0x4143374c;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            _l7c = new L7C(File.OpenRead(filename));
        }

        public void Save(string filename)
        {
            //if (!string.IsNullOrEmpty(filename))
            //    FileInfo = new FileInfo(filename);

            //var irlstFilename = filename;
            //var irarcFilename = filename.Remove(filename.Length - 5) + "irarc";

            //// Save As...
            //if (!string.IsNullOrWhiteSpace(filename))
            //{
            //    _l7c.Save(File.Create(irlstFilename), File.Create(irarcFilename));
            //    _l7c.Close();
            //}
            //else
            //{
            //    // Create the temp files
            //    _l7c.Save(File.Create(irlstFilename + ".tmp"), File.Create(irarcFilename + ".tmp"));
            //    _l7c.Close();
            //    // Delete the originals
            //    FileInfo.Delete();
            //    File.Delete(irarcFilename);
            //    // Rename the temporary files
            //    File.Move(irlstFilename + ".tmp", irlstFilename);
            //    File.Move(irarcFilename + ".tmp", irarcFilename);
            //}

            //// Reload the new file to make sure everything is in order
            //Load(FileInfo.FullName);
        }

        public void Unload()
        {
            //_l7c?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _l7c.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
