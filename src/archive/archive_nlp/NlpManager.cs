using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;

namespace archive_nlp
{
    public class NlpManager : IArchiveManager
    {
        private NLP _nlp = null;

        #region Properties

        // Information
        public string Name => "NLP";
        public string Description => "New Love Plus Archive";
        public string Extension => "*.bin";
        public string About => "This is the NLP archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            return filename.Contains("img.bin");
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _nlp = new NLP(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _nlp.Save(FileInfo.Create());
                _nlp.Close();
            }
            else
            {
                // Create the temp file
                _nlp.Save(File.Create(FileInfo.FullName + ".tmp"));
                _nlp.Close();
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
            _nlp?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _nlp.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
