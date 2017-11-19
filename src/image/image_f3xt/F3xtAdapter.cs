using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.ComponentModel;
using System.Linq;

namespace image_f3xt
{
    [FilePluginMetadata(Name = "F3XT", Description = "F3XT Texture", Extension = "*.tex",
        Author = "onepiecefreak", About = "This is the F3XT image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class F3xtAdapter : IImageAdapter
    {
        private F3XT _f3xt = null;
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
                if (br.ReadString(4) == "F3XT") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

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
