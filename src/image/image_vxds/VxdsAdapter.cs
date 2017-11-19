using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_vxds
{
    [FilePluginMetadata(Name = "VXDS", Description = "VXDS Video format", Extension = "*.vx",
        Author = "onepiecefreak", About = "This is the VXDS image sequence adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class MoflexAdapter : IImageAdapter
    {
        private VXDS _vxds = null;
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
                if (br.ReadString(4) == "VXDS") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
