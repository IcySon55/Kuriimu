using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_bnr
{
    [FilePluginMetadata(Name = "BNR", Description = "BaNneR", Extension = "*.bnr", Author = "onepiecefreak",
        About = "This is the BNR image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class BnrAdapter : IImageAdapter
    {
        private BNR _bnr = null;
        private List<BitmapInfo> _bitmaps;
        private Import imports = new Import();

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            uint GetInt(byte[] ba) => ba.Aggregate(0u, (i, b) => (i << 8) | b);

            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 0x840) return Identification.False;
                br.BaseStream.Position = 2;
                var crc16 = br.ReadUInt16();

                br.BaseStream.Position = 0x20;
                if (GetInt(imports.crc16.Create(br.ReadBytes(0x820), 0)) == crc16) return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bnr = new BNR(FileInfo.OpenRead());

                var _bmpList = _bnr.bmps.Select(o => new BNRBitmapInfo { Bitmap = o, Format = _bnr.settings.Format.FormatName }).ToList();
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _bnr.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _bnr.Save(FileInfo.FullName, imports);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class BNRBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
