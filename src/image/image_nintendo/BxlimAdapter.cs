using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.IO;
using Kontract.Interface;

namespace image_nintendo.BXLIM
{
    public class BxlimAdapter : IImageAdapter
    {
        private Cetera.Image.BXLIM _bxlim = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "BXLIM";
        public string Description => "Binary Layout Image";
        public string Extension => "*.bclim;*.bflim";
        public string About => "This is the BCLIM and BFLIM image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 40) return false;
                br.BaseStream.Seek((int)br.BaseStream.Length - 40, SeekOrigin.Begin);
                var magic = br.ReadString(4);
                return magic == "CLIM" || magic == "FLIM";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bxlim = new Cetera.Image.BXLIM(FileInfo.OpenRead());
                _bitmaps = new List<BitmapInfo> { new BxlimBitmapInfo { Bitmap = _bxlim.Image, Format = _bxlim.Settings.Format.FormatName } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            var info = _bitmaps[0] as BxlimBitmapInfo;
            _bxlim.Image = info.Bitmap;
            _bxlim.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }

    public sealed class BxlimBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public string Format { get; set; }
    }
}