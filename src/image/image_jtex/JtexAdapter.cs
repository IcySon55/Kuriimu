using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace image_jtex
{
    class JtexAdapter : IImageAdapter
    {
        private JTEX _jtex;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "JTEX";
        public string Description => "J Texture";
        public string Extension => "*.jtex";
        public string About => "This is the JTEX image adapter for Kukkii.";

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
                return br.ReadString(4) == "jIMG";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _jtex = new JTEX(FileInfo.OpenRead());

                var _bmpList = _jtex.bmps.Select(o => new JTEXBitmapInfo { Bitmap = o, Format = _jtex.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _jtex.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _jtex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class JTEXBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
