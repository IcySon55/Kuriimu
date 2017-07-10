using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using Kuriimu.Compression;

namespace archive_aatri.aabin
{
    public class AAbinManager : IArchiveManager
    {
        private AABIN _aabin = null;

        #region Properties

        // Information
        public string Name => "AABIN";
        public string Description => "Ace Attorney bin";
        public string Extension => "*.inc";
        public string About => "This is the Ace Attorney bin archive manager for Karameru.";

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
            try
            {
                using (var br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    uint dataOffset = 0;
                    while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        var test = Nintendo.Decompress(br.BaseStream);
                        if (test == null)
                        {
                            var count = br.ReadUInt32();
                            br.BaseStream.Position += (count - 1) * 8;
                            var offset = br.ReadUInt32();
                            var size = br.ReadUInt32();
                            br.BaseStream.Position = dataOffset + offset + size;
                        }
                        else
                        {
                            return true;
                        }

                        while (br.BaseStream.Position % 4 != 0)
                        {
                            br.BaseStream.Position++;
                        }
                        dataOffset = (uint)br.BaseStream.Position;

                        if (br.BaseStream.Position > br.BaseStream.Length) return false;
                    }

                    return true;
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _aabin = new AABIN(File.OpenRead(filename));
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _aabin.Save(File.Create(filename));
                _aabin.Dispose();
            }
            else
            {
                // Create the temp files
                _aabin.Save(File.Create(filename + ".tmp"));
                _aabin.Dispose();
                // Delete the originals
                FileInfo.Delete();
                File.Delete(filename);
                // Rename the temporary files
                File.Move(filename + ".tmp", filename);
            }

            // Reload the new file to make sure everything is in order
            Load(FileInfo.FullName);
        }

        public void Unload()
        {
            _aabin?.Dispose();
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files => _aabin.Files;

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
