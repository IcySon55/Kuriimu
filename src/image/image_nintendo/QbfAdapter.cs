using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_nintendo.QBF
{
    public sealed class QbfAdapter : IImageAdapter
    {
        private QBF _qbf;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "QBF";
        public string Description => "QB Font";
        public string Extension => "*.qbf";
        public string About => "This is the QBF image adapter for Kukkii.";

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
                return br.ReadString(4) == "QBF1";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _qbf = new QBF(FileInfo.FullName);
                _bitmaps = _qbf.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _qbf.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _qbf.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
