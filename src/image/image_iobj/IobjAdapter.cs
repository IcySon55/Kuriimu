using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace image_iobj
{
    public sealed class IobjAdapter : IImageAdapter
    {
        private IOBJ _iobj = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "IOBJ";
        public string Description => "Image OBJect";
        public string Extension => "*.bin;*.iobj";
        public string About => "This is the IOBJ image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadString(4) == "IOBJ";
            }
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

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
