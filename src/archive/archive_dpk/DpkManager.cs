using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace archive_dpk.DPK4
{
    [FilePluginMetadata(Name = "CPK4", Description = "Data Package v4", Extension = "*.dpk", Author = "IcySon55", About = "This is the DPK4 archive manager for Karameru.")]
    [Export(typeof(IArchiveManager))]
    public class DpkManager : IArchiveManager
    {
        //private DPK4 _dpk4 = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanAddFiles => false;
        public bool CanRenameFiles => false;
        public bool CanReplaceFiles => false;
        public bool CanDeleteFiles => false;
        public bool CanIdentify => true;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4) return false;
                var magic = br.ReadString(4);
                return (magic == "DPK4");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            //if (FileInfo.Exists)
            //	_dpk4 = new DPK4(new FileStream(FileInfo.FullName, FileMode.Open, FileAccess.Read));
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            //_dpk4.Save(new FileStream(FileInfo.FullName, FileMode.Create, FileAccess.Write));
        }

        public void New()
        {

        }

        public void Unload()
        {
            // TODO: Implement closing open handles here
        }

        // Files
        public IEnumerable<ArchiveFileInfo> Files
        {
            get
            {
                ArchiveFileInfo NewFile(string filename) => new ArchiveFileInfo { FileName = filename, FileData = new MemoryStream(new byte[] { 0x64, 0x64, 0x64, 0x64 }), State = ArchiveFileState.Archived };
                yield return NewFile("archive_file.ctpk");
                yield return NewFile("image_file.bclim");
                yield return NewFile("text_file.msbt");
                yield return NewFile("dir1/subfile1.ext");
                yield return NewFile("dir1/subfile2.ext");
                yield return NewFile("dir1/subdir2/filez.xi");
                yield return NewFile("dir2/zilla.ext");
                yield return NewFile("dir2/somefile.ext");
                yield return NewFile("/scr/start.fsb");
                yield return NewFile("/scr/start.dat");
            }
        }

        public bool AddFile(ArchiveFileInfo afi) => false;

        public bool DeleteFile(ArchiveFileInfo afi) => false;

        // Features
        public bool ShowProperties(Icon icon) => false;
    }
}
