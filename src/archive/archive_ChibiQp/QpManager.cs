using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;
using Kontract.Interface;
using System.Drawing;

namespace archive_ChibiQp
{
    public class QpManager : IArchiveManager
    {
        private QP _qp = null;

        #region Properties

        // Information
        public string Name => "QP";
        public string Description => "qb.bin from Chibi Robo";
        public string Extension => "*.bin";
        public string About => "This is the QP archive manager for Karameru.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename),ByteOrder.BigEndian))
            {
                br.BaseStream.Position = 0x10;
                return br.ReadUInt32() == 0xcccccccc && br.ReadUInt32() == 0xcccccccc && br.ReadUInt32() == 0xcccccccc && br.ReadUInt32() == 0xcccccccc;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _qp = new QP(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                _qp.Save(FileInfo.Create());
                _qp.Close();
            }
            else
            {
                // Create the temp file
                _qp.Save(File.Create(FileInfo.FullName + ".tmp"));
                _qp.Close();
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
            _qp?.Close();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _qp.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
