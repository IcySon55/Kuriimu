using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace image_nintendo.BCH
{
    [FilePluginMetadata(Name = "BCH", Description = "BCH", Extension = "*.bch",
        Author = "onepiecefreak", About = "This is the BCH image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class BchAdapter : IImageAdapter
    {
        private BCH _bch;
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
                if (br.BaseStream.Length < 3) return Identification.False;
                if (br.ReadString(3) == "BCH") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bch = new BCH(FileInfo.FullName);

                _bitmaps = _bch.bmps.Select((o, i) => new BchBitmapInfo { Bitmap = o, Format = _bch._settings[i].Format.FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _bch.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _bch.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
