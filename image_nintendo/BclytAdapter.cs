using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_nintendo.BCLYT
{
    public class BclytAdapter : IImageAdapter
    {
        private Bitmap _bclyt = null;

        #region Properties

        public string Name => "BCLYT";
        public string Description => "Standard Nintendo Layout format";
        public string Extension => "*.bclyt";
        public string About => "This is the BCLYT image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                return br.ReadString(4) == "CLYT";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _bclyt = BCLYT.Load(FileInfo.OpenRead(), filename);
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                //_bclyt.Save(FileInfo.FullName);
            }
            catch (Exception) { }
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get => _bclyt;
            set => _bclyt = value;
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
