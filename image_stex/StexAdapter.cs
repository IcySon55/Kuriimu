using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_stex
{
    public sealed class StexAdapter : IImageAdapter
    {
        private STEX _stex = null;

        #region Properties

        public string Name => "STEX";
        public string Description => "Atlus Simple Texture";
        public string Extension => "*.stex";
        public string About => "This is the STEX image adapter for Kukkii.";

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
                return br.ReadString(4) == "STEX";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _stex = new STEX(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _stex.Save(FileInfo.FullName, _stex.bmp);
            }
            catch (Exception) { }
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get => _stex.bmp;
            set => _stex.bmp = value;
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
