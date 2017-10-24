using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CeteraDS.Hash;
using Kontract.IO;
using Kontract.Interface;

namespace image_bnr
{
    public sealed class BnrAdapter : IImageAdapter
    {
        private BNR _bnr = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "BNR";
        public string Description => "BaNneR";
        public string Extension => "*.bnr";
        public string About => "This is the BNR image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                br.BaseStream.Position = 2;
                var crc16 = br.ReadUInt16();
                if (br.BaseStream.Length < 0x20) return false;
                if (br.BaseStream.Length < 0x820) return false;
                br.BaseStream.Position = 0x20;

                try
                {
                    return Crc16.Create(br.ReadBytes(0x820)) == crc16;
                }
                catch (Exception) { }

                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _bnr = new BNR(FileInfo.OpenRead());

                _bitmaps = _bnr.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _bnr.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _bnr.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
