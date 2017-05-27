using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_ctxb
{
    public sealed class CtxbAdapter : IImageAdapter
    {
        private CTXB _ctxb = null;

        #region Properties

        public string Name => "CTXB";
        public string Description => "Whatever CTXB should mean";
        public string Extension => "*.ctxb";
        public string About => "This is the CTXB image adapter for Kukkii.";

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
                return br.ReadString(4) == "ctxb";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _ctxb = new CTXB(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                //_ctxb.Save(FileInfo.FullName, _ctxb.bmp);
            }
            catch (Exception) { }
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get => _ctxb.bmp;
            set => _ctxb.bmp = value;
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
