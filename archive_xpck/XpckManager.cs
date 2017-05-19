using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Cetera.Hash;
using Kuriimu.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_xpck
{
    public class XpckManager : IArchiveManager
    {
        private XPCK _xpck = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "Level 5 eXtractable PaCKage";
        public string Extension => "*.xa;*.xc;*.xf;*.xk;*.xl;*.xr;*.xv";
        public string About => "This is the XPCK archive manager for Karameru.";

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
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                if (br.ReadString(4) == "XPCK") return true;

                br.BaseStream.Position = 0;
                byte[] decomp;
                try { decomp = Level5.Decompress(br.BaseStream); } catch { return false; }
                return new BinaryReaderX(new MemoryStream(decomp)).ReadString(4) == "XPCK";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _xpck = new XPCK(FileInfo.FullName);
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _xpck.Save(FileInfo.Create());
                _xpck.Close();
            }
            else
            {
                // Create the temp file
                _xpck.Save(File.Create(FileInfo.FullName + ".tmp"));
                _xpck.Close();
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
            _xpck?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _xpck.Files;

        public bool AddFile(ArchiveFileInfo afi)
        {
            _xpck.Files.Add(new XPCKFileInfo()
            {
                Entry = new Entry()
                {
                    crc32 = Crc32.Create(Encoding.ASCII.GetBytes(afi.FileName)),
                    ID = (ushort)(_xpck.Files.Count * 8),
                    fileSize = (int)afi.FileData.Length
                },
                FileData = afi.FileData,
                FileName = afi.FileName,
                State = afi.State
            });

            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
