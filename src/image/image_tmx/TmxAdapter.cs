using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace image_tmx
{
    [FilePluginMetadata(Name = "TMX", Description = "Atlus Texture Matrix", Extension = "*.tmx",
        Author = "onepiecefreak", About = "This is the TMX image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class TmxAdapter : IImageAdapter
    {
        private TMX _tmx = null;
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
                if (br.BaseStream.Length < 12) return Identification.False;
                br.BaseStream.Position = 8;
                if (br.ReadString(4) == "TMX0") return Identification.True; ;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tmx = new TMX(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _tmx.bmp } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _tmx.bmp = _bitmaps[0].Bitmap;
            _tmx.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
