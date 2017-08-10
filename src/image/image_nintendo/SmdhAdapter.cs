using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_nintendo.SMDH
{
    public sealed class IcnAdapter : IImageAdapter
    {
        private SMDH _smdh;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "SMDH";
        public string Description => "SMDH Icon";
        public string Extension => "*.icn;*.bin";
        public string About => "This is the SMDH image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => true;
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
                _smdh = new SMDH(FileInfo.OpenRead());
                _bitmaps = _smdh.bmps.Select(o => new SMDHBitmapInfo { Bitmap = o, Format = Cetera.Image.Format.RGB565 }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            var stream = FileInfo.Create();
            try
            {
                _smdh.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
                _smdh.Save(stream);
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

        public bool ShowProperties(Icon icon)
        {
            var extendedProperties = new SmdhProperties(_smdh, icon);
            extendedProperties.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            extendedProperties.ShowDialog();
            return extendedProperties.HasChanges;
        }
    }
}
