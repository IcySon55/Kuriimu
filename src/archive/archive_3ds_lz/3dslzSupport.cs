using Kontract.Compression;
using Kontract.Interface;
using System.IO;

namespace archive_3ds_lz
{
    public class _3dslzFileInfo : ArchiveFileInfo
    {
        public int Size { get; set; } = 0;

        public override Stream FileData
        {
            get => State == ArchiveFileState.Archived ? new MemoryStream(Nintendo.Decompress(_fileData)) : _fileData;
            set => _fileData = value;
        }

        public Stream CompressedFileData => _fileData;

        public override long? FileSize => Size;
    }
}
