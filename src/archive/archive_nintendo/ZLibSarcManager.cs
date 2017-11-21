using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;
using System.Text;
using Komponent.Compression;
using Komponent.CTR.Hash;

namespace archive_nintendo.SARC
{
    [FilePluginMetadata(Name = "ZLib-SARC", Description = "ZLib-Compressed NW4C Sorted ARChive", Extension = "*.zlib", Author = "", About = "This is the ZLib-Compressed SARC archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class ZLibSarcManager : IArchiveManager
    {
        private SARC _sarc = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => true;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => true;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            return false;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            using (var br = new BinaryReaderX(FileInfo.OpenRead()))
            {
                br.ReadBytes(4);
                _sarc = new SARC(new MemoryStream(new ZLib().Decompress(new MemoryStream(br.ReadBytes((int)FileInfo.Length - 4)), 0)));
            }
        }

        public void Save(string filename = "")
        {
            if (!string.IsNullOrEmpty(filename))
                FileInfo = new FileInfo(filename);

            // Save As...
            if (!string.IsNullOrEmpty(filename))
            {
                var ms = new MemoryStream();
                _sarc.Save(ms, true);
                _sarc.Close();
                using (var bw = new BinaryWriterX(FileInfo.Create()))
                {
                    bw.Write(BitConverter.GetBytes((int)ms.Length).Reverse().ToArray());
                    ms.Position = 0;
                    var comp = new ZLib();
                    comp.SetMethod(0);
                    bw.Write(comp.Compress(ms));
                }
            }
            else
            {
                // Create the temp file
                var ms = new MemoryStream();
                _sarc.Save(ms, true);
                _sarc.Close();
                using (var bw = new BinaryWriterX(File.Create(FileInfo.FullName + ".tmp")))
                {
                    bw.Write(BitConverter.GetBytes((int)ms.Length).Reverse().ToArray());
                    ms.Position = 0;
                    var comp = new ZLib();
                    comp.SetMethod(0);
                    bw.Write(comp.Compress(ms));
                }
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
            _sarc.Close();
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
                hash = GetInt(new SimpleHash3DS().Create(Encoding.ASCII.GetBytes(afi.FileName), 0)) % _sarc.hashMultiplier
            });
            return true;
        }

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
