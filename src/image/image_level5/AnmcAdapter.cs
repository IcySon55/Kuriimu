using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Komponent.IO;
using Kontract.Interface;
using System.IO;

namespace image_xi.ANMC
{
    [FilePluginMetadata(Name = "ANMC", Description = "Level 5 Animation File", Extension = "*.bin",
        Author = "onepiecefreak", About = "This is the ANMC image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class ImgaAdapter : IImageAdapter
    {
        private ANMC _anmc = null;
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
                if (br.BaseStream.Length < 4) return Identification.False;
                if (br.ReadString(4) == "ANMC") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _anmc = new ANMC(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _anmc.Bitmap } };
            }
        }

        public void Save(string filename = "")
        {
            /*if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _anmc = _bitmaps[0].Bitmap;
            ANMC.Save(File.Create(FileInfo.FullName));*/
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
