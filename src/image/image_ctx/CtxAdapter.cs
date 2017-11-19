using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

namespace image_ctx
{
    [FilePluginMetadata(Name = "CTX", Description = "CTR TeXture", Extension = "*.ctx",
        Author = "onepiecefreak", About = "This is the CTX image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class CtxAdapter : IImageAdapter
    {
        private CTX _ctx = null;
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
                if (br.BaseStream.Length < 8) return Identification.False;
                if (br.ReadString(8) == "CTX 10 ") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _ctx = new CTX(FileInfo.OpenRead());

                var _bmpList = _ctx.bmps.Select(o => new CTXBitmapInfo { Bitmap = o, Format = _ctx.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ctx.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ctx.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class CTXBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
