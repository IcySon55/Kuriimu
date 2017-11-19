using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_mods
{
    [FilePluginMetadata(Name = "MODS", Description = "MODS Video format", Extension = "*.mods",
        Author = "onepiecefreak", About = "This is the MODS image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class MoflexAdapter : IImageAdapter
    {
        private MODS _mods = null;
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
                if (br.ReadString(4) == "MODS") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
