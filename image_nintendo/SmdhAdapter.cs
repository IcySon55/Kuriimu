using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_nintendo.ICN
{
    public sealed class IcnAdapter : IImageAdapter
    {
        private SMDH _icn;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "SMDH";
        public string Description => "SMDH Icon";
        public string Extension => "*.icn;*.bin";
        public string About => "This is the SMDH Icon image adapter for Kukkii.";

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
                return br.ReadString(4) == "SMDH";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _icn = new SMDH(FileInfo.OpenRead());
                _bitmaps = _icn.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            var stream = FileInfo.Create();
            try
            {
                _icn.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
                _icn.Save(stream);
            }
            catch (Exception)
            {
                stream.Close();
                FileInfo.Delete();
                throw;
            }
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
