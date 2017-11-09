using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace image_nintendo.BFFNT
{
    public sealed class BffntAdapter : IImageAdapter
    {
        private Cetera.Font.BFFNT _bffnt;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "BFFNT";
        public string Description => "Binary FoNT";
        public string Extension => "*.bffnt";
        public string About => "This is the BFFNT image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "FFNT";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bffnt = new Cetera.Font.BFFNT(File.OpenRead(FileInfo.FullName));
                _bitmaps = _bffnt.bmps.Select((o, i) => new BFFNTBitmapInfo { Bitmap = o, Format = _bffnt._settings[i].Format.FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            /*if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _bffnt.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _bffnt.Save(FileInfo.FullName);*/
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class BFFNTBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
