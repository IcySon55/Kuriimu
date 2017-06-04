using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;
using System.Linq;

namespace image_nintendo.ICN
{
    public sealed class IcnAdapter : IImageAdapter
    {
        private SMDH _icn = null;

        #region Properties

        public string Name => "SMDH";
        public string Description => "SMDH Icon";
        public string Extension => "*.icn";
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
                _icn = new SMDH(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _icn.Save(FileInfo.FullName);
            }
            catch (Exception) { }
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _icn.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();

        public bool ShowProperties(Icon icon) => false;
    }
}
