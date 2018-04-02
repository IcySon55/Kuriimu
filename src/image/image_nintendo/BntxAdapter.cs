using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.IO;
using Kontract.Interface;

namespace image_nintendo.BNTX
{
    public class BntxAdapter : IImageAdapter
    {
        private BNTX _bntx = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "BNTX";
        public string Description => "Binary NX Image";
        public string Extension => "*.bntx;";
        public string About => "This is the BNTX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadString(4) == "BNTX";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bntx = new BNTX(FileInfo.OpenRead());
                _bitmaps = new List<BitmapInfo>();
                foreach (var e in _bntx.Images)
                    _bitmaps.Add(new BntxBitmapInfo
                    {
                        Bitmap = e.bmp,
                        Format = e.format,
                        Name = e.name
                    });
                //_bitmaps = new List<BitmapInfo> { new BntxBitmapInfo { Bitmap = _bntx.Image, Format = _bntx.Settings.Format.FormatName } };
            }
        }

        public void Save(string filename = "")
        {
            /*if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            var info = _bitmaps[0] as BntxBitmapInfo;
            _bntx.Image = info.Bitmap;
            _bntx.Save(FileInfo.Create());*/
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }

    public sealed class BntxBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public string Format { get; set; }
        [Category("Properties")]
        [ReadOnly(true)]
        public string Name { get; set; }
    }
}