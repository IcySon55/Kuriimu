using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_ff1_dpk
{
    public class FF1DpkManager : IArchiveManager
    {
        private FF1DPK _dpk;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "Data Package for Final Fantasy 1";
        public string Extension => "*.dpk;*.pck";
        public string About => "This is the DPK/PCK archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                // Vague identification, remove with K2
                br.BaseStream.Position = 4;
                var fileSize = br.ReadInt32();
                return br.BaseStream.Length == fileSize;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _dpk = new FF1DPK(new FileStream(FileInfo.FullName, FileMode.Open, FileAccess.Read));
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
