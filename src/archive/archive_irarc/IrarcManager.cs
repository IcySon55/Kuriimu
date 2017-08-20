using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace archive_irarc
{
    public class IrarcManager : IArchiveManager
    {
        private IRARC _irarc = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "IR ARChive";
        public string Extension => "*.irlst";
        public string About => "This is the IRARC archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;
        public bool CanReplaceFiles => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var irlstFilename = filename;
            var irarcFilename = filename.Remove(filename.Length - 5) + "irarc";

            if (!File.Exists(irlstFilename) || !File.Exists(irarcFilename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(irlstFilename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadUInt32() * 0x10 + 4 == br.BaseStream.Length;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            var irlstFilename = filename;
            var irarcFilename = filename.Remove(filename.Length - 5) + "irarc";

            _irarc = new IRARC(File.OpenRead(irlstFilename), File.OpenRead(irarcFilename));
        }

        public void Save(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            var irlstFilename = filename;
            var irarcFilename = filename.Remove(filename.Length - 5) + "irarc";

            // Save As...
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _irarc.Save(File.Create(irlstFilename), File.Create(irarcFilename));
                _irarc.Close();
            }
            else
            {
                // Create the temp files
                _irarc.Save(File.Create(irlstFilename + ".tmp"), File.Create(irarcFilename + ".tmp"));
                _irarc.Close();
                // Delete the originals
                FileInfo.Delete();
                File.Delete(irarcFilename);
                // Rename the temporary files
                File.Move(irlstFilename + ".tmp", irlstFilename);
                File.Move(irarcFilename + ".tmp", irarcFilename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _irarc?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _irarc.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            return false;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
