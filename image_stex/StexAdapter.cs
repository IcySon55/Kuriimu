using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_stex
{
    public sealed class StexAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private STEX _stex = null;

        public string Name => "STEX";
        public string Description => "Atlus Simple Texture";
        public string Extension => "*.stex";
        public string About => "This is the STEX file adapter for Kukkii.";

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

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "STEX";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _stex = new STEX(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
                _stex.Save(_fileInfo.FullName, _stex.bmp);
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
                return _stex.bmp;
            }
            set
            {
                _stex.bmp = value;
            }
        }
    }
}