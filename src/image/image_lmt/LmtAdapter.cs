using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

namespace image_lmt
{
    [FilePluginMetadata(Name = "LMT", Description = "Whatever LMT means", Extension = "*.lmt",
        Author = "onepiecefreak", About = "This is the LMT image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class LmtAdapter : IImageAdapter
    {
        private LMT _lmt = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                var entryCount = br.ReadUInt32();
                var infoSize = entryCount * 0x14 + 8;
                if (br.BaseStream.Length < infoSize + 8) return Identification.False;
                br.BaseStream.Position = infoSize + 5;
                if (br.ReadString(3) == "PNG") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _lmt = new LMT(FileInfo.OpenRead());

                _bitmaps = _lmt.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _lmt.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _lmt.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
