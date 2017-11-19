using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_iobj
{
    [FilePluginMetadata(Name = "IOBJ", Description = "Image OBJect", Extension = "*.bin;*.iobj",
        Author = "onepiecefreak", About = "This is the IOBJ image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class IobjAdapter : IImageAdapter
    {
        private IOBJ _iobj = null;
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
                if (br.ReadString(4) == "IOBJ") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _iobj = new IOBJ(FileInfo.OpenRead());

                _bitmaps = _iobj.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _iobj.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _iobj.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
