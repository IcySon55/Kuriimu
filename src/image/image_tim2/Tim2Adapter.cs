using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace image_tim2
{
    [FilePluginMetadata(Name = "TIM2", Description = "Default PS2 Image Format v2", Extension = "*.tm2",
        Author = "onepiecefreak,IcySon55", About = "This is the TIM2 image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class Tim2Adapter : IImageAdapter
    {
        private TIM2 _tim2 = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        Bitmap tmp;

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.ReadString(4) == "TIM2") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tim2 = new TIM2(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new Tim2BitmapInfo { Bitmap = _tim2.bmp, Format = _tim2.settings.Format.FormatName } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _tim2.bmp = _bitmaps[0].Bitmap;
            //_tim2.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class Tim2BitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
