using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_vxds
{
    public class MoflexAdapter : IImageAdapter
    {
        private VXDS _vxds = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "VXDS";
        public string Description => "VXDS Video format";
        public string Extension => "*.vx";
        public string About => "This is the VXDS image sequence adapter for Kukkii.";

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
                return br.ReadString(4) == "VXDS";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _vxds = new VXDS(FileInfo.OpenRead());

                _bitmaps = _vxds.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _vxds.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _vxds.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
