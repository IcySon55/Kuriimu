using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tmx
{
    public sealed class TmxAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private TMX _tmx = null;

        #region Properties

        public string Name => "TMX";
        public string Description => "Atlus Texture Matrix";
        public string Extension => "*.tmx";
        public string About => "This is the TMX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

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
                if (br.BaseStream.Length < 12) return false;
                br.BaseStream.Position = 8;
                return br.ReadString(4) == "TMX0";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _tmx = new TMX(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
                _tmx.Save(_fileInfo.FullName);
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
                return _tmx.bmp;
            }
            set
            {
                _tmx.bmp = value;
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
