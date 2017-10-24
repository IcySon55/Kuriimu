using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace image_lmt
{
    public sealed class LmtAdapter : IImageAdapter
    {
        private LMT _lmt = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "LMT";
        public string Description => "Whatever LMT means";
        public string Extension => "*.lmt";
        public string About => "This is the LMT image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                var entryCount = br.ReadUInt32();
                var infoSize = entryCount * 0x14 + 8;
                if (br.BaseStream.Length < infoSize + 8) return false;
                br.BaseStream.Position = infoSize + 5;
                return br.ReadString(3) == "PNG";
            }
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

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
