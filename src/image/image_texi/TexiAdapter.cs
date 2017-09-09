using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace image_texi
{
    public sealed class TexiAdapter : IImageAdapter
    {
        private TEXI _texi = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "TEXI";
        public string Description => "NLP Texture";
        public string Extension => "*.tex";
        public string About => "This is the TEXI image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var texiName = filename + "i";
            if (!File.Exists(texiName)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(texiName)))
            {
                br.BaseStream.Position = 0x18;
                return br.ReadString(4) == "SERI";
            }
        }

        public void Load(string filename)
        {
            var texiName = filename + "i";
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _texi = new TEXI(FileInfo.OpenRead(), File.OpenRead(texiName));

                _bitmaps = _texi.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _texi.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _texi.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
