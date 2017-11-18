using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Komponent.IO;
using Kontract.Interface;

namespace archive_nintendo.VIW
{
    public class ViwFileInfo : ArchiveFileInfo
    {
        public Import imports;

        public override Stream FileData => State != ArchiveFileState.Archived ? base.FileData : new MemoryStream(imports.nintendo.Decompress(base.FileData, 0));
        public override long? FileSize => base.FileData.Length;

        public int Write(Stream output)
        {
            var compressedSize = (int)base.FileData.Length;

            using (var bw = new BinaryWriterX(output, true))
            {
                if (State == ArchiveFileState.Archived)
                    base.FileData.CopyTo(bw.BaseStream);
                else
                {
                    imports.nintendo.SetMethod(0x10);
                    var bytes = imports.nintendo.Compress(base.FileData);
                    compressedSize = bytes.Length;
                    bw.Write(bytes);
                }
            }

            return compressedSize;
        }
    }

    public class Import
    {
        [Import("Nintendo")]
        public ICompressionCollection nintendo;

        public Import()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfHeader
    {
        public int FileCount;
        public int MetaEntryCount;
        public int Table0Offset;
        public int Table1Offset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfEntry
    {
        public int Offset;
        public int CompressedSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class InfMetaEntry
    {
        public short Unk1;
        public short Unk2;
        public short Unk3;
        public short Unk4;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ViwEntry
    {
        public int ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x14)]
        public string Name;
    }
}
