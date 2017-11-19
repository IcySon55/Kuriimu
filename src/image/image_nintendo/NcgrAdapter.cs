using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_nintendo.NCGLR
{
    [FilePluginMetadata(Name = "NCGR/NCLR", Description = "Nintendo Character Graphic Resource", Extension = "*.ncgr",
        Author = "onepiecefreak", About = "This is the NCGR/NCLR image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class BnrAdapter : IImageAdapter
    {
        private NCGLR _ncgr = null;
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
            var ncgrFilename = filename;
            var nclrFilename = filename.Remove(filename.Length - 2) + "lr";

            if (!File.Exists(ncgrFilename) || !File.Exists(nclrFilename)) return Identification.False;

            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length >= 4 && br.ReadString(4) == "RGCN") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
