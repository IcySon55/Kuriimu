using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_nintendo.CGFX
{
    public class BcfnxAdapter : IImageAdapter
    {
        private CGFX _cgfx;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "CGFX";
        public string Description => "CTR GFX";
        public string Extension => "*.bcres;*.bcmdl;*.bctex";
        public string About => "This is the CGFX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            try
            {
                using (var br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    if (br.ReadString(4) != "CGFX") return false;
                    br.BaseStream.Position = 0x24;
                    return br.ReadUInt32() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _cgfx = new CGFX(FileInfo.OpenRead());
                _bitmaps = _cgfx.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            //_cgfx.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
