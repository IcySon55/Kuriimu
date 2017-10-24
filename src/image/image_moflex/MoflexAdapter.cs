using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_moflex
{
    public class MoflexAdapter : IImageAdapter
    {
        private MOFLEX _moflex = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "MOFLEX";
        public string Description => "Moflex Video format";
        public string Extension => "*.moflex;";
        public string About => "This is the Moflex image sequence adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadUInt32() == 0xabaa324c;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _moflex = new MOFLEX(FileInfo.OpenRead());

                _bitmaps = _moflex.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _moflex.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _moflex.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
