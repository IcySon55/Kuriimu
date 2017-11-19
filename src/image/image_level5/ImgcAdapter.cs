using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace image_level5.imgc
{
    [FilePluginMetadata(Name = "IMGC", Description = "Level 5 Compressed Image", Extension = "*.xi",
        Author = "onepiecefreak", About = "This is the IMGC image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class ImgcAdapter : IImageAdapter
    {
        private IMGC _imgc = null;
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
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4) return Identification.False;
                if (br.ReadString(4) == "IMGC") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _imgc = new IMGC(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new ImgcBitmapInfo { Bitmap = _imgc.Image, Format = _imgc.settings.Format.FormatName } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _imgc.Image = _bitmaps[0].Bitmap;
            _imgc.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class ImgcBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
