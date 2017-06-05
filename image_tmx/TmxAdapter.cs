using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tmx
{
    public sealed class TmxAdapter : IImageAdapter
    {
        private TMX _tmx = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "TMX";
        public string Description => "Atlus Texture Matrix";
        public string Extension => "*.tmx";
        public string About => "This is the TMX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 12) return false;
                br.BaseStream.Position = 8;
                return br.ReadString(4) == "TMX0";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _tmx = new TMX(FileInfo.OpenRead());

            _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _tmx.bmp } };
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _tmx.bmp = _bitmaps[0].Bitmap;
            _tmx.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
