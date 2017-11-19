using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace image_nintendo.CGFX
{
    [FilePluginMetadata(Name = "CGFX", Description = "CTR GFX", Extension = "*.bcres;*.bcmdl;*.bctex",
        Author = "onepiecefreak", About = "This is the CGFX image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class CgfxAdapter : IImageAdapter
    {
        private CGFX _cgfx;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            try
            {
                using (var br = new BinaryReaderX(stream, true))
                {
                    if (br.ReadString(4) != "CGFX") return Identification.False;
                    br.BaseStream.Position = 0x24;
                    if (br.ReadUInt32() > 0) return Identification.True;
                }
            }
            catch
            {
                return Identification.False;
            }

            return Identification.False;
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

        public void New()
        {

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
