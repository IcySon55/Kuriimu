using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Text;
using System.Linq;

namespace archive_nintendo.SARC
{
    [FilePluginMetadata(Name = "SARC", Description = "NW4C Sorted ARChive", Extension = "*.sarc;*.arc", Author = "onepiecefreak,Neobeo,IcySon55", About = "This is the SARC archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class SarcManager : IArchiveManager
    {
        private SARC _sarc = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => true;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4 || br.ReadString(4) != "SARC") return Identification.False;
                br.ReadInt16();
                var ind = br.ReadUInt16();
                if (ind == 0xfeff || ind == 0xfffe) return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _sarc = new SARC(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _sarc.Save(FileInfo.Create());
                _sarc.Close();
            }
            else
            {
                // Create the temp file
                _sarc.Save(File.Create(FileInfo.FullName + ".tmp"));
                _sarc.Close();
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
            _sarc?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _sarc.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            uint GetInt(byte[] ba) => ba.Aggregate(0u, (i, b) => (i << 8) | b);
            _sarc.Files.Add(new SarcArchiveFileInfo
            {
                FileData = afi.FileData,
                FileName = afi.FileName,
                State = afi.State,
                hash = GetInt(_sarc.imports.simplehash.Create(Encoding.ASCII.GetBytes(afi.FileName), 0)) % _sarc.hashMultiplier
            });
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
