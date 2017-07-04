using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_jtex
{
    class JtexAdapter : IImageAdapter
    {
        private JTEX _jtex;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "JTEX";
        public string Description => "J Texture";
        public string Extension => "*.jtex";
        public string About => "This is the JTEX image adapter for Kukkii.";

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
                return br.ReadString(4) == "jIMG";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _jtex = new JTEX(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _jtex.Image } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _jtex.Image = _bitmaps[0].Bitmap;
            _jtex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
