using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace image_ctxb
{
    public sealed class CtxbAdapter : IImageAdapter
    {
        private CTXB _ctxb = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "CTXB";
        public string Description => "CTR TeXture Box";
        public string Extension => "*.ctxb";
        public string About => "This is the CTXB image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "ctxb";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _ctxb = new CTXB(FileInfo.OpenRead());

                var _bmpList = _ctxb.bmps.Select((o, i) => new CTXBBitmapInfo { Bitmap = o, Format = _ctxb.settingsList[i].Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ctxb.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ctxb.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class CTXBBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
