using System;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_jtex
{
    class JtexAdapter : IImageAdapter
    {
        private JTEX _jtex = null;
        private RawJTEX _rawjtex = null;
        private bool raw = false;

        #region Properties

        // Information
        public string Name => "JTEX";
        public string Description => "J Texture";
        public string Extension => "*.jtex";
        public string About => "This is the JTEX image adapter for Kukkii.";

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
                if (filename.Split('.')[filename.Split('.').Length - 1] == "jtex")
                {
                    raw = true;
                    return true;
                }
                return br.ReadString(4) == "jIMG";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                if (raw)
                    _rawjtex = new RawJTEX(FileInfo.OpenRead());
                else
                    _jtex = new JTEX(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                if (raw)
                    _rawjtex.Save(FileInfo.Create());
                else
                    _jtex.Save(FileInfo.Create());
            }
            catch (Exception) { }
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get => raw ? _rawjtex.Image : _jtex.Image;
            set
            {
                if (raw)
                    _rawjtex.Image = value;
                else
                    _jtex.Image = value;
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
