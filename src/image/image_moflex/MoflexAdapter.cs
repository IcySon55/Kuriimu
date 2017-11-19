using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_moflex
{
    [FilePluginMetadata(Name = "MOFLEX", Description = "Moflex Video format", Extension = "*.moflex",
        Author = "onepiecefreak", About = "This is the Moflex image sequence adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class MoflexAdapter : IImageAdapter
    {
        private MOFLEX _moflex = null;
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
                if (br.ReadUInt32() == 0xabaa324c) return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
