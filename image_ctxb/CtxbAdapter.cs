using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;
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
                _ctxb = new CTXB(FileInfo.OpenRead());

            _bitmaps = _ctxb.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
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
    }
}
