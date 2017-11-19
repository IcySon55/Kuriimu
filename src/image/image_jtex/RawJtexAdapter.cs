using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Windows.Forms;

namespace image_rawJtex
{
    [FilePluginMetadata(Name = "RawJTEX", Description = "J Texture without header", Extension = "*.jtex",
        Author = "onepiecefreak", About = "This is the RawJTEX image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class RawJtexAdapter : IImageAdapter
    {
        private RawJTEX _rawjtex;
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
                _rawjtex = new RawJTEX(FileInfo.OpenRead());

                var _bmpList = new List<BitmapInfo> { new RawJTEXBitmapInfo { Bitmap = _rawjtex.Image, Format = _rawjtex.settings.Format.FormatName } };
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _rawjtex.Image = _bitmaps[0].Bitmap;
            _rawjtex.Save(FileInfo.Create());
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class RawJTEXBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
