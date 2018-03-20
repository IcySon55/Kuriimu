using System.IO;
using System.Runtime.InteropServices;
using Kontract.Compression;
using Kontract.Interface;

namespace archive_3ds_lz
{
    // TODO: Define your format's logical blocks here to keep the main file clean and straight forward
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class archive_3dslzFileEntry
    {
        public int Size = 0;
    }

    public class archive_3dslzFileInfo : ArchiveFileInfo
    {
        public archive_3dslzFileEntry Entry { get; set; }

        public override Stream FileData
        {
            get => State == ArchiveFileState.Archived ? new MemoryStream(Nintendo.Decompress(_fileData)) : _fileData;
            set => _fileData = value;
        }

        public Stream CompressedFileData => _fileData;

        public override long? FileSize => Entry?.Size;
    }
}
