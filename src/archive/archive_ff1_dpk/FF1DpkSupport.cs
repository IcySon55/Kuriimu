using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;

namespace archive_ff1_dpk
{
    class FF1DpkArchiveFileInfo : ArchiveFileInfo
    {
        private bool _isDataSet;

        public FileEntry Entry { get; set; }

        public FF1DpkArchiveFileInfo(Stream fileData, FileEntry entry)
        {
            base.FileData = fileData;
            Entry = entry;
        }

        public override Stream FileData
        {
            get
            {
                if (_isDataSet || Entry.compressedSize == Entry.uncompressedSize)
                {
                    return base.FileData;
                }
                else
                {
                    var ms = new MemoryStream();
                    new Wp16().Decompress(base.FileData, ms);
                    return ms;
                }
            }
            set
            {
                _isDataSet = true;
                base.FileData = value;
            }
        }

        public override long? FileSize
        {
            get
            {
                if (!_isDataSet)
                    return Entry.uncompressedSize;
                else
                    return base.FileData.Length;
            }
        }

        public void WriteFile(Stream output)
        {
            if (!_isDataSet || Entry.compressedSize == Entry.uncompressedSize)
            {
                base.FileData.Position = 0;
                base.FileData.CopyTo(output);
            }
            else
            {
                var ms = new MemoryStream();
                new Wp16().Compress(base.FileData, ms);
                ms.Position = 0;
                ms.CopyTo(output);
            }
        }
    }

    class FileEntry
    {
        public string fileName;

        public short nameSum;
        public int fileOffset;
        public int compressedSize;
        public int uncompressedSize;
    }
}
