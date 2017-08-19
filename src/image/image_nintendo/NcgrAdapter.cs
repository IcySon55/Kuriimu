using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CeteraDS.Hash;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace image_nintendo.NCGLR
{
    public sealed class BnrAdapter : IImageAdapter
    {
        private NCGLR _ncgr = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "NCGR/NCLR";
        public string Description => "Nintendo Character Graphic Resource";
        public string Extension => "*.ncgr";
        public string About => "This is the NCGR/NCLR image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var ncgrFilename = filename;
            var nclrFilename = filename.Remove(filename.Length - 2) + "lr";

            if (!File.Exists(ncgrFilename) || !File.Exists(nclrFilename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(ncgrFilename)))
            {
                return br.BaseStream.Length >= 4 && br.ReadString(4) == "RGCN";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            var ncgrFilename = filename;
            var nclrFilename = filename.Remove(filename.Length - 2) + "lr";
            var nscrFilename = filename.Remove(filename.Length - 3) + "scr";
            var ncerFilename = filename.Remove(filename.Length - 3) + "cer";

            if (FileInfo.Exists)
            {
                if (File.Exists(nscrFilename))
                    _ncgr = new NCGLR(File.OpenRead(ncgrFilename), File.OpenRead(nclrFilename), File.OpenRead(nscrFilename));
                else if (File.Exists(ncerFilename))
                    _ncgr = new NCGLR(File.OpenRead(ncgrFilename), File.OpenRead(nclrFilename), File.OpenRead(ncerFilename));
                else
                    _ncgr = new NCGLR(File.OpenRead(ncgrFilename), File.OpenRead(nclrFilename));


                _bitmaps = _ncgr.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _ncgr.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _ncgr.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
