using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Windows.Forms;

namespace image_rawJtex
{
    public class RawJtexAdapter : IImageAdapter
    {
        private RawJTEX _rawjtex;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        // Information
        public string Name => "RawJTEX";
        public string Description => "J Texture without header";
        public string Extension => "*.jtex";
        public string About => "This is the RawJTEX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (Path.GetExtension(filename) == ".jtex" && br.PeekString(4) != "jIMG" && br.ReadByte() == 0x11)
                {
                    MessageBox.Show("This headerless jTEX seems compressed!", "Compressed", MessageBoxButtons.OK);
                    return false;
                }

                return (Path.GetExtension(filename) == ".jtex" && br.PeekString(4) != "jIMG" && br.ReadByte() != 0x11);
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _rawjtex = new RawJTEX(FileInfo.OpenRead());

                var _bmpList = new List<BitmapInfo> { new RawJTEXBitmapInfo { Bitmap = _rawjtex.Image, Format = _rawjtex.settings.Format.FormatName } };
                _bitmaps = new List<BitmapInfo>();
                _bitmaps.AddRange(_bmpList);
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _rawjtex.Image = _bitmaps[0].Bitmap;
            _rawjtex.Save(FileInfo.Create());
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;

        public sealed class RawJTEXBitmapInfo : BitmapInfo
        {
            [Category("Properties")]
            [ReadOnly(true)]
            public string Format { get; set; }
        }
    }
}
