using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_ctpk
{
    public sealed class CtpkAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private CTPK _stex = null;

        public string Name => "CTPK";
        public string Description => "CTR Texture PaCkage";
        public string Extension => "*.ctpk";
        public string About => "This is the CTPK file adapter for Kukkii.";

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

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "CTPK";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _stex = new CTPK(_fileInfo.FullName);
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
                //_stex.Save(_fileInfo.FullName, _stex.bmp);
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