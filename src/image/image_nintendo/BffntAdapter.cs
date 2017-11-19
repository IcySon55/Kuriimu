using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.IO;

namespace image_nintendo.BFFNT
{
    [FilePluginMetadata(Name = "BFFNT", Description = "Binary FoNT", Extension = "*.bffnt",
        Author = "", About = "This is the BFFNT image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class BffntAdapter : IImageAdapter
    {
        private BFFNT _bffnt;
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
                if (br.ReadString(4) == "FFNT") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bffnt = new BFFNT(File.OpenRead(FileInfo.FullName));
                _bitmaps = _bffnt.bmps.Select((o, i) => new BFFNTBitmapInfo { Bitmap = o, Format = _bffnt._settings[i].Format.FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            /*if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _bffnt.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _bffnt.Save(FileInfo.FullName);*/
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class BFFNTBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
