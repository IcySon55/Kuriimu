using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

namespace image_aif
{
    [FilePluginMetadata(Name = "AIF", Description = "Whatever AIF means", Extension = "*.aif", Author = "onepiecefreak",
        About = "This is the AIF image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class AifAdapter : IImageAdapter
    {
        private AIF _aif = null;
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
                if (br.ReadString(4) == " FIA") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

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
