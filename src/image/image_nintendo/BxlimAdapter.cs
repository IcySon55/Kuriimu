using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Komponent.IO;
using Kontract.Interface;

namespace image_nintendo.BXLIM
{
    [FilePluginMetadata(Name = "BXLIM", Description = "Binary Layout Image", Extension = "*.bclim;*.bflim",
        Author = "IcySon55,Neobeo,onepiecefreak", About = "This is the BXLIM image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class BxlimAdapter : IImageAdapter
    {
        private BXLIM _bxlim = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 40) return Identification.False;
                br.BaseStream.Seek((int)br.BaseStream.Length - 40, SeekOrigin.Begin);
                var magic = br.ReadString(4);
                if (magic == "CLIM" || magic == "FLIM") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bxlim = new BXLIM(FileInfo.OpenRead());
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

        public void New()
        {

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