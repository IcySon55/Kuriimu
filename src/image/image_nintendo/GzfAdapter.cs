using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace image_nintendo.GZF
{
    [FilePluginMetadata(Name = "GZF", Description = "GZ Font", Extension = "*.gzf",
        Author = "onepiecefreak", About = "This is the GZF image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class GzfAdapter : IImageAdapter
    {
        private GZF _gzf;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4) return Identification.False;
                if (br.ReadString(4) == "GZFX") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _gzf = new GZF(FileInfo.OpenRead());
                _bitmaps = _gzf.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            /*if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _gzf.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _gzf.Save(FileInfo.FullName);*/
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
