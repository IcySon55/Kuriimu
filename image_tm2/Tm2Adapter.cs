using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tm2
{
    public sealed class Tm2Adapter : IImageAdapter
    {
        private TM2 _tm2 = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "TM2";
        public string Description => "Texture Matrix 2";
        public string Extension => "*.bip";
        public string About => "This is the TM2 image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                try
                {
                    if (br.BaseStream.Length < 4) return false;
                    int count = br.ReadInt32();
                    if (br.BaseStream.Length < count * 0x4) return false;
                    br.BaseStream.Position = (count - 1) * 0x4;
                    int off = br.ReadInt32();
                    if (br.BaseStream.Length < off + 8) return false;
                    br.BaseStream.Position = off;
                    return (br.ReadString(8) == "EMUARC__");
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tm2 = new TM2(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _tm2.bmp } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _tm2.bmp = _bitmaps[0].Bitmap;
            //_tm2.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
