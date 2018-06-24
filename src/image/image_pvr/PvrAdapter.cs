using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace image_pvr
{
    public sealed class PvrAdapter : IImageAdapter
    {
        private PVR _pvr = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "PVR";
        public string Description => "PvrTexTool PVR Format";
        public string Extension => "*.pvr";
        public string About => "This is the PvrTexTool image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                var magic = br.ReadString(4);
                return magic == "PVR\x3" || magic == "\x3RVP";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _pvr = new PVR(FileInfo.OpenRead());

                var _bmpList = _pvr.bmps.Select((o, i) => new PicaRg4BitmapInfo { Bitmap = o, Format = _pvr.settings[i].Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _pvr.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _pvr.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class PicaRg4BitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
