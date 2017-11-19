using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

namespace image_ctxb
{
    [FilePluginMetadata(Name = "CTXB", Description = "CTR TeXture Box", Extension = "*.ctxb",
        Author = "onepiecefreak", About = "This is the CTXB image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class CtxbAdapter : IImageAdapter
    {
        private CTXB _ctxb = null;
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
                if (br.ReadString(4) == "ctxb") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _ctxb = new CTXB(FileInfo.OpenRead());

                var _bmpList = _ctxb.bmps.Select((o, i) => new CTXBBitmapInfo { Bitmap = o, Format = _ctxb.settingsList[i].Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ctxb.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ctxb.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class CTXBBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
