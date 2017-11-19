using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

namespace image_jtex
{
    [FilePluginMetadata(Name = "JTEX", Description = "J Texture", Extension = "*.jtex",
        Author = "onepiecefreak", About = "This is the JTEX image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    class JtexAdapter : IImageAdapter
    {
        private JTEX _jtex;
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
                if (br.ReadString(4) == "jIMG") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

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
