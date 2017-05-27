using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tex
{
    public class TexAdapter : IImageAdapter
    {
        private TEX _tex = null;

        #region Properties

        // Information
        public string Name => "TEX";
        public string Description => "MT Framework Texture";
        public string Extension => "*.tex";
        public string About => "This is the MT Framework TEX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(3) == "TEX";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _tex = new TEX(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                //_tex.Save(FileInfo.Create());
            }
            catch (Exception) { }
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get => _tex.Image;
            set => _tex.Image = value;
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
