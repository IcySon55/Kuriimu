using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Linq;

namespace image_aa
{
    public sealed class CtxbAdapter : IImageAdapter
    {
        private AA _aa = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "AA";
        public string Description => "Ace Attorney Image";
        public string Extension => "*.bin";
        public string About => "This is the AA image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            /*using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 2) return false;
                var pre = br.ReadUInt16();
                return pre == 0 || pre == 0x83e0;
            }*/
            return false;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _aa = new AA(FileInfo.OpenRead());

                _bitmaps = _aa.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _aa.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _aa.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
