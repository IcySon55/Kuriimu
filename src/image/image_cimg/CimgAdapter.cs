using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;
using Kontract;

namespace image_cimg
{
    public sealed class CimgAdapter : IImageAdapter
    {
        private CIMG _cimg = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "CIMG";
        public string Description => "Compressed Image";
        public string Extension => "*.cimg;*.limg";
        public string About => "This is the CIMG image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                var mag1 = br.ReadString(4);
                br.BaseStream.Position = 5;
                return mag1 == "LIMG" || br.ReadString(4) == "LIMG";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _cimg = new CIMG(FileInfo.OpenRead());

                var _bmpList = _cimg.bmps.Select(o => new CimgBitmapInfo { Bitmap = o, Format = _cimg.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _cimg.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _cimg.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class CimgBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
