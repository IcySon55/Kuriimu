using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Kontract.IO;

namespace image_nintendo.CGFX
{
    public class CgfxAdapter : IImageAdapter
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
        public bool CanSave => true;

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

                var _bmpList = _cgfx.bmps.Select((o, i) => new CGFXBitmapInfo { Bitmap = o, Format = _cgfx._settings[i].Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _cgfx.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _cgfx.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class CGFXBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
