using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace image_aif
{
    [Export(typeof(IImageAdapter))]
    public sealed class AifAdapter : IImageAdapter
    {
        private AIF _aif = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "AIF";
        public string Description => "Whatever AIF means";
        public string Extension => "*.aif";
        public string About => "This is the AIF image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadString(4) == " FIA";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _aif = new AIF(FileInfo.OpenRead());

                var _bmpList = _aif.bmps.Select(o => new AIFBitmapInfo { Bitmap = o, Format = _aif.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _aif.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _aif.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class AIFBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
