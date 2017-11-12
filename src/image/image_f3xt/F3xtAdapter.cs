using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;
using System.ComponentModel;
using System.Linq;

namespace image_f3xt
{
    public class F3xtAdapter : IImageAdapter
    {
        private F3XT _f3xt = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "F3XT";
        public string Description => "F3XT Texture";
        public string Extension => "*.tex";
        public string About => "This is the F3XT image adapter for Kukkii.";

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
                return br.ReadString(4) == "F3XT";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _f3xt = new F3XT(FileInfo.OpenRead());

                var _bmpList = new List<BitmapInfo> { new F3XTBitmapInfo { Bitmap = _f3xt.Image, Format = _f3xt.settings.Format.FormatName } };
                var _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);


            _f3xt.Image = _bitmaps[0].Bitmap;
            _f3xt.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class F3XTBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
