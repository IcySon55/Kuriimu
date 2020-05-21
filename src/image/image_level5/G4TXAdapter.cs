using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace image_xi.G4TX
{
    public class G4TXAdapter : IImageAdapter
    {
        private G4TX _g4tx;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "G4TX";
        public string Description => "Level 5 Switch Texture Format";
        public string Extension => "*.g4tx";
        public string About => "This is the G4TX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4)
                    return false;
                return br.ReadString(4) == "G4TX";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _g4tx = new G4TX(FileInfo.OpenRead());

                _bitmaps = _g4tx.bmps.Cast<BitmapInfo>().ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _g4tx.bmps = _bitmaps.Cast<G4TXBitmapInfo>().ToList();
            _g4tx.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }

    // TODO: Mipmaps get ommitted; Fix in K2
    public class G4TXBitmapInfo : BitmapInfo
    {
        [Browsable(false)]
        public NXTCH TextureEntry { get; set; }
    }
}
