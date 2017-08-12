using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace image_comp
{
    public sealed class CompAdapter : IImageAdapter
    {
        private COMP _comp = null;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "COMP";
        public string Description => "Generic image format";
        public string Extension => "*.comp";
        public string About => "This is the COMP image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadUInt32() + 0x10 == br.BaseStream.Length;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _comp = new COMP(FileInfo.OpenRead());

                _bitmaps = _comp.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _comp.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _comp.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
