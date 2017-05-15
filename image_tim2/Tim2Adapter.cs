using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tim2
{
    public sealed class Tim2Adapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private TIM2 _tim2 = null;

        #region Properties

        public string Name => "TIM2";
        public string Description => "Texture IMage 2";
        public string Extension => "*.tm2";
        public string About => "This is the TIM2 file adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo
        {
            get
            {
                return _fileInfo;
            }
            set
            {
                _fileInfo = value;
            }
        }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "TIM2";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _tim2 = new TIM2(_fileInfo.FullName);
            else
                result = LoadResult.FileNotFound;

            return result;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (filename.Trim() != string.Empty)
                _fileInfo = new FileInfo(filename);

            try
            {
                //_tim2.Save(_fileInfo.FullName);
            }
            catch (Exception)
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get
            {
                return _tim2.bmp;
            }
            set
            {
                _tim2.bmp = value;
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
