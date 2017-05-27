using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_nintendo.CTPK
{
    public sealed class CtpkAdapter : IImageAdapter
    {
        private CTPK _ctpk = null;

        #region Properties

        public string Name => "CTPK";
        public string Description => "CTR Texture PaCkage";
        public string Extension => "*.ctpk";
        public string About => "This is the CTPK image adapter for Kukkii.";

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
                return br.ReadString(4) == "CTPK" || Path.GetExtension(filename) == ".ctpk";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                using (var br = new BinaryReaderX(FileInfo.OpenRead()))
                    if (br.ReadString(4) == "CTPK")
                        _ctpk = new CTPK(FileInfo.FullName);
                    else
                        _ctpk = new CTPK(FileInfo.FullName, true);
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _ctpk.Save(FileInfo.FullName);
            }
            catch (Exception) { }
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get => _ctpk.bmp;
            set => _ctpk.bmp = value;
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
