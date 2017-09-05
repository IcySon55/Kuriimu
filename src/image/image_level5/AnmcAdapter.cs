using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kuriimu.IO;
using Kuriimu.Kontract;
using System.IO;

namespace image_xi.ANMC
{
    public sealed class ImgaAdapter : IImageAdapter
    {
        private Bitmap _anmc = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "ANMC";
        public string Description => "Level 5 Animation File";
        public string Extension => "*.bin";
        public string About => "This is the ANMC image adapter for Kukkii.";

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
                return br.ReadString(4) == "ANMC";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _anmc = ANMC.Load(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _anmc } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _anmc = _bitmaps[0].Bitmap;
            ANMC.Save(File.Create(FileInfo.FullName));
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
