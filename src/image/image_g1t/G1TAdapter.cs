using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;
using System.ComponentModel;
using System.Linq;

namespace image_g1t
{
    public class G1tAdapter : IImageAdapter
    {
        private G1T _g1t = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "G1T";
        public string Description => "G1T Texture";
        public string Extension => "*.g1t";
        public string About => "This is the G1T image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                var magic1 = br.ReadString(4);
                var magic2 = br.ReadString(4);
                return (magic1 == "GT1G" || magic1 == "G1TG") && (magic2 == "0600" || magic2 == "0500" || magic2 == "1600");
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                var platform = Platform.PS;
                if (Path.GetFileName(filename).ToLower().Contains("vita")) platform = Platform.Vita;
                if (Path.GetFileName(filename).ToLower().Contains("3ds")) platform = Platform.N3DS;
                _g1t = new G1T(FileInfo.OpenRead(), platform);

                _bitmaps = _g1t.bmps.Select((b, i) => new G1TBitmapInfo { Bitmap = b, Format = _g1t.settings[i].Format.FormatName, Platform = platform }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);


            _g1t.bmps = _bitmaps.Select(b => b.Bitmap).ToList();
            _g1t.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class G1TBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
            [Category("Properties")]
            [ReadOnly(true)]
            public Platform Platform { get; set; }
        }
    }
}
