using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;
using System.Linq;

/*Mystery Tales - Time Travel NDS*/
namespace image_ctgd
{
    [FilePluginMetadata(Name = "CTGD", Description = "Whatever CTGD means", Extension = "*.ctgd",
        Author = "onepiecefreak", About = "This is the CTGD image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class CtgdAdapter : IImageAdapter
    {
        private CTGD _ctgd = null;
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
                if (br.BaseStream.Length < 8) return Identification.True;
                br.BaseStream.Position = 4;
                if (br.ReadString(4) == "nns_") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _ctgd = new CTGD(FileInfo.OpenRead());

                var _bmpList = _ctgd.bmps.Select(o => new CTGDBitmapInfo { Bitmap = o, Format = _ctgd.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ctgd.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ctgd.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class CTGDBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
