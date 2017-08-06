using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Linq;

namespace image_aif
{
    public sealed class AifAdapter : IImageAdapter
    {
        private AIF _aif = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "AIF";
        public string Description => "Whatever AIF means";
        public string Extension => "*.aif";
        public string About => "This is the AIF image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadString(4) == " FIA";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _aif = new AIF(FileInfo.OpenRead());

                _bitmaps = _aif.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _aif.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _aif.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
