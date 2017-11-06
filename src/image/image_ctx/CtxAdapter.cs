using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace image_ctx
{
    public sealed class CtxAdapter : IImageAdapter
    {
        private CTX _ctx = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "CTX";
        public string Description => "CTR TeXture";
        public string Extension => "*.ctx";
        public string About => "This is the CTX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 8) return false;
                return br.ReadString(8) == "CTX 10 ";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _ctx = new CTX(FileInfo.OpenRead());

                var _bmpList = _ctx.bmps.Select(o => new CTXBitmapInfo { Bitmap = o, Format = _ctx.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ctx.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ctx.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class CTXBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
