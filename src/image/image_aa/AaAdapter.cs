using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace image_aa
{
    [FilePluginMetadata(Name = "Ace Attorney DS", Description = "Ace Attorney Images from the DS", Extension = "*.bin", Author = "onepiecefreak",
        About = "This is the AA image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class CtxbAdapter : IImageAdapter
    {
        private AA _aa = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            return Identification.Raw;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _aa = new AA(FileInfo.OpenRead());

                var _bmpList = _aa.bmps.Select(o => new AABitmapInfo { Bitmap = o, Format = _aa.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _aa.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _aa.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class AABitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
