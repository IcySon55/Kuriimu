using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace image_nintendo.BCFNX
{
    [FilePluginMetadata(Name = "BCFNX", Description = "Binary CTR Font", Extension = "*.bcfnt;*.bcfna",
        Author = "", About = "This is the BCFNX image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class BcfnxAdapter : IImageAdapter
    {
        private BCFNT _bcfnx;
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
                string magic = br.ReadString(4);
                if (magic == "CFNT" || magic == "CFNA") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

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
