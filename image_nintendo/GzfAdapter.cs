using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_nintendo.GZF
{
    public sealed class GzfAdapter : IImageAdapter
    {
        private GZF _gzf;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "GZF";
        public string Description => "GZ Font";
        public string Extension => "*.gzf";
        public string About => "This is the GZF image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "GZFX";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _gzf = new GZF(FileInfo.FullName);
                _bitmaps = _gzf.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _gzf.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _gzf.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
