using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_mods
{
    public class MoflexAdapter : IImageAdapter
    {
        private MODS _mods = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "MODS";
        public string Description => "MODS Video format";
        public string Extension => "*.mods";
        public string About => "This is the MODS image sequence adapter for Kukkii.";

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
                return br.ReadString(4) == "MODS";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _mods = new MODS(FileInfo.OpenRead());

                _bitmaps = _mods.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _mods.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _mods.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
