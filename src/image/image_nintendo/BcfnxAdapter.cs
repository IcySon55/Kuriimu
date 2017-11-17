using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Cetera.Font;
using Kontract.Interface;
using Kontract.IO;

namespace image_nintendo.BCFNX
{
    public class BcfnxAdapter : IImageAdapter
    {
        private BCFNT _bcfnx;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "BCFNX";
        public string Description => "Binary CTR Font";
        public string Extension => "*.bcfnt;*.bcfna";
        public string About => "This is the BCFNT and BCFNA image adapter for Kukkii.";

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
                string magic = br.ReadString(4);
                return magic == "CFNT" || magic == "CFNA";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bcfnx = new BCFNT(FileInfo.OpenRead());

                var _bmpList = _bcfnx.bmps.Select((o, i) => new BCFNTBitmapInfo { Bitmap = o, Format = _bcfnx._settings[i].Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            /*if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _bcfnx.Save(FileInfo.Create());
            }
            catch (Exception) { }*/
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class BCFNTBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
