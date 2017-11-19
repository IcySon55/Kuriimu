using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_comp
{
    [FilePluginMetadata(Name = "COMP", Description = "Generic image format", Extension = "*.comp",
        Author = "onepiecefreak", About = "This is the COMP image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class CompAdapter : IImageAdapter
    {
        private COMP _comp = null;
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
                _comp = new COMP(FileInfo.OpenRead());

                var _bmpList = _comp.bmps.Select(o => new COMPBitmapInfo { Bitmap = o, Format = _comp.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _comp.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _comp.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class COMPBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
