using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_hpi_hpb
{
    public class HpiHpbManager : IArchiveManager
    {
        private HPIHPB _hpihpb = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "Atlus Archive (Etrian Odyssey Series)";
        public string Extension => "*.hpi";
        public string About => "This is the HPI/HPB archive manager for Karameru.";

        // Feature Support
        public bool ArchiveHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanReplaceFiles => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var hpiFilename = filename;
            var hpbFilename = filename.Remove(filename.Length - 1) + "B";

            if (!File.Exists(hpiFilename) || !File.Exists(hpbFilename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(hpiFilename)))
            {
                return br.BaseStream.Length >= 4 && br.ReadString(4) == "HPIH";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            var hpiFilename = filename;
            var hpbFilename = filename.Remove(filename.Length - 1) + "B";

            _hpihpb = new HPIHPB(File.OpenRead(hpiFilename), File.OpenRead(hpbFilename));
        }

        public void Save(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            var hpiFilename = FileInfo.FullName;
            var hpbFilename = FileInfo.FullName.Remove(FileInfo.FullName.Length - 1) + "B";

            // Save As...
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _hpihpb.Save(File.Create(hpiFilename), File.Create(hpbFilename));
                _hpihpb.Close();
            }
            else
            {
                // Create the temp files
                _hpihpb.Save(File.Create(hpiFilename + ".tmp"), File.Create(hpbFilename + ".tmp"));
                _hpihpb.Close();
                // Delete the originals
                FileInfo.Delete();
                File.Delete(hpbFilename);
                // Rename the temporary files
                File.Move(hpiFilename + ".tmp", hpiFilename);
                File.Move(hpbFilename + ".tmp", hpbFilename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _hpihpb?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _hpihpb.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            _hpihpb.Files.Add(new HpiHpbAfi
            {
                FileName = afi.FileName,
                FileData = afi.FileData,
                State = afi.State
            });

            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
