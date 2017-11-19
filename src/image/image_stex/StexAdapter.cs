using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using System.ComponentModel;
using Komponent.IO;

namespace image_stex
{
    [FilePluginMetadata(Name = "STEX", Description = "Atlus Simple Texture", Extension = "*.stex",
        Author = "onepiecefreak", About = "This is the STEX image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class StexAdapter : IImageAdapter
    {
        private STEX _stex = null;
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
                if (br.ReadString(4) == "STEX") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _stex = new STEX(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new StexBitmapInfo { Bitmap = _stex.bmp, Format = _stex.settings.Format.FormatName } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _stex.bmp = _bitmaps[0].Bitmap;
            _stex.Save(FileInfo.FullName, _stex.bmp);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class StexBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
